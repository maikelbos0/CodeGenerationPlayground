namespace CodeGenerationPlayground.Consumer;

public class Bar {
    [ValidatorMethod(nameof(Validate))]
    public string? Example { get; set; }

    public bool Validate(object? value) => value != null;
}
