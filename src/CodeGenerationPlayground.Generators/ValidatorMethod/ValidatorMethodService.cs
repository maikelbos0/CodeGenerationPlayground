using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public class ValidatorMethodService {
    private readonly PropertyDeclarationSyntax? propertyDeclarationSyntax;
    private readonly TypeDeclarationSyntax? typeDeclarationSyntax;
    private readonly bool hasValidatorMethodAttributes;

    public bool IsProperty => propertyDeclarationSyntax != null;
    public bool HasValidParent => typeDeclarationSyntax != null;
    public bool HasValidatorMethodAttributes => hasValidatorMethodAttributes;

    public ValidatorMethodService(ISymbolProvider symbolProvider, SyntaxNode node, CancellationToken cancellationToken) {
        if ((propertyDeclarationSyntax = node as PropertyDeclarationSyntax) == null || (typeDeclarationSyntax = node.Parent as TypeDeclarationSyntax) == null) {
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
}
