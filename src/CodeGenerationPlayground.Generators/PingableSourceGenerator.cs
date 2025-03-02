using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenerationPlayground.Generators;

[Generator]
public class PingableSourceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var methodsToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "CodeGenerationPlayground.PingableAttribute",
                static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax,
                static (context, _) => GetMethodData(context.TargetNode));

        context.RegisterSourceOutput(methodsToGenerate, static (context, source) => {
            context.AddSource(
                source.GetFileName(), 
                source.GetSource()
            );
        });

    }

    private static MethodData GetMethodData(SyntaxNode node) {
        // TODO take into account structs?
        var methodDeclarationSyntax = (MethodDeclarationSyntax)node;
        var methodOwners = new List<MethodOwnerData>();

        var parent = methodDeclarationSyntax.Parent;

        while (parent != null) {
            if (parent is ClassDeclarationSyntax classDeclarationSyntax) {
                methodOwners.Add(new MethodOwnerData(classDeclarationSyntax.Identifier.Text, MethodOwnerType.Class));
            }
            else if (parent is StructDeclarationSyntax structDeclarationSyntax) {
                methodOwners.Add(new MethodOwnerData(structDeclarationSyntax.Identifier.Text, MethodOwnerType.Struct));
            }
            else if (parent is NamespaceDeclarationSyntax namespaceDeclarationSyntax) {
                methodOwners.Add(new MethodOwnerData(namespaceDeclarationSyntax.Name.ToString(), MethodOwnerType.Namespace));
            }
            else if (parent is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax) {
                // File scoped namespaces can be handled the same as normal namespaces, but we know for sure there will not be any parents above it so we can exit the loop
                methodOwners.Add(new MethodOwnerData(fileScopedNamespaceDeclarationSyntax.Name.ToString(), MethodOwnerType.Namespace));
                break;
            }

            parent = parent.Parent;
        }

        var methodModifiers = methodDeclarationSyntax.Modifiers.Select(modifier => modifier.Text).ToList();
        var methodName = methodDeclarationSyntax.Identifier.Text;

        return new MethodData(methodOwners, methodModifiers, methodName);
    }

    // TODO add filters for these (and see if we can create errors):
    // && methodDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))
    // && type
    // TODO group
}
