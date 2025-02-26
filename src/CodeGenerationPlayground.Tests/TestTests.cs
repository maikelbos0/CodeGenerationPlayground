namespace CodeGenerationPlayground.Tests;

public class TestTests {
    [Fact]
    public void Ping_Works() {
        Assert.Equal("Pong", Ping());
    }

    [Pingable]
    public string Ping() => "Pong";
}
