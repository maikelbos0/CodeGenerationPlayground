using CodeGenerationPlayground.Generators.ValidatorMethod;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace CodeGenerationPlayground.Generators.Tests.ValidatorMethod;

public class ValidatorMethodDataTests {
    [Fact]
    public void GetValidMethodCandidates() {
        var subject = new ValidatorMethodData(
            "Validate",
            "Namespace.Bar",
            [
                new(ParameterType.None, ParameterType.None, false, false, true, false),
                new(ParameterType.None, ParameterType.None, false, true, false, false),
                new(ParameterType.None, ParameterType.None, false, true, true, false),
                new(ParameterType.None, ParameterType.None, false, true, true, false),
                new(ParameterType.None, ParameterType.None, false, true, true, true)
            ]
        );

        var result = subject.GetValidMethodCandidates();

        Assert.Equal(2, result.Length);
    }

    [Theory]
    [InlineData(ParameterType.None, ParameterType.None)]
    [InlineData(ParameterType.Object, ParameterType.None)]
    [InlineData(ParameterType.ValidationContext, ParameterType.None)]
    [InlineData(ParameterType.Object, ParameterType.ValidationContext)]
    [InlineData(ParameterType.ValidationContext, ParameterType.Object)]
    public Task WriteSource(ParameterType firstParameterType, ParameterType secondParameterType) {
        var sourceBuilder = new StringBuilder();
        var subject = new ValidatorMethodData(
            "Validate",
            "Namespace.Bar",
            [
                new(firstParameterType, secondParameterType, false, true, true, false)
            ]
        );

        subject.WriteSource(sourceBuilder);

        return Verifier.Verify(sourceBuilder.ToString());
    }
}
