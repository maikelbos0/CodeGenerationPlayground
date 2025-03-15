using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public class ValidatorMethodService {
    private readonly PropertyDeclarationSyntax? propertyDeclarationSyntax;
    private readonly TypeDeclarationSyntax? typeDeclarationSyntax;

    public bool IsProperty => propertyDeclarationSyntax != null;
    public bool HasValidParent => typeDeclarationSyntax != null;
    
    public ValidatorMethodService(SyntaxNode node) {
        propertyDeclarationSyntax = node as PropertyDeclarationSyntax;
        typeDeclarationSyntax = node.Parent as TypeDeclarationSyntax;
    }
}
