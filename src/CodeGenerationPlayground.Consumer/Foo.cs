using System.ComponentModel.DataAnnotations;

namespace CodeGenerationPlayground.Consumer;

public class Foo {
    [ValidatorMethod(nameof(Validate1))]
    [ValidatorMethod(nameof(Validate2))]
    [ValidatorMethod(nameof(Validate3))]
    [ValidatorMethod(nameof(Validate4))]
    public string? Bar { get; set; }

    [ValidatorMethod("NotFound")]
    public string? Baz { get; set; }

    [ValidatorMethod(nameof(NoValidate1))]
    [ValidatorMethod(nameof(NoValidate2))]
    [ValidatorMethod(nameof(NoValidate3))]
    [ValidatorMethod(nameof(NoValidate4))]
    [ValidatorMethod(nameof(NoValidate5))]
    public string? Qux { get; set; }

    [ValidatorMethod(nameof(DoubleValidate))]
    public string? Quux { get; set; }

    public bool Validate1() => Bar != null;

    public bool Validate2(object? value) => value != null;

    public bool Validate3(ValidationContext validationContext) => (validationContext.ObjectInstance as Foo)?.Bar != null;

    public bool Validate4(ValidationContext validationContext, object? value) => value != null;

    public string NoValidate1() => Qux ?? "Test";

    public bool NoValidate2(string no) => Qux != null;

    public bool NoValidate3(object? first, object? second) => Qux != null;

    public bool NoValidate4(ValidationContext validationContext, object? value, string no) => value != null;

    protected bool NoValidate5<TValue>(TValue value) => value != null;

    public bool DoubleValidate(object? value) => true;

    public bool DoubleValidate(ValidationContext validationContext, object? value) => true;
}
