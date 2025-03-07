using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGenerationPlayground.Generators;

[Generator(LanguageNames.CSharp)]
public class PingableSourceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var typeData = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                PingableConstants.FullyQualifiedAttributeName,
                static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax,
                static (context, _) => GetMethodData(context.TargetNode))
            .Where(methodData => methodData != null)
            .Select((methodData, _) => methodData!.Value)
            .Collect()
            .SelectMany((collection, _) => collection.GroupBy(methodData => methodData.Owner));

        context.RegisterSourceOutput(typeData, static (context, typeData) => {
            var sourceBuilder = new StringBuilder("/*");
            var indentLevel = 0;

            typeData.Key.WriteStart(sourceBuilder, ref indentLevel);
            foreach (var methodData in typeData) {
                methodData.WriteSource(sourceBuilder, ref indentLevel);
            }
            typeData.Key.WriteEnd(sourceBuilder, ref indentLevel);

            sourceBuilder.Append("*/");

            context.AddSource(
                typeData.Key.GetFileName(),
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

            if (parent is ClassDeclarationSyntax classDeclarationSyntax) {
                methodOwner = new MethodOwnerData(methodOwner, classDeclarationSyntax.Identifier.Text, MethodOwnerType.Class);
            }
            else if (parent is StructDeclarationSyntax structDeclarationSyntax) {
                methodOwner = new MethodOwnerData(methodOwner, structDeclarationSyntax.Identifier.Text, MethodOwnerType.Struct);
            }
            else if (parent is RecordDeclarationSyntax recordClassDeclarationSyntax && recordClassDeclarationSyntax.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)) {
                methodOwner = new MethodOwnerData(methodOwner, recordClassDeclarationSyntax.Identifier.Text, MethodOwnerType.RecordStruct);
            }
            else if (parent is RecordDeclarationSyntax recordStructDeclarationSyntax) {
                methodOwner = new MethodOwnerData(methodOwner, recordStructDeclarationSyntax.Identifier.Text, MethodOwnerType.RecordClass);
            }
            else if (parent is InterfaceDeclarationSyntax interfaceDeclarationSyntax) {
                methodOwner = new MethodOwnerData(methodOwner, interfaceDeclarationSyntax.Identifier.Text, MethodOwnerType.Interface);
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
