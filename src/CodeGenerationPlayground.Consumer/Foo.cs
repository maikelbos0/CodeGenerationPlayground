namespace CodeGenerationPlayground.Consumer;
public class Foo {
    [ValidatorMethod(nameof(Validate))]
    public string? Bar { get; set; }

    [ValidatorMethod(null!)]
    public string? Baz { get; set; }

    public bool Validate() => Bar != null;
}
