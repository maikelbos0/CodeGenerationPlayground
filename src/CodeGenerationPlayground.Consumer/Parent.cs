namespace CodeGenerationPlayground.Consumer;

public partial class Parent {
    public partial class Class {
        [Pingable]
        public partial string Ping();

        [Pingable]
        public partial string Pong();
    }

    public partial struct Struct {
        [Pingable]
        public partial string Ping();

        [Pingable]
        public partial string Pong();
    }

    public partial record RecordClass {
        [Pingable]
        public partial string Ping();

        [Pingable]
        public partial string Pong();
    }

    public partial record struct RecordStruct {
        [Pingable]
        public partial string Ping();

        [Pingable]
        public partial string Pong();
    }
}
