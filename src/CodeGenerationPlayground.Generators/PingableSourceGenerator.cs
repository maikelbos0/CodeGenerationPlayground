﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGenerationPlayground.Generators;

[Generator(LanguageNames.CSharp)]
public class PingableSourceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var methodData = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                PingableConstants.FullyQualifiedAttributeName,
                static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax,
                static (context, _) => GetMethodData(context.TargetNode))
            .Where(methodData => methodData != null)
            .Select((methodData, _) => methodData!.Value);

        context.RegisterSourceOutput(methodData, static (context, methodData) => {
            var sourceBuilder = new StringBuilder("/*");
            var indentLevel = 0;

            methodData.WriteSource(sourceBuilder, ref indentLevel);
            sourceBuilder.Append("*/");

            context.AddSource(
                methodData.GetFileName(),
                sourceBuilder.ToString()
            );
        });
    }

    private static MethodData? GetMethodData(SyntaxNode node) {
        if (node is not MethodDeclarationSyntax methodDeclarationSyntax || methodDeclarationSyntax.Parent is not TypeDeclarationSyntax) {
            return null;
        }

        var ancestors = new Stack<SyntaxNode>();
        var parent = methodDeclarationSyntax.Parent;

        while (parent != null) {
            ancestors.Push(parent);
            parent = parent.Parent;
        }

        MethodOwnerData? methodOwner = null;

        while (ancestors.Count > 0) {
            parent = ancestors.Pop();

            // TODO support record?
            //https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.syntax.typedeclarationsyntax?view=roslyn-dotnet-4.9.0
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
                methodOwner = new MethodOwnerData(methodOwner, fileScopedNamespaceDeclarationSyntax.Name.ToString(), MethodOwnerType.Namespace);
            }
        }

        var methodModifiers = string.Join(" ", methodDeclarationSyntax.Modifiers.Select(modifier => modifier.Text));
        var methodName = methodDeclarationSyntax.Identifier.Text;

        return new MethodData(methodOwner!.Value, methodModifiers, methodName);
    }

    // TODO add add analyzer for null owner
    // TODO add analyzer for parameter count > 0
    // TODO add filter for parameter count > 0
    // TODO add filter for missing partial // && methodDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))
}
