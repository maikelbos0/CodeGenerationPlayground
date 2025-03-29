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
            "Identifier",
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
    [InlineData(ParameterType.None, ParameterType.None, false)]
    [InlineData(ParameterType.Object, ParameterType.None, false)]
    [InlineData(ParameterType.ValidationContext, ParameterType.None, false)]
    [InlineData(ParameterType.Object, ParameterType.ValidationContext, false)]
    [InlineData(ParameterType.ValidationContext, ParameterType.Object, false)]
    [InlineData(ParameterType.ValidationContext, ParameterType.Object, true)]
    public Task WriteSource(ParameterType firstParameterType, ParameterType secondParameterType, bool isStatic) {
        var sourceBuilder = new StringBuilder();
        var subject = new ValidatorMethodData(
            "Identifier",
            "Validate",
            "Namespace.Bar",
            [
                new(firstParameterType, secondParameterType, isStatic, true, true, false)
            ]
        );
        var objectInstance = 2;

        subject.WriteSource(sourceBuilder, ref objectInstance);

        if (isStatic) {
            Assert.Equal(2, objectInstance);
        }
        else {
            Assert.Equal(3, objectInstance);
        }

        return Verifier.Verify(sourceBuilder.ToString());
    }
}
