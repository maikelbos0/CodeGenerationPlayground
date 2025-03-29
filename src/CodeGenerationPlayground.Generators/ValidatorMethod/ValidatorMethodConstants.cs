﻿namespace CodeGenerationPlayground.Generators.ValidatorMethod;

public static class ValidatorMethodConstants {
    public const string AttributeName = "ValidatorMethodAttribute";
    public const string FullyQualifiedAttributeName = $"{nameof(CodeGenerationPlayground)}.{AttributeName}";
    public const string GlobalFullyQualifiedAttributeName = $"global::{FullyQualifiedAttributeName}";
    public const string AttributeDeclaration = $@"
using System;
using System.ComponentModel.DataAnnotations;

namespace {nameof(CodeGenerationPlayground)} {{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public partial class {AttributeName} : ValidationAttribute {{
        public string MethodName {{ get; }}
        
        public {AttributeName}(string methodName) {{
            MethodName = methodName;
        }}
    }}
}}
";
    public const string ValueParameterName = "value";
    public const string ValidationContextParameterName = "validationContext";
    public const string AttributeImplementationStart = $@"
#nullable enable

using System;
using System.ComponentModel.DataAnnotations;

namespace {nameof(CodeGenerationPlayground)} {{
    public partial class {ValidatorMethodConstants.AttributeName} : ValidationAttribute {{
        protected override ValidationResult? IsValid(object? {ValueParameterName}, ValidationContext {ValidationContextParameterName}) {{
    public const string AttributeImplementationEnd = @"
        }
    }
}
";
    public const string GlobalFullyQualifiedValidationContextTypeName = "global::System.ComponentModel.DataAnnotations.ValidationContext";
}
