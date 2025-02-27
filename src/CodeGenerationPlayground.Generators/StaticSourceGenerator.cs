using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CodeGenerationPlayground.Generators;

[Generator]
public class StaticSourceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource($"{nameof(StaticSources.PingableAttribute2)}.g.cs", SourceText.From(StaticSources.PingableAttribute2, Encoding.UTF8)));
    }
}
