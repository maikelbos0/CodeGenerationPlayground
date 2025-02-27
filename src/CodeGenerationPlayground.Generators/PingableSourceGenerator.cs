using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace CodeGenerationPlayground.Generators;

[Generator]
public class PingableSourceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        //var methodsToGenerate = context.SyntaxProvider
        //    .ForAttributeWithMetadataName(
        //        "CodeGenerationPlayground.PingableAttribute",
        //        static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax,
        //        static (context, _) => context.TargetNode.ToString());
        //.Where(static m => m is not null); // Filter out errors that we don't care about

        var methodsToGenerate = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax,
                static (context, _) => ((MethodDeclarationSyntax)context.Node).Identifier);


        context.RegisterSourceOutput(methodsToGenerate, static (context, source) => context.AddSource($"{Guid.NewGuid()}.g.cs", $"// {source}"));

    }

    //static void Execute(EnumToGenerate? enumToGenerate, SourceProductionContext context) {
    //    if (enumToGenerate is { } value) {
    //        // generate the source code and add it to the output
    //        string result = SourceGenerationHelper.GenerateExtensionClass(value);
    //        // Create a separate partial class file for each enum
    //        context.AddSource($"EnumExtensions.{value.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
    //    }
    //}
}
