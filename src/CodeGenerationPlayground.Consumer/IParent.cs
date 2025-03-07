namespace CodeGenerationPlayground.Consumer;

public partial interface IParent {
    public partial class Test {
        [Pingable]
        public string Ping() => "";

        [Pingable]
        public string Pong() => "";
    }

    public partial interface ITest {
        [Pingable]
        public string Ping() => "";

        [Pingable]
        public string Pong() => "";
    }
}
