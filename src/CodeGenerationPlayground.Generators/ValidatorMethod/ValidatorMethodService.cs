using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public class ValidatorMethodService {
    private readonly PropertyDeclarationSyntax? propertyDeclarationSyntax;
    private readonly TypeDeclarationSyntax? typeDeclarationSyntax;
    private readonly bool hasValidatorMethodAttributes;
    private readonly IPropertySymbol? propertySymbol;
    private readonly ISymbolProvider symbolProvider;

    public bool IsProperty => propertyDeclarationSyntax != null;
    public bool HasValidParent => typeDeclarationSyntax != null;
    public bool HasValidatorMethodAttributes => hasValidatorMethodAttributes;
    public bool HasPropertySymbol => propertySymbol != null;

    public ValidatorMethodService(ISymbolProvider symbolProvider, SyntaxNode node, CancellationToken cancellationToken) {
        this.symbolProvider = symbolProvider;
        propertyDeclarationSyntax = node as PropertyDeclarationSyntax;

        if (propertyDeclarationSyntax == null) {
            return;
        }

        typeDeclarationSyntax = node.Parent as TypeDeclarationSyntax;

        if (typeDeclarationSyntax == null) {
            return;
        }

        propertySymbol = symbolProvider.GetPropertySymbol(propertyDeclarationSyntax, cancellationToken);

        if (propertySymbol == null) {
            return;
        }

        hasValidatorMethodAttributes = HasValidatorMethodAttributes(propertyDeclarationSyntax);

        bool HasValidatorMethodAttributes(PropertyDeclarationSyntax propertyDeclarationSyntax) {
            foreach (var attributeListSyntax in propertyDeclarationSyntax.AttributeLists) {
                foreach (var attributeSyntax in attributeListSyntax.Attributes) {
                    if (symbolProvider.GetSymbol(attributeSyntax, cancellationToken) is IMethodSymbol methodSymbol && methodSymbol.ContainingType.HasName(ValidatorMethodConstants.GlobalFullyQualifiedAttributeName)) {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public List<ValidatorMethodData> GetValidatorMethodData(CancellationToken cancellationToken) {
        var validatorMethodData = new List<ValidatorMethodData>();

        if (propertySymbol != null) {
            foreach (var attributeData in propertySymbol.GetAttributes()) {
                if (attributeData.AttributeClass.HasName(ValidatorMethodConstants.GlobalFullyQualifiedAttributeName) && symbolProvider.TryGetConstructorArgumentValue(attributeData, 0, out var validatorMethod)) {
                    validatorMethodData.Add(new ValidatorMethodData(validatorMethod, GetCandidateMethodDeclarations(validatorMethod, cancellationToken)));
                }
            }
        }

        return validatorMethodData;
    }

    private ImmutableArray<ValidatorMethodCandidateData> GetCandidateMethodDeclarations(string? methodName, CancellationToken cancellationToken) {
        var candidateMethodDeclarations = new List<ValidatorMethodCandidateData>();

        if (typeDeclarationSyntax != null) {
            foreach (var member in typeDeclarationSyntax.Members) {
                if (member is MethodDeclarationSyntax methodDeclarationSyntax && methodDeclarationSyntax.Identifier.Text == methodName) {
                    var candidateMethodSymbol = symbolProvider.GetMethodSymbol(methodDeclarationSyntax, cancellationToken);

                    if (candidateMethodSymbol == null) {
                        break;
                    }

                    var isAccessible = false;
                    foreach (var modifier in methodDeclarationSyntax.Modifiers) { 
                        if (modifier.IsKind(SyntaxKind.PublicKeyword) || modifier.IsKind(SyntaxKind.InternalKeyword)) {
                            isAccessible = true;
                            break;
                        }
                    }

                    var hasValidSignature = candidateMethodSymbol.ReturnType.SpecialType == SpecialType.System_Boolean && candidateMethodSymbol.Parameters.Length <= 2;
                    var firstParameterType = candidateMethodSymbol.Parameters.Length > 0 ? GetParamaterType(candidateMethodSymbol.Parameters[0]) : ParameterType.None;
                    var secondParameterType = candidateMethodSymbol.Parameters.Length > 1 ? GetParamaterType(candidateMethodSymbol.Parameters[1]) : ParameterType.None;

                    if (firstParameterType == ParameterType.Invalid || secondParameterType == ParameterType.Invalid) {
                        hasValidSignature = false;
                    }
                    
                    if (firstParameterType != ParameterType.None && firstParameterType == secondParameterType) {
                        hasValidSignature = false;
                    }

                    candidateMethodDeclarations.Add(new(firstParameterType, secondParameterType, hasValidSignature, isAccessible));
                }
            }
        }

        return ImmutableArray.CreateRange(candidateMethodDeclarations);
    }

    private ParameterType GetParamaterType(IParameterSymbol parameterSymbol) {
        if (parameterSymbol.Type.SpecialType == SpecialType.System_Object && parameterSymbol.NullableAnnotation != NullableAnnotation.NotAnnotated) {
            return ParameterType.Object;
        }
        else if (parameterSymbol.Type.HasName(ValidatorMethodConstants.GlobalFullyQualifiedValidationContextTypeName)) {
            return ParameterType.ValidationContext;
        }
        else {
            return ParameterType.Invalid;
        }
    }
}
