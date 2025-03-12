using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

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

    private static readonly DiagnosticDescriptor missingValidatorMethodDescriptor = new(
        id: "CGP006",
        title: "Missing validator method",
        messageFormat: "Property '{0}' needs to specify a validator method",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        propertyNotOwnedByTypeDescriptor,
        missingValidatorMethodDescriptor
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

        if (propertyDeclarationSyntax.Parent is not TypeDeclarationSyntax) {
            context.ReportDiagnostic(CreateDiagnostic(propertyNotOwnedByTypeDescriptor, propertyDeclarationSyntax, propertyDeclarationSyntax.Identifier.Text));
        }

        var methodName = GetValidatorMethodName(propertySymbol);

        if (methodName == null) {
            context.ReportDiagnostic(CreateDiagnostic(missingValidatorMethodDescriptor, propertyDeclarationSyntax, propertyDeclarationSyntax?.Identifier.Text));
            return;
        }


    }

    private bool IsValidatorMethodAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax) {
        if (context.SemanticModel.GetSymbolInfo(attributeSyntax, context.CancellationToken).Symbol is IMethodSymbol methodSymbol) {
            return methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == ValidatorMethodConstants.GlobalFullyQualifiedAttributeName;
        }

        return false;
    }

    private static Diagnostic CreateDiagnostic(DiagnosticDescriptor diagnosticDescriptor, PropertyDeclarationSyntax propertyDeclarationSyntax, params object?[] messageArgs)
        => Diagnostic.Create(
            diagnosticDescriptor,
            propertyDeclarationSyntax.GetLocation(),
            messageArgs
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
}
