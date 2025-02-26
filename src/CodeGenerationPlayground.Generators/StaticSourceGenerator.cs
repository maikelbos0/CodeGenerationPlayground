using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CodeGenerationPlayground.Generators;

[Generator]
public class StaticSourceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource($"{nameof(StaticSources.PingableAttribute)}.g.cs", SourceText.From(StaticSources.PingableAttribute, Encoding.UTF8)));
    
    }
}
