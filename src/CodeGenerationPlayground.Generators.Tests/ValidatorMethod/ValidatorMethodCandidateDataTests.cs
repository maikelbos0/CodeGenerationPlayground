using CodeGenerationPlayground.Generators.ValidatorMethod;
using Xunit;

namespace CodeGenerationPlayground.Generators.Tests.ValidatorMethod;

public class ValidatorMethodCandidateDataTests {
    [Theory]
    [InlineData(true, true, false, true)]
    [InlineData(false, false, true, false)]
    [InlineData(false, true, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(true, true, true, false)]
    public void IsValid(bool hasValidSignature, bool isAccessible, bool isGeneric, bool expectedIsValid) {
        var subject = new ValidatorMethodCandidateData(ParameterType.None, ParameterType.None, false, hasValidSignature, isAccessible, isGeneric);

        Assert.Equal(expectedIsValid, subject.IsValid);
    }
}
