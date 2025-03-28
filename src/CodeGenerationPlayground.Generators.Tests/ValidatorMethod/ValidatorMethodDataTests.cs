using CodeGenerationPlayground.Generators.ValidatorMethod;
using Xunit;

namespace CodeGenerationPlayground.Generators.Tests.ValidatorMethod;

public class ValidatorMethodDataTests {
    [Fact]
    public void GetValidMethodCandidates() {
        var subject = new ValidatorMethodData(
            "Validate",
            [],
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
}
