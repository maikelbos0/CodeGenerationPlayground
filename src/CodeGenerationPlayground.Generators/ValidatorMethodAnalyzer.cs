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
    private static readonly DiagnosticDescriptor methodNotOwnedByTypeDescriptor = new(
        id: "CGP005",
        title: "Property is not owned by a type",
        messageFormat: "Property '{0}' needs to be part of a class, struct, record or interface",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw new System.NotImplementedException();

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

        if (propertyDeclarationSyntax.Parent is not TypeDeclarationSyntax) {
            context.ReportDiagnostic(CreateDiagnostic(methodNotOwnedByTypeDescriptor, propertyDeclarationSyntax));
        }
    }

    private static Diagnostic CreateDiagnostic(DiagnosticDescriptor diagnosticDescriptor, PropertyDeclarationSyntax propertyDeclarationSyntax)
        => Diagnostic.Create(
            diagnosticDescriptor,
            propertyDeclarationSyntax.GetLocation(),
            propertyDeclarationSyntax.Identifier.Text
        );

    private bool IsValidatorMethodAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax) {
        if (context.SemanticModel.GetSymbolInfo(attributeSyntax, context.CancellationToken).Symbol is IMethodSymbol methodSymbol) {
            return methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == ValidatorMethodConstants.GlobalFullyQualifiedAttributeName;
        }

        return false;
    }
}
