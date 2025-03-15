using CodeGenerationPlayground.Generators.ValidatorMethod;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace CodeGenerationPlayground.Generators.Tests.ValidatorMethod;

public class ValidatorMethodServiceTests {
    [Fact]
    public void IsPropertyReturnsTrueForPropertyDeclarationSyntax() {
        var node = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo"));

        var subject = new ValidatorMethodService(node);

        Assert.True(subject.IsProperty);
    }

    [Fact]
    public void IsPropertyReturnsFalseForMethodDeclarationSyntax() {
        var node = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("GetFoo"));

        var subject = new ValidatorMethodService(node);

        Assert.False(subject.IsProperty);
    }
}
