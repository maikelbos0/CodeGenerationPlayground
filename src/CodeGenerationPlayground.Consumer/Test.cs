namespace CodeGenerationPlayground.Consumer;

public partial class Parent {
    public partial class Test {
        [Pingable]
        public string Ping() => "";

        [Pingable]
        public string Pong() => "";
    }
}
