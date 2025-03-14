using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Threading;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

[Generator(LanguageNames.CSharp)]
public class ValidatorMethodAttributeGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource($"{ValidatorMethodConstants.AttributeName}.g.cs", SourceText.From(ValidatorMethodConstants.AttributeDeclaration, Encoding.UTF8)));

        var methodNames = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                ValidatorMethodConstants.FullyQualifiedAttributeName,
                static (syntaxNode, _) => syntaxNode is PropertyDeclarationSyntax,
                static (context, cancellationToken) => GetValidatorMethodName(context, cancellationToken))
            .Where(methodName => methodName != null)
            .Collect();

        context.RegisterSourceOutput(methodNames, static (context, methodNames) => {
            var sourceBuilder = new StringBuilder();

            sourceBuilder.AppendLine("/*");
            foreach (var methodName in methodNames.Distinct()) {
                sourceBuilder.AppendLine(methodName);
            }
            sourceBuilder.AppendLine("*/");

            context.AddSource(
                "test.g.cs",
                sourceBuilder.ToString()
            );
        });
    }

    private static string? GetValidatorMethodName(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken) {
        var propertyDeclarationSyntax = (PropertyDeclarationSyntax)context.TargetNode;
        var symbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax, cancellationToken);

        if (symbol == null) {
            return null;
        }

        foreach (var attributeData in symbol.GetAttributes()) {
            if (attributeData.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == ValidatorMethodConstants.GlobalFullyQualifiedAttributeName
                && attributeData.ConstructorArguments.Length > 0) {

                return attributeData.ConstructorArguments[0].Value?.ToString();
            }
        }

        return null;
    }
}
