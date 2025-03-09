using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CodeGenerationPlayground.Generators;

[Generator(LanguageNames.CSharp)]
public class ValidatorMethodAttributeGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource($"{ValidatorMethodConstants.AttributeName}.g.cs", SourceText.From(ValidatorMethodConstants.AttributeDeclaration, Encoding.UTF8)));
    }
}
