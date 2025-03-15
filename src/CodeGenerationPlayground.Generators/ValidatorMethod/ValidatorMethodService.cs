using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Threading;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public class ValidatorMethodService {
    private readonly PropertyDeclarationSyntax? propertyDeclarationSyntax;
    private readonly TypeDeclarationSyntax? typeDeclarationSyntax;

    public bool IsProperty => propertyDeclarationSyntax != null;
    public bool HasValidParent => typeDeclarationSyntax != null;
    public bool HasValidatorMethodAttributes { get; }

    public ValidatorMethodService(ISymbolProvider symbolProvider, SyntaxNode node, CancellationToken cancellationToken) {
        propertyDeclarationSyntax = node as PropertyDeclarationSyntax;

        if (propertyDeclarationSyntax == null) {
            return;
        }

        typeDeclarationSyntax = node.Parent as TypeDeclarationSyntax;

        if (typeDeclarationSyntax == null) {
            return;
        }

        this.HasValidatorMethodAttributes = HasValidatorMethodAttributes(propertyDeclarationSyntax);

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
