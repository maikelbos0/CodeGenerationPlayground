using System.ComponentModel.DataAnnotations;

namespace CodeGenerationPlayground.Consumer;
public class Foo {
    [ValidatorMethod(nameof(Validate1))]
    [ValidatorMethod(nameof(Validate2))]
    public string? Bar { get; set; }

    [ValidatorMethod(null!)]
    public string? Baz { get; set; }

    [ValidatorMethod("NotFound")]
    public string? Qux { get; set; }

    [ValidatorMethod(nameof(NoValidate1))]
    [ValidatorMethod(nameof(NoValidate2))]
    [ValidatorMethod(nameof(NoValidate3))]
    [ValidatorMethod(nameof(NoValidate4))]
    public string? Quux { get; set; }

    public bool Validate1() => Bar != null;

    public bool Validate2(object? value) => value != null;

    public bool Validate3(ValidationContext validationContext) => (validationContext.ObjectInstance as Foo)?.Bar != null;

    public bool Validate4(ValidationContext validationContext, object? value) => value != null;

    public string NoValidate1() => Bar ?? "Test";

    public bool NoValidate2(string no) => Quux != null;

    public bool NoValidate3(object? first, object? second) => Quux != null;

    public bool NoValidate4(ValidationContext validationContext, object? value, string no) => value != null;
}
