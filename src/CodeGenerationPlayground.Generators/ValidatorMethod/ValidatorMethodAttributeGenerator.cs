using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace CodeGenerationPlayground.Generators.ValidatorMethod;

[Generator(LanguageNames.CSharp)]
public class ValidatorMethodAttributeGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(context => context.AddSource($"{ValidatorMethodConstants.AttributeName}.1.g.cs", SourceText.From(ValidatorMethodConstants.AttributeDeclaration, Encoding.UTF8)));

        var validatorMethodData = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                ValidatorMethodConstants.FullyQualifiedAttributeName,
                static (syntaxNode, _) => syntaxNode is PropertyDeclarationSyntax,
                static (context, cancellationToken) => new ValidatorMethodService(new SymbolProvider(context.SemanticModel), context.TargetNode, cancellationToken).GetValidatorMethodData(cancellationToken))
            .Collect();

        context.RegisterSourceOutput(validatorMethodData, static (context, validatorMethodData) => {
            var sourceBuilder = new StringBuilder();

            sourceBuilder.Append(ValidatorMethodConstants.AttributeImplementationStart);
            sourceBuilder.Append(@"
            return ValidationResult.Success;
            /* ");
            sourceBuilder.Append(string.Join(", ", validatorMethodData.SelectMany(x => x).Select(x => x.Name)));
            sourceBuilder.Append(" */");
            sourceBuilder.Append(ValidatorMethodConstants.AttributeImplementationEnd);

            context.AddSource(
                $"{ValidatorMethodConstants.AttributeName}.2.g.cs",
                sourceBuilder.ToString()
            );
        });
    }
}
