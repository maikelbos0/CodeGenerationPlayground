using Xunit;

namespace CodeGenerationPlayground.Generators.Tests;

public class MethodDataTests {
    [Fact]
    public void EqualsByValue() {
        var subject1 = new MethodData([], [], "Name");
        var subject2 = new MethodData([], [], "Name");

        Assert.True(subject1.Equals(subject2));
    }
}
