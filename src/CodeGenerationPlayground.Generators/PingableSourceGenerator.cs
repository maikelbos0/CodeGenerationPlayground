using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGenerationPlayground.Generators;

[Generator(LanguageNames.CSharp)]
public class PingableSourceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var methodsToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                PingableConstants.FullyQualifiedAttributeName,
                static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax,
                static (context, _) => GetMethodData(context.TargetNode))
            .Where(methodData => methodData != null)
            .Select((methodData, _) => methodData!.Value);


        context.RegisterSourceOutput(methodsToGenerate, static (context, methodData) => {
            var sourceBuilder = new StringBuilder("/*");
            var indentLevel = 0;

            methodData.WriteSource(sourceBuilder, ref indentLevel);
            sourceBuilder.Append("*/");

            context.AddSource(
                methodData.Owner.GetFileName(),
                sourceBuilder.ToString()
            );
        });
    }

    private static MethodData? GetMethodData(SyntaxNode node) {
        if (node is not MethodDeclarationSyntax methodDeclarationSyntax || methodDeclarationSyntax.Parent is not TypeDeclarationSyntax) {
            return null;
        }

        var parent = methodDeclarationSyntax.Parent;

        while (parent != null) {
            if (parent is ClassDeclarationSyntax classDeclarationSyntax) {
                methodOwner = new MethodOwnerData(methodOwner, classDeclarationSyntax.Identifier.Text, MethodOwnerType.Class);
            }
            else if (parent is StructDeclarationSyntax structDeclarationSyntax) {
                methodOwner = new MethodOwnerData(methodOwner, structDeclarationSyntax.Identifier.Text, MethodOwnerType.Struct);
            }
            else if (parent is NamespaceDeclarationSyntax namespaceDeclarationSyntax) {
                methodOwner = new MethodOwnerData(methodOwner, namespaceDeclarationSyntax.Name.ToString(), MethodOwnerType.Namespace);
            }
            else if (parent is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax) {
                // File scoped namespaces can be handled the same as normal namespaces, but we know for sure there will not be any parents above it so we can exit the loop
                methodOwner = new MethodOwnerData(methodOwner, fileScopedNamespaceDeclarationSyntax.Name.ToString(), MethodOwnerType.Namespace);
                break;
            }

            parent = parent.Parent;
        }

        var methodModifiers = string.Join(" ", methodDeclarationSyntax.Modifiers.Select(modifier => modifier.Text));
        var methodName = methodDeclarationSyntax.Identifier.Text;

        return new MethodData(methodOwner!.Value, methodModifiers, methodName);
    }

    // TODO also add analyzer for each filter
    // TODO add filter for null owner
    // TODO add filters for these (and see if we can create errors):
    // && methodDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))
    // && type
        // TODO group
}
