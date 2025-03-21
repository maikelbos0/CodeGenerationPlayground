using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public class SymbolProvider : ISymbolProvider {
    private readonly SemanticModel semanticModel;

    public SymbolProvider(SemanticModel semanticModel) {
        this.semanticModel = semanticModel;
    }

    public IPropertySymbol? GetPropertySymbol(PropertyDeclarationSyntax propertyDeclarationSyntax, CancellationToken cancellationToken)
        => semanticModel.GetDeclaredSymbol(propertyDeclarationSyntax, cancellationToken);

    public IMethodSymbol? GetMethodSymbol(MethodDeclarationSyntax methodDeclarationSyntax, CancellationToken cancellationToken) 
        => semanticModel.GetDeclaredSymbol(methodDeclarationSyntax, cancellationToken);

    public ISymbol? GetSymbol(AttributeSyntax attributeSyntax, CancellationToken cancellationToken)
        => semanticModel.GetSymbolInfo(attributeSyntax, cancellationToken).Symbol;

    public bool TryGetConstructorArgumentValue(AttributeData attributeData, int index, out string? value) {
        if (attributeData.ConstructorArguments.Length > index) {
            value = attributeData.ConstructorArguments[index].Value?.ToString();
            return true;
        }

        value = null;
        return false;
    }
}
