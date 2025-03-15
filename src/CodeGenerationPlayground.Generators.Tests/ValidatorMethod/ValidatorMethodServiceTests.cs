using CodeGenerationPlayground.Generators.ValidatorMethod;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Xunit;

namespace CodeGenerationPlayground.Generators.Tests.ValidatorMethod;

public class ValidatorMethodServiceTests {
    [Fact]
    public void IsPropertyReturnsTrueForPropertyDeclarationSyntax() {
        var node = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var subject = new ValidatorMethodService(node);

        Assert.True(subject.IsProperty);
    }

    [Fact]
    public void IsPropertyReturnsFalseForMethodDeclarationSyntax() {
        var node = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("GetFoo")
        );

        var subject = new ValidatorMethodService(node);

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
        
        var subject = new ValidatorMethodService(node);

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
        
        var subject = new ValidatorMethodService(node);

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
        
        var subject = new ValidatorMethodService(node);

        Assert.False(subject.HasValidParent);
    }

    [Fact]
    public void HasValidParentReturnsFalseWithoutParent() {
        var node = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var subject = new ValidatorMethodService(node);

        Assert.False(subject.HasValidParent);
    }
}
