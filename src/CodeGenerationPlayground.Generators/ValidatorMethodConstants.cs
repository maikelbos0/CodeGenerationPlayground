namespace CodeGenerationPlayground.Generators;

public static class ValidatorMethodConstants {
    public const string AttributeName = "ValidatorMethodAttribute";
    public const string FullyQualifiedAttributeName = $"{nameof(CodeGenerationPlayground)}.{AttributeName}";
    public const string GlobalFullyQualifiedAttributeName = $"global::{FullyQualifiedAttributeName}";
    public const string AttributeDeclaration = $@"
using System;

namespace {nameof(CodeGenerationPlayground)} {{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public partial class {AttributeName} : Attribute {{
        public string MethodName {{ get; }}
        
        public {AttributeName}(string methodName) {{
            MethodName = methodName;
        }}
    }}
}}
";
}
