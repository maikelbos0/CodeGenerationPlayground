using CodeGenerationPlayground.Generators.ValidatorMethod;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSubstitute;
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
    public void HasPropertySymbolReturnsTrueWithPropertySymbol() {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property]));

        var node = parent.Members.Single();

        var symbolProvider = Substitute.For<ISymbolProvider>();
        symbolProvider.GetPropertySymbol(Arg.Any<PropertyDeclarationSyntax>(), CancellationToken.None).Returns(Substitute.For<IPropertySymbol>());

        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        Assert.True(subject.HasPropertySymbol);
    }

    [Fact]
    public void HasPropertySymbolReturnsFalseWithoutPropertySymbol() {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property]));

        var node = parent.Members.Single();

        var symbolProvider = Substitute.For<ISymbolProvider>();
        symbolProvider.GetPropertySymbol(Arg.Any<PropertyDeclarationSyntax>(), CancellationToken.None).Returns((IPropertySymbol?)null);

        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        Assert.False(subject.HasPropertySymbol);
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

        var symbolProvider = Substitute.For<ISymbolProvider>();
        symbolProvider.GetPropertySymbol(Arg.Any<PropertyDeclarationSyntax>(), CancellationToken.None).Returns(Substitute.For<IPropertySymbol>());
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

        var symbolProvider = Substitute.For<ISymbolProvider>();
        symbolProvider.GetPropertySymbol(Arg.Any<PropertyDeclarationSyntax>(), CancellationToken.None).Returns(Substitute.For<IPropertySymbol>());
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

    [Fact]
    public void GetValidatorMethodDataReturnsEmptyListWithoutPropertySymbol() {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property]));

        var node = parent.Members.Single();

        var symbolProvider = Substitute.For<ISymbolProvider>();
        symbolProvider.GetPropertySymbol(Arg.Any<PropertyDeclarationSyntax>(), CancellationToken.None).Returns((IPropertySymbol?)null);

        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        var result = subject.GetValidatorMethodData();

        Assert.Empty(result);
    }

    [Fact]
    public void GetValidatorMethodDataReturnsEmptyListWithoutValidatorMethodAttributes() {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property]));

        var node = parent.Members.Single();

        var symbolProvider = Substitute.For<ISymbolProvider>();
        var propertySymbol = Substitute.For<IPropertySymbol>();
        var attributeData1 = Substitute.For<AttributeData>();
        var attributeData2 = Substitute.For<AttributeData>();
        symbolProvider.GetPropertySymbol(Arg.Any<PropertyDeclarationSyntax>(), CancellationToken.None).Returns(propertySymbol);
        propertySymbol.GetAttributes().Returns([attributeData1, attributeData2]);
        attributeData1.AttributeClass!.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns($"global::{nameof(CodeGenerationPlayground)}.FooAttribute");
        attributeData2.AttributeClass!.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns($"global::{nameof(CodeGenerationPlayground)}.BarAttribute");

        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        var result = subject.GetValidatorMethodData();

        Assert.Empty(result);
    }

    [Fact]
    public void GetValidatorMethodDataReturnsValidatorMethodDataIfPresent() {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property]));

        var node = parent.Members.Single();

        var symbolProvider = Substitute.For<ISymbolProvider>();
        var propertySymbol = Substitute.For<IPropertySymbol>();
        var attributeData1 = Substitute.For<AttributeData>();
        var attributeData2 = Substitute.For<AttributeData>();
        var attributeData3 = Substitute.For<AttributeData>();
        var attributeData4 = Substitute.For<AttributeData>();
        var attributeData5 = Substitute.For<AttributeData>();
        symbolProvider.GetPropertySymbol(Arg.Any<PropertyDeclarationSyntax>(), CancellationToken.None).Returns(propertySymbol);
        propertySymbol.GetAttributes().Returns([attributeData1, attributeData2, attributeData3, attributeData4, attributeData5]);
        attributeData1.AttributeClass!.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns($"global::{nameof(CodeGenerationPlayground)}.FooAttribute");
        attributeData2.AttributeClass!.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns(ValidatorMethodConstants.GlobalFullyQualifiedAttributeName);
        attributeData3.AttributeClass!.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns(ValidatorMethodConstants.GlobalFullyQualifiedAttributeName);
        attributeData4.AttributeClass!.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns($"global::{nameof(CodeGenerationPlayground)}.BarAttribute");
        attributeData5.AttributeClass!.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns(ValidatorMethodConstants.GlobalFullyQualifiedAttributeName);
        symbolProvider.TryGetConstructorArgumentValue(Arg.Any<AttributeData>(), Arg.Any<int>(), out Arg.Any<string?>()).Returns(false);
        symbolProvider.TryGetConstructorArgumentValue(attributeData2, Arg.Any<int>(), out Arg.Any<string?>()).Returns(callInfo => {
            callInfo[2] = "ValidatorMethod1";
            return true;
        });
        symbolProvider.TryGetConstructorArgumentValue(attributeData3, Arg.Any<int>(), out Arg.Any<string?>()).Returns(callInfo => {
            callInfo[2] = "ValidatorMethod2";
            return true; 
        });
        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        var result = subject.GetValidatorMethodData();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, validatorMethodData => validatorMethodData.Name == "ValidatorMethod1");
        Assert.Contains(result, validatorMethodData => validatorMethodData.Name == "ValidatorMethod2");
    }


    [Fact]
    public void GetValidatorMethodDataReturnsCandidateMethodsIfPresent() {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var candidateMethod1 = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
            SyntaxFactory.Identifier("ValidatorMethod")
        );

        var candidateMethod2 = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
            SyntaxFactory.Identifier("ValidatorMethod")
        );

        var otherMethod = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property, candidateMethod1, candidateMethod2, otherMethod]));

        var node = parent.Members.OfType<PropertyDeclarationSyntax>().Single();

        var symbolProvider = Substitute.For<ISymbolProvider>();
        var propertySymbol = Substitute.For<IPropertySymbol>();
        var attributeData = Substitute.For<AttributeData>();
        symbolProvider.GetPropertySymbol(Arg.Any<PropertyDeclarationSyntax>(), CancellationToken.None).Returns(propertySymbol);
        propertySymbol.GetAttributes().Returns([attributeData]);
        attributeData.AttributeClass!.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns(ValidatorMethodConstants.GlobalFullyQualifiedAttributeName);
        symbolProvider.TryGetConstructorArgumentValue(attributeData, Arg.Any<int>(), out Arg.Any<string?>()).Returns(callInfo => {
            callInfo[2] = "ValidatorMethod";
            return true;
        });

        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        var result = Assert.Single(subject.GetValidatorMethodData());

        Assert.Equal(2, result.CandidateMethods.Length);
    }
}
