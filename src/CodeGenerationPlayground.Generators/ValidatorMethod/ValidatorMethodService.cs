using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
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

    public List<ValidatorMethodData> GetValidatorMethodData() {
        var validatorMethodData = new List<ValidatorMethodData>();

        if (propertySymbol != null) {
            foreach (var attributeData in propertySymbol.GetAttributes()) {
                if (attributeData.AttributeClass.HasName(ValidatorMethodConstants.GlobalFullyQualifiedAttributeName) && symbolProvider.TryGetConstructorArgumentValue(attributeData, 0, out var validatorMethod)) {
                    validatorMethodData.Add(new ValidatorMethodData(validatorMethod));
                }
            }
        }

        return validatorMethodData;
    }
}
