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

    public ISymbol? GetSymbol(AttributeSyntax attributeSyntax, CancellationToken cancellationToken)
        => semanticModel.GetSymbolInfo(attributeSyntax, cancellationToken).Symbol;
}
