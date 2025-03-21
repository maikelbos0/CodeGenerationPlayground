using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public interface ISymbolProvider {
    IPropertySymbol? GetPropertySymbol(PropertyDeclarationSyntax propertyDeclarationSyntax, CancellationToken cancellationToken);
    IMethodSymbol? GetMethodSymbol(MethodDeclarationSyntax methodDeclarationSyntax, CancellationToken cancellationToken);
    ISymbol? GetSymbol(AttributeSyntax attributeSyntax, CancellationToken cancellationToken);
    bool TryGetConstructorArgumentValue(AttributeData attributeData, int index, out string? value);
}