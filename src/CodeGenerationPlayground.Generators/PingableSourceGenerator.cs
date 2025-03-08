using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CodeGenerationPlayground.Generators;

[Generator(LanguageNames.CSharp)]
public class PingableSourceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var typeData = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                PingableConstants.FullyQualifiedAttributeName,
                static (syntaxNode, _) => ShouldGeneratePingable(syntaxNode),
                static (context, cancellationToken) => GetMethodData(context.TargetNode, cancellationToken))
            .Collect()
            .SelectMany((collection, _) => collection.GroupBy(methodData => methodData.Owner));

        context.RegisterSourceOutput(typeData, static (context, typeData) => {
            var sourceBuilder = new StringBuilder();
            var indentLevel = 0;

            typeData.Key.WriteStart(sourceBuilder, ref indentLevel);
            foreach (var methodData in typeData) {
                methodData.WriteSource(sourceBuilder, ref indentLevel);
            }
            typeData.Key.WriteEnd(sourceBuilder, ref indentLevel);

            context.AddSource(
                typeData.Key.GetFileName(),
                sourceBuilder.ToString()
            );
        });
    }

    private static bool ShouldGeneratePingable(SyntaxNode syntaxNode)
        => syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
        && methodDeclarationSyntax.Parent is TypeDeclarationSyntax
        && methodDeclarationSyntax.ParameterList.Parameters.Count == 0
        && methodDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword));

    private static MethodData GetMethodData(SyntaxNode syntaxNode, CancellationToken cancellationToken) {
        var methodDeclarationSyntax = (MethodDeclarationSyntax)syntaxNode;
        var ancestors = new Stack<SyntaxNode>();
        var parent = methodDeclarationSyntax.Parent;

        while (parent != null) {
            cancellationToken.ThrowIfCancellationRequested();

            ancestors.Push(parent);
            parent = parent.Parent;
        }

        MethodOwnerData? methodOwner = null;

        while (ancestors.Count > 0) {
            cancellationToken.ThrowIfCancellationRequested();

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
}
