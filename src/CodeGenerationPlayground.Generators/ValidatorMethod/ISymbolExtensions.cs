using Microsoft.CodeAnalysis;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public static class ISymbolExtensions {
    public static bool HasName(this ISymbol? symbol, string name)
        => symbol != null && symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == name;
}
