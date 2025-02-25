namespace CodeGenerationPlayground.Tests;

public class TestTests {
    [Fact]
    public void Ping() {
        var subject = new Test();

        Assert.Equal("Pong", subject.Ping());
    }
}