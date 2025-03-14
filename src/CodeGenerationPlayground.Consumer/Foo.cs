namespace CodeGenerationPlayground.Consumer;
public class Foo {
    [ValidatorMethod(nameof(Validate))]
    public string? Bar { get; set; }

    [ValidatorMethod(null!)]
    public string? Baz { get; set; }

    [ValidatorMethod("NotFound")]
    public string? Qux { get; set; }

    [ValidatorMethod(nameof(NoValidate))]
    public string? Quux { get; set; }


    public bool Validate() => Bar != null;

    public string NoValidate() => Bar ?? "Test";
}
