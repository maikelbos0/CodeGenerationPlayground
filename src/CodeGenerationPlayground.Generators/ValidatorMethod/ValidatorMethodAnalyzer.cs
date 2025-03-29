using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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

    private static readonly DiagnosticDescriptor multipleValidatorMethodsDescriptor = new(
        id: "CGP008",
        title: "Multiple validator methods found",
        messageFormat: "Multiple validator methods found named '{1}' specified by property '{0}'; only one validator method can be specified",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor validatorMethodIsNotAccessibleDescriptor = new(
        id: "CGP009",
        title: "Validator method is not accessible",
        messageFormat: "Validator method '{1}' specified by property '{0}' is not accessible; it should either be 'public' or 'internal'",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor validatorMethodCannotBeGenericDescriptor = new(
        id: "CGP010",
        title: "Validator method cannot be generic",
        messageFormat: "Validator method '{1}' specified by property '{0}' cannot be generic",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        propertyNotOwnedByTypeDescriptor,
        validatorMethodNotFoundDescriptor,
        validatorMethodSignatureIsInvalidDescriptor,
        multipleValidatorMethodsDescriptor,
        validatorMethodIsNotAccessibleDescriptor,
        validatorMethodCannotBeGenericDescriptor
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

        if (!service.HasValidParent) {
            context.ReportDiagnostic(service.CreateDiagnostic(propertyNotOwnedByTypeDescriptor, null));
            return;
        }

        foreach (var validatorMethodData in service.GetValidatorMethodData(context.CancellationToken)) {
            if (validatorMethodData.MethodName == null) {
                context.ReportDiagnostic(service.CreateDiagnostic(validatorMethodNotFoundDescriptor, validatorMethodData.MethodName));
                return;
            }

            if (validatorMethodData.MethodCandidates.Length == 0) {
                context.ReportDiagnostic(service.CreateDiagnostic(validatorMethodNotFoundDescriptor, validatorMethodData.MethodName));
                return;
            }

            var validMethodCandidates = validatorMethodData.GetValidMethodCandidates();

            if (validMethodCandidates.Length == 0) {
                if (validatorMethodData.MethodCandidates.Any(validatorMethodCandidate => !validatorMethodCandidate.HasValidSignature)) {
                    context.ReportDiagnostic(service.CreateDiagnostic(validatorMethodSignatureIsInvalidDescriptor, validatorMethodData.MethodName));
                }

                if (validatorMethodData.MethodCandidates.Any(validatorMethodCandidate => !validatorMethodCandidate.IsAccessible)) {
                    context.ReportDiagnostic(service.CreateDiagnostic(validatorMethodIsNotAccessibleDescriptor, validatorMethodData.MethodName));
                }

                if (validatorMethodData.MethodCandidates.Any(validatorMethodCandidate => validatorMethodCandidate.IsGeneric)) {
                    context.ReportDiagnostic(service.CreateDiagnostic(validatorMethodCannotBeGenericDescriptor, validatorMethodData.MethodName));
                }
            }
            else if (validMethodCandidates.Length > 1) {
                context.ReportDiagnostic(service.CreateDiagnostic(multipleValidatorMethodsDescriptor, validatorMethodData.MethodName));
            }
        }
    }
}
