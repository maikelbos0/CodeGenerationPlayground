using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace CodeGenerationPlayground.Generators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PingableAnalyzer : DiagnosticAnalyzer {
    //private static SymbolDisplayFormat fullyQualifiedFormat = new()

    private static readonly DiagnosticDescriptor methodMissingPartialModifierDescriptor = new(
        id: "CGP001",
        title: "Method is missing 'partial' modifier",
        messageFormat: "Method '{0}' is missing the required modifier 'partial'",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(methodMissingPartialModifierDescriptor);

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context) {
        if (context.Node is not MethodDeclarationSyntax methodDeclarationSyntax) {
            return;
        }

        if (!methodDeclarationSyntax.AttributeLists
            .SelectMany(attributeList => attributeList.Attributes)
            .Any(attribute => IsPingableAttribute(context, attribute))) {
            return;
        }

        if (!methodDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))) {
            var location = methodDeclarationSyntax.Identifier.GetLocation();
            var diagnostic = Diagnostic.Create(methodMissingPartialModifierDescriptor, location, methodDeclarationSyntax.Identifier.Text);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private bool IsPingableAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax) {
        if (context.SemanticModel.GetSymbolInfo(attributeSyntax, context.CancellationToken).Symbol is IMethodSymbol methodSymbol) {
            return methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).EndsWith(PingableConstants.GlobalFullyQualifiedAttributeName);
        }

        return false;
    }
}
