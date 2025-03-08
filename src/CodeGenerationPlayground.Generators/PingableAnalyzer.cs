using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace CodeGenerationPlayground.Generators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PingableAnalyzer : DiagnosticAnalyzer {
    private static readonly DiagnosticDescriptor methodMissingPartialModifierDescriptor = new(
        id: "CGP001",
        title: "Method is missing 'partial' modifier",
        messageFormat: "Method '{0}' is missing required modifier 'partial'",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor methodDoesNotReturnStringDescriptor = new(
        id: "CGP002",
        title: "Method does not return 'string'",
        messageFormat: "Method '{0}' needs to have return type 'string'",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor methodNotOwnedByTypeDescriptor = new(
        id: "CGP003",
        title: "Method is not owned by a type",
        messageFormat: "Method '{0}' needs to be part of a class, struct, record or interface",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor methodHasParametersDescriptor = new(
        id: "CGP004",
        title: "Method has parameters",
        messageFormat: "Method '{0}' needs to be parameterless",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        methodMissingPartialModifierDescriptor,
        methodDoesNotReturnStringDescriptor,
        methodNotOwnedByTypeDescriptor,
        methodHasParametersDescriptor
    );

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

        if (methodDeclarationSyntax.Parent is not TypeDeclarationSyntax) {
            context.ReportDiagnostic(CreateDiagnostic(methodNotOwnedByTypeDescriptor, methodDeclarationSyntax));
        }

        if (!methodDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))) {
            context.ReportDiagnostic(CreateDiagnostic(methodMissingPartialModifierDescriptor, methodDeclarationSyntax));
        }

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);

        if (methodSymbol != null && methodSymbol.ReturnType.SpecialType != SpecialType.System_String) {
            context.ReportDiagnostic(CreateDiagnostic(methodDoesNotReturnStringDescriptor, methodDeclarationSyntax));
        }

        if (methodDeclarationSyntax.ParameterList.Parameters.Count != 0) {
            context.ReportDiagnostic(CreateDiagnostic(methodHasParametersDescriptor, methodDeclarationSyntax));
        }
    }

    private static Diagnostic CreateDiagnostic(DiagnosticDescriptor diagnosticDescriptor, MethodDeclarationSyntax methodDeclarationSyntax)
        => Diagnostic.Create(
            diagnosticDescriptor,
            methodDeclarationSyntax.GetLocation(),
            methodDeclarationSyntax.Identifier.Text
        );

    private bool IsPingableAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax) {
        if (context.SemanticModel.GetSymbolInfo(attributeSyntax, context.CancellationToken).Symbol is IMethodSymbol methodSymbol) {
            return methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).EndsWith(PingableConstants.GlobalFullyQualifiedAttributeName);
        }

        return false;
    }
}
