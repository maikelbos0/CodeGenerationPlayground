using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValidatorMethodAnalyzer : DiagnosticAnalyzer {
    private static readonly DiagnosticDescriptor propertyNotOwnedByTypeDescriptor = new(
        id: "CGP005",
        title: "Property is not owned by a type",
        messageFormat: "Property '{0}' needs to be part of a class, struct, record or interface",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor validatorMethodNotFoundDescriptor = new(
        id: "CGP006",
        title: "Validator method not found",
        messageFormat: "Can not find validator method '{1}' specified by property '{0}'",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor validatorMethodSignatureIsInvalidDescriptor = new(
        id: "CGP007",
        title: "Validator method does not have a valid signature",
        messageFormat: "Validator method '{1}' specified by property '{0}' needs to have return type 'bool' and only accept parameters of type 'object?' and 'ValidationContext'",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        propertyNotOwnedByTypeDescriptor,
        validatorMethodNotFoundDescriptor,
        validatorMethodSignatureIsInvalidDescriptor
    );

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyseProperty, SyntaxKind.PropertyDeclaration);
    }

    private void AnalyseProperty(SyntaxNodeAnalysisContext context) {
        var service = new ValidatorMethodService(new SymbolProvider(context.SemanticModel), context.Node, context.CancellationToken);

        if (!service.IsProperty || !service.HasValidatorMethodAttributes || !service.HasPropertySymbol) {
            return;
        }

        // We can get this from the service if we need to but currently I don't think it's needed; let's first move all logic to the service and then see
        var propertyDeclarationSyntax = (PropertyDeclarationSyntax)context.Node;
        var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax, context.CancellationToken)!;
        var typeDeclarationSyntax = (TypeDeclarationSyntax)context.Node.Parent!;

        if (!service.HasValidParent) {
            context.ReportDiagnostic(CreateDiagnostic(propertyNotOwnedByTypeDescriptor, propertyDeclarationSyntax, null));
            return;
        }

        foreach (var validatorMethodData in service.GetValidatorMethodData(context.CancellationToken)) {
            if (validatorMethodData.Name == null) {
                context.ReportDiagnostic(CreateDiagnostic(validatorMethodNotFoundDescriptor, propertyDeclarationSyntax, validatorMethodData.Name));
                return;
            }

            var candidateMethodDeclarations = GetCandidateMethodDeclarations(typeDeclarationSyntax, validatorMethodData.Name);

            if (candidateMethodDeclarations.Count == 0) {
                context.ReportDiagnostic(CreateDiagnostic(validatorMethodNotFoundDescriptor, propertyDeclarationSyntax, validatorMethodData.Name));
                return;
            }

            var candidateMethodSymbols = candidateMethodDeclarations
                .Select(candidateMethodDeclaration => context.SemanticModel.GetDeclaredSymbol(candidateMethodDeclaration))
                .Where(candidateMethodSymbol => candidateMethodSymbol != null)
                .Select(candidateMethodSymbol => candidateMethodSymbol!)
                .ToList();

            // TODO needs to check for 2+ also, different error message
            if (candidateMethodSymbols.Count(IsValidatorMethod) != 1) {
                context.ReportDiagnostic(CreateDiagnostic(validatorMethodSignatureIsInvalidDescriptor, propertyDeclarationSyntax, validatorMethodData.Name));
            }
            
            // TOOD handle new checks with diagnostics
        }
    }

    private static Diagnostic CreateDiagnostic(DiagnosticDescriptor diagnosticDescriptor, PropertyDeclarationSyntax propertyDeclarationSyntax, string? methodName)
        => Diagnostic.Create(
            diagnosticDescriptor,
            propertyDeclarationSyntax.GetLocation(),
            propertyDeclarationSyntax.Identifier.Text,
            methodName
        );

    private List<MethodDeclarationSyntax> GetCandidateMethodDeclarations(TypeDeclarationSyntax typeDeclarationSyntax, string methodName)
        => typeDeclarationSyntax.Members
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.Text == methodName)
            .ToList();

    private bool IsValidatorMethod(IMethodSymbol candidateMethodSymbol) {
        if (candidateMethodSymbol.ReturnType.SpecialType != SpecialType.System_Boolean) {
            return false;
        }

        var validParameters = 0;

        if (candidateMethodSymbol.Parameters.Length > 2) {
            return false;
        }

        if (candidateMethodSymbol.Parameters.Any(parameterSymbol => parameterSymbol.Type.SpecialType == SpecialType.System_Object && parameterSymbol.NullableAnnotation != NullableAnnotation.NotAnnotated)) {
            validParameters++;
        }

        if (candidateMethodSymbol.Parameters.Any(parameterSymbol => parameterSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == ValidatorMethodConstants.GlobalFullyQualifiedValidationContextTypeName)) {
            validParameters++;
        }

        return validParameters == candidateMethodSymbol.Parameters.Length;
    }
}
