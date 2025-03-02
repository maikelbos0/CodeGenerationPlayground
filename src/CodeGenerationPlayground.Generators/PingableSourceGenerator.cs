﻿using Microsoft.CodeAnalysis;
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
        var classDeclarationSyntax = (methodDeclarationSyntax.Parent as ClassDeclarationSyntax);
                
        var className = classDeclarationSyntax?.Identifier.Text;
        
        var parent = classDeclarationSyntax?.Parent;
        var parentClassNames = new List<string>();
        string? namespaceName = null;

        while (parent != null) {
            if (parent is ClassDeclarationSyntax parentClassDeclarationSyntax) {
                parentClassNames.Add(parentClassDeclarationSyntax.Identifier.Text);
            }

            if (parent is NamespaceDeclarationSyntax namespaceDeclarationSyntax) {
                namespaceName = namespaceDeclarationSyntax.Name.ToString();
                break;
            }
            else if (parent is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax) {
                namespaceName = fileScopedNamespaceDeclarationSyntax.Name.ToString();
                break;
            }

            parent = parent.Parent;
        }

        var methodModifiers = methodDeclarationSyntax.Modifiers.Select(modifier => modifier.Text).ToList();
        var methodName = methodDeclarationSyntax.Identifier.Text;

        return new MethodData(namespaceName, parentClassNames, className, methodModifiers, methodName);
    }

    // TODO add filters for these (and see if we can create errors):
    // && methodDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))
    // && type
}
