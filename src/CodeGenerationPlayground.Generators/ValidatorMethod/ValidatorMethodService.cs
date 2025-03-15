using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public class ValidatorMethodService {
    private readonly PropertyDeclarationSyntax? propertyDeclarationSyntax;

    public bool IsProperty => propertyDeclarationSyntax != null;

    public ValidatorMethodService(/*SemanticModel semanticModel, */SyntaxNode node) {
        propertyDeclarationSyntax = node as PropertyDeclarationSyntax;
    }
}
