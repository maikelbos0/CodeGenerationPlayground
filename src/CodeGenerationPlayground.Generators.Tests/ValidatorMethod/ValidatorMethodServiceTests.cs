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

        var result = subject.GetValidatorMethodData(CancellationToken.None);

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

        var result = subject.GetValidatorMethodData(CancellationToken.None);

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

        var grandParent = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName("Namespace"))
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([parent]));

        var node = ((ClassDeclarationSyntax)grandParent.Members.Single()).Members.Single();

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

        var results = subject.GetValidatorMethodData(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, validatorMethodData => validatorMethodData.Name == "ValidatorMethod1");
        Assert.Contains(results, validatorMethodData => validatorMethodData.Name == "ValidatorMethod2");

        foreach (var result in results) {
            Assert.Equal(result.Ancestors, ["Namespace", "Bar"]);
        }
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

        var methodSymbol = Substitute.For<IMethodSymbol>();
        symbolProvider.GetMethodSymbol(Arg.Any<MethodDeclarationSyntax>(), CancellationToken.None).Returns(methodSymbol);
        methodSymbol.Parameters.Returns(ImmutableArray<IParameterSymbol>.Empty);

        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        var result = Assert.Single(subject.GetValidatorMethodData(CancellationToken.None));

        Assert.Equal(2, result.MethodCandidates.Length);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, true, ParameterType.Object)]
    [InlineData(true, true, ParameterType.ValidationContext)]
    [InlineData(true, true, ParameterType.Object, ParameterType.ValidationContext)]
    [InlineData(true, true, ParameterType.ValidationContext, ParameterType.Object)]
    [InlineData(false, false)]
    [InlineData(true, false, ParameterType.Invalid)]
    [InlineData(true, false, ParameterType.Object, ParameterType.Invalid)]
    [InlineData(true, false, ParameterType.Object, ParameterType.Object)]
    [InlineData(true, false, ParameterType.ValidationContext, ParameterType.ValidationContext)]
    [InlineData(true, false, ParameterType.ValidationContext, ParameterType.Object, ParameterType.Object)]
    [InlineData(true, false, ParameterType.ValidationContext, ParameterType.Object, ParameterType.Invalid)]
    public void GetValidatorMethodDataReturnsCorrectlyMappedCandidateMethod(bool returnBoolean, bool expectedHasValidSignature, params ParameterType[] parameterTypes) {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var candidateMethod = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(returnBoolean ? SyntaxKind.BoolKeyword : SyntaxKind.VoidKeyword)),
            SyntaxFactory.Identifier("ValidatorMethod")
        );

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property, candidateMethod]));

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

        var methodSymbol = Substitute.For<IMethodSymbol>();
        var parameterSymbols = ImmutableArray.CreateRange(parameterTypes.Select(CreateParameterSymbol));
        symbolProvider.GetMethodSymbol(Arg.Any<MethodDeclarationSyntax>(), CancellationToken.None).Returns(methodSymbol);
        methodSymbol.ReturnType.SpecialType.Returns(returnBoolean ? SpecialType.System_Boolean : SpecialType.System_Void);
        methodSymbol.Parameters.Returns(parameterSymbols);

        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        var result = Assert.Single(subject.GetValidatorMethodData(CancellationToken.None));

        var candidateMethodData = Assert.Single(result.MethodCandidates);

        Assert.Equal(expectedHasValidSignature, candidateMethodData.HasValidSignature);
        Assert.Equal(parameterTypes.DefaultIfEmpty(ParameterType.None).First(), candidateMethodData.FirstParameterType);
        Assert.Equal(parameterTypes.Skip(1).DefaultIfEmpty(ParameterType.None).First(), candidateMethodData.SecondParameterType);

        IParameterSymbol CreateParameterSymbol(ParameterType parameterType) {
            var parameterSymbol = Substitute.For<IParameterSymbol>();

            if (parameterType == ParameterType.Object) {
                parameterSymbol.Type.SpecialType.Returns(SpecialType.System_Object);
            }
            else if (parameterType == ParameterType.ValidationContext) {
                var namedTypeSymbol = Substitute.For<INamedTypeSymbol>();

                namedTypeSymbol.ToDisplayString(Arg.Any<SymbolDisplayFormat>()).Returns(ValidatorMethodConstants.GlobalFullyQualifiedValidationContextTypeName);
                parameterSymbol.Type.Returns(namedTypeSymbol);
            }
            else {
                parameterSymbol.Type.SpecialType.Returns(SpecialType.System_String);
            }

            return parameterSymbol;
        }
    }

    [Theory]
    [InlineData(NullableAnnotation.None, ParameterType.Object)]
    [InlineData(NullableAnnotation.Annotated, ParameterType.Object)]
    [InlineData(NullableAnnotation.NotAnnotated, ParameterType.Invalid)]
    public void GetValidatorMethodDataCorrectlyCheckObjectsNullability(NullableAnnotation nullableAnnotation, ParameterType expectedParameterType) {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var candidateMethod = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
            SyntaxFactory.Identifier("ValidatorMethod")
        );

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property, candidateMethod]));

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

        var methodSymbol = Substitute.For<IMethodSymbol>();
        var parameterSymbol = Substitute.For<IParameterSymbol>();
        parameterSymbol.Type.SpecialType.Returns(SpecialType.System_Object);
        parameterSymbol.NullableAnnotation.Returns(nullableAnnotation);
        symbolProvider.GetMethodSymbol(Arg.Any<MethodDeclarationSyntax>(), CancellationToken.None).Returns(methodSymbol);
        methodSymbol.ReturnType.SpecialType.Returns(SpecialType.System_Boolean);
        methodSymbol.Parameters.Returns(ImmutableArray.Create(parameterSymbol));

        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        var result = Assert.Single(subject.GetValidatorMethodData(CancellationToken.None));

        var candidateMethodData = Assert.Single(result.MethodCandidates);

        Assert.Equal(expectedParameterType, candidateMethodData.FirstParameterType);
    }

    [Theory]
    [InlineData(true, SyntaxKind.PublicKeyword)]
    [InlineData(true, SyntaxKind.InternalKeyword)]
    [InlineData(true, SyntaxKind.StaticKeyword, SyntaxKind.InternalKeyword)]
    [InlineData(false, SyntaxKind.PrivateKeyword)]
    [InlineData(false)]
    public void GetValidatorMethodDataReturnsCorrectIsAccessible(bool expectedIsAccessible, params SyntaxKind[] modifiers) {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var candidateMethod = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
            SyntaxFactory.Identifier("ValidatorMethod")
        ).WithModifiers(SyntaxFactory.TokenList(modifiers.Select(modifier => SyntaxFactory.Token(modifier))));

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property, candidateMethod]));

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

        var methodSymbol = Substitute.For<IMethodSymbol>();
        symbolProvider.GetMethodSymbol(Arg.Any<MethodDeclarationSyntax>(), CancellationToken.None).Returns(methodSymbol);
        methodSymbol.ReturnType.SpecialType.Returns(SpecialType.System_Boolean);
        methodSymbol.Parameters.Returns(ImmutableArray<IParameterSymbol>.Empty);

        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        var result = Assert.Single(subject.GetValidatorMethodData(CancellationToken.None));

        var candidateMethodData = Assert.Single(result.MethodCandidates);

        Assert.Equal(expectedIsAccessible, candidateMethodData.IsAccessible);
    }

    [Theory]
    [InlineData(false, SyntaxKind.PublicKeyword)]
    [InlineData(true, SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword)]
    [InlineData(true, SyntaxKind.StaticKeyword)]
    [InlineData(false)]
    public void GetValidatorMethodDataReturnsCorrectIsStatic(bool expectedIsStatic, params SyntaxKind[] modifiers) {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var candidateMethod = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
            SyntaxFactory.Identifier("ValidatorMethod")
        ).WithModifiers(SyntaxFactory.TokenList(modifiers.Select(modifier => SyntaxFactory.Token(modifier))));

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property, candidateMethod]));

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

        var methodSymbol = Substitute.For<IMethodSymbol>();
        symbolProvider.GetMethodSymbol(Arg.Any<MethodDeclarationSyntax>(), CancellationToken.None).Returns(methodSymbol);
        methodSymbol.ReturnType.SpecialType.Returns(SpecialType.System_Boolean);
        methodSymbol.Parameters.Returns(ImmutableArray<IParameterSymbol>.Empty);

        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        var result = Assert.Single(subject.GetValidatorMethodData(CancellationToken.None));

        var candidateMethodData = Assert.Single(result.MethodCandidates);

        Assert.Equal(expectedIsStatic, candidateMethodData.IsStatic);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void GetValidatorMethodDataReturnsCorrectIsGeneric(bool isGenericMethod, bool expectedIsGeneric) {
        var property = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.Identifier("Foo")
        );

        var candidateMethod = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
            SyntaxFactory.Identifier("ValidatorMethod")
        );

        var parent = SyntaxFactory.ClassDeclaration("Bar")
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>([property, candidateMethod]));

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

        var methodSymbol = Substitute.For<IMethodSymbol>();
        symbolProvider.GetMethodSymbol(Arg.Any<MethodDeclarationSyntax>(), CancellationToken.None).Returns(methodSymbol);
        methodSymbol.ReturnType.SpecialType.Returns(SpecialType.System_Boolean);
        methodSymbol.IsGenericMethod.Returns(isGenericMethod);
        methodSymbol.Parameters.Returns(ImmutableArray<IParameterSymbol>.Empty);

        var subject = new ValidatorMethodService(symbolProvider, node, CancellationToken.None);

        var result = Assert.Single(subject.GetValidatorMethodData(CancellationToken.None));

        var candidateMethodData = Assert.Single(result.MethodCandidates);

        Assert.Equal(expectedIsGeneric, candidateMethodData.IsGeneric);
    }
}
