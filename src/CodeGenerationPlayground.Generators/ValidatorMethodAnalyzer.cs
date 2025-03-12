using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CodeGenerationPlayground.Generators;

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

    private static readonly DiagnosticDescriptor nullValidatorMethodDescriptor = new(
        id: "CGP006",
        title: "Null validator method",
        messageFormat: "Property '{0}' needs to specify a validator method",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor validatorMethodNotFoundDescriptor = new(
        id: "CGP007",
        title: "Validator method not found",
        messageFormat: "Can not find method specified by property '{0}'",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        propertyNotOwnedByTypeDescriptor,
        nullValidatorMethodDescriptor,
        validatorMethodNotFoundDescriptor
    );

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyseProperty, SyntaxKind.PropertyDeclaration);
    }

    private void AnalyseProperty(SyntaxNodeAnalysisContext context) {
        if (context.Node is not PropertyDeclarationSyntax propertyDeclarationSyntax) {
            return;
        }

        if (!propertyDeclarationSyntax.AttributeLists
            .SelectMany(attributeList => attributeList.Attributes)
            .Any(attribute => IsValidatorMethodAttribute(context, attribute))) {
            return;
        }

        var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax);

        if (propertySymbol == null) {
            return;
        }

        if (propertyDeclarationSyntax.Parent is not TypeDeclarationSyntax typeDeclarationSyntax) {
            context.ReportDiagnostic(CreateDiagnostic(propertyNotOwnedByTypeDescriptor, propertyDeclarationSyntax));
            return;
        }

        var methodName = GetValidatorMethodName(propertySymbol);

        if (methodName == null) {
            context.ReportDiagnostic(CreateDiagnostic(nullValidatorMethodDescriptor, propertyDeclarationSyntax));
            return;
        }

        var candidateMethodDeclarations = GetCandidateMethodDeclarations(typeDeclarationSyntax, methodName);

        if (candidateMethodDeclarations.Count == 0) {
            context.ReportDiagnostic(CreateDiagnostic(validatorMethodNotFoundDescriptor, propertyDeclarationSyntax));
        }
    }

    private bool IsValidatorMethodAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax) {
        if (context.SemanticModel.GetSymbolInfo(attributeSyntax, context.CancellationToken).Symbol is IMethodSymbol methodSymbol) {
            return methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == ValidatorMethodConstants.GlobalFullyQualifiedAttributeName;
        }

        return false;
    }

    private static Diagnostic CreateDiagnostic(DiagnosticDescriptor diagnosticDescriptor, PropertyDeclarationSyntax propertyDeclarationSyntax)
        => Diagnostic.Create(
            diagnosticDescriptor,
            propertyDeclarationSyntax.GetLocation(),
            propertyDeclarationSyntax.Identifier.Text
        );

    private string? GetValidatorMethodName(IPropertySymbol propertySymbol) {
        foreach (var attributeData in propertySymbol.GetAttributes()) {
            if (attributeData.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == ValidatorMethodConstants.GlobalFullyQualifiedAttributeName
                && attributeData.ConstructorArguments.Length > 0) {

                return attributeData.ConstructorArguments[0].Value?.ToString();
            }
        }

        return null;
    }

    private List<MethodDeclarationSyntax> GetCandidateMethodDeclarations(TypeDeclarationSyntax typeDeclarationSyntax, string methodName)
        => typeDeclarationSyntax.Members
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.Text == methodName)
            .ToList();
}
