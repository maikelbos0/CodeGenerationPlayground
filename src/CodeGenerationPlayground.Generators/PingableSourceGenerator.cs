﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGenerationPlayground.Generators;

[Generator(LanguageNames.CSharp)]
public class PingableSourceGenerator : IIncrementalGenerator {

    // TODO maybe analyzer release tracking

    private static readonly DiagnosticDescriptor methodMissingPartialModifier = new(
        id: "CGP001",
        title: "Method is missing 'partial' modifier",
        messageFormat: "Method '{0}' is missing the required modifier 'partial'",
        category: "Analyzer",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var methodsToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "CodeGenerationPlayground.PingableAttribute",
                static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax,
                static (context, _) => GetMethodData(context.TargetNode));


        context.RegisterSourceOutput(methodsToGenerate, static (context, source) => {
            if (!source.MethodModifiers.Contains("partial")) {
                context.ReportDiagnostic(Diagnostic.Create(
                    methodMissingPartialModifier, 
                    Location.Create(source.Location.FilePath, source.Location.TextSpan, default),
                    source.MethodName
                ));
                return;
            }
            var sourceBuilder = new StringBuilder("/*");
            var indentLevel = 0;

            source.WriteSource(sourceBuilder, ref indentLevel);
            sourceBuilder.Append("*/");

            context.AddSource(
                source.Owner.GetFileName(), 
                sourceBuilder.ToString()
            );
        });
    }

    private static MethodData GetMethodData(SyntaxNode node) {
        var methodDeclarationSyntax = (MethodDeclarationSyntax)node;
        MethodOwnerData? methodOwner = null;

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
        var location = new LocationData(methodDeclarationSyntax.SyntaxTree.FilePath, methodDeclarationSyntax.Identifier.Span);

        return new MethodData(methodOwner!.Value, methodModifiers, methodName, location);
    }

    // TODO add filter for null owner
    // TODO add filters for these (and see if we can create errors):
    // && methodDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))
    // && type
    // TODO group
}
