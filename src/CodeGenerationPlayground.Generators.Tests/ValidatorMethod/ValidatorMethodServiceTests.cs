using CodeGenerationPlayground.Generators.ValidatorMethod;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSubstitute;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Xunit;

namespace CodeGenerationPlayground.Generators.Tests.ValidatorMethod;

public class ValidatorMethodServiceTests {
    [Fact]
    public void IsPropertyReturnsTrueForPropertyDeclarationSyntax() {
        var node = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var subject = new ValidatorMethodService(Substitute.For<ISymbolProvider>(), node, CancellationToken.None);

        Assert.True(subject.IsProperty);
    }

    [Fact]
    public void IsPropertyReturnsFalseForMethodDeclarationSyntax() {
        var node = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("GetFoo")
        );

        var subject = new ValidatorMethodService(Substitute.For<ISymbolProvider>(), node, CancellationToken.None);

        Assert.False(subject.IsProperty);
    }

    [Fact]
    public void HasValidParentReturnsTrueForClass() {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property]));

        var node = parent.Members.Single();

        var subject = new ValidatorMethodService(Substitute.For<ISymbolProvider>(), node, CancellationToken.None);

        Assert.True(subject.HasValidParent);
    }

    [Fact]
    public void HasValidParentReturnsTrueForStruct() {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var parent = SyntaxFactory.StructDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property]));

        var node = parent.Members.Single();

        var subject = new ValidatorMethodService(Substitute.For<ISymbolProvider>(), node, CancellationToken.None);

        Assert.True(subject.HasValidParent);
    }

    [Fact]
    public void HasValidParentReturnsFalseForNamespace() {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var parent = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName("Bar"))
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property]));

        var node = parent.Members.Single();

        var subject = new ValidatorMethodService(Substitute.For<ISymbolProvider>(), node, CancellationToken.None);

        Assert.False(subject.HasValidParent);
    }

    [Fact]
    public void HasValidParentReturnsFalseWithoutParent() {
        var node = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var subject = new ValidatorMethodService(Substitute.For<ISymbolProvider>(), node, CancellationToken.None);

        Assert.False(subject.HasValidParent);
    }

    [Fact]
    public void HasValidatorMethodAttributesReturnsTrueWithValidatorMethodAttribute() {
        var attributeList1 = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("FooAttribute"))
        ]));

        var attributeList2 = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("BarAttribute")),
            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(ValidatorMethodConstants.AttributeName)),
            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("BazAttribute"))
        ]));

        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        ).WithAttributeLists(SyntaxFactory.List([attributeList1, attributeList2]));

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property]));

        var node = parent.Members.Single();

        var x = Substitute.For<IMethodSymbol>();

        x.GetAttributes().Returns(callInfo => new ImmutableArray<AttributeData>());
        
        var symbolProvider = Substitute.For<ISymbolProvider>();
        symbolProvider.GetSymbol(Arg.Any<AttributeSyntax>(), CancellationToken.None)
            .Returns(callInfo => {
                var methodSymbol = Substitute.For<IMethodSymbol>();
                var namedTypeSymbol = Substitute.For<INamedTypeSymbol>();

                namedTypeSymbol.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns($"global::{nameof(CodeGenerationPlayground)}.{callInfo.ArgAt<AttributeSyntax>(0).Name}");
                methodSymbol.ContainingType.Returns(namedTypeSymbol);

                return methodSymbol;
            });

        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        Assert.True(subject.HasValidatorMethodAttributes);
    }

    [Fact]
    public void HasValidatorMethodAttributesReturnsFalseWithoutValidatorMethodAttribute() {
        var attributeList1 = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("FooAttribute"))
        ]));

        var attributeList2 = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("BarAttribute")),
            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("BazAttribute"))
        ]));

        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        ).WithAttributeLists(SyntaxFactory.List([attributeList1, attributeList2]));

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property]));

        var node = parent.Members.Single();

        var x = Substitute.For<IMethodSymbol>();

        x.GetAttributes().Returns(callInfo => new ImmutableArray<AttributeData>());

        var symbolProvider = Substitute.For<ISymbolProvider>();
        symbolProvider.GetSymbol(Arg.Any<AttributeSyntax>(), CancellationToken.None)
            .Returns(callInfo => {
                var methodSymbol = Substitute.For<IMethodSymbol>();
                var namedTypeSymbol = Substitute.For<INamedTypeSymbol>();

                namedTypeSymbol.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns($"global::{nameof(CodeGenerationPlayground)}.{callInfo.ArgAt<AttributeSyntax>(0).Name}");
                methodSymbol.ContainingType.Returns(namedTypeSymbol);

                return methodSymbol;
            });

        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        Assert.False(subject.HasValidatorMethodAttributes);
    }
}
