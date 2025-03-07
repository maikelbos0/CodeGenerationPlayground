namespace CodeGenerationPlayground.Consumer;

public partial class Parent {
    public partial class Class {
        [Pingable]
        public string Ping() => "";

        [Pingable]
        public string Pong() => "";
    }

    public partial struct Struct {
        [Pingable]
        public string Ping() => "";

        [Pingable]
        public string Pong() => "";
    }

    public partial record RecordClass {
        [Pingable]
        public string Ping() => "";

        [Pingable]
        public string Pong() => "";
    }

    public partial record struct RecordStruct {
        [Pingable]
        public string Ping() => "";

        [Pingable]
        public string Pong() => "";
    }
}
