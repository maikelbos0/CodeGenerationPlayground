using CodeGenerationPlayground.Consumer;
using System;
using System.ComponentModel.DataAnnotations;

Console.WriteLine(new Parent.Class().Ping());
Console.ReadKey();

public class Bla123 : ValidationAttribute     {
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
        return base.IsValid(value, validationContext);
    }

    public override bool IsValid(object? value) {
        return base.IsValid(value);
    }
}
/*
 using System;
using System.ComponentModel.DataAnnotations;

namespace Questio.Shared;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class ValidatorMethodAttribute : ValidationAttribute {
    public string MethodName { get; }

    public ValidatorMethodAttribute(string methodName) {
        MethodName = methodName;
    }

    protected override ValidationResult? IsValid(object? _, ValidationContext validationContext) {
        var methodInfo = validationContext.ObjectType.GetMethod(MethodName);
        
        if (methodInfo == null) {
            throw new InvalidOperationException($"Validation method '{MethodName}' does not exist on type '{validationContext.ObjectType.FullName}'");
        }
        else if (methodInfo.ReturnType != typeof(bool)) {
            throw new InvalidOperationException($"Validation method '{MethodName}' needs to return a boolean value indicating model validity");
        }
        else if (methodInfo.GetParameters().Length > 0) {
            throw new InvalidOperationException($"Validation method '{MethodName}' needs to be parameterless");
        }
                    
        if (!(bool)methodInfo.Invoke(validationContext.ObjectInstance, null)!) {
            var memberNames = validationContext.MemberName is { } memberName ? new[] { memberName } : null;

            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), memberNames);
        }

        return ValidationResult.Success;
    }
}

 */