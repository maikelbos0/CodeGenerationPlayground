namespace CodeGenerationPlayground.Consumer;
public class Foo {
    [ValidatorMethod(nameof(Validate))]
    public string? Bar { get; set; }

    public bool Validate() => Bar != null;
}
