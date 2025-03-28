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
            .SelectMany((validatorMethodData, _) => validatorMethodData)
            .Where(validatorMethodData => validatorMethodData.MethodCandidates.Count(validatorMethodCandidataData => validatorMethodCandidataData.IsValid) == 1)
            .Collect();

        context.RegisterSourceOutput(validatorMethodData, static (context, validatorMethodData) => {
            var sourceBuilder = new StringBuilder();
            var validatorMethodDataByType = validatorMethodData.GroupBy(validatorMethodData => validatorMethodData.TypeName);

            sourceBuilder.Append(ValidatorMethodConstants.AttributeImplementationStart);
            sourceBuilder.AppendLine(@"
            return ValidationResult.Success;
            /* ");
            foreach (var validatorMethodType in validatorMethodDataByType) {
                sourceBuilder.Append(validatorMethodType.Key);
                sourceBuilder.Append(": ");
                sourceBuilder.AppendLine(string.Join(", ", validatorMethodType.Select(x => x.Name)));
            }
            sourceBuilder.Append(" */");
            sourceBuilder.Append(ValidatorMethodConstants.AttributeImplementationEnd);

            context.AddSource(
                $"{ValidatorMethodConstants.AttributeName}.2.g.cs",
                sourceBuilder.ToString()
            );
        });
    }
}
