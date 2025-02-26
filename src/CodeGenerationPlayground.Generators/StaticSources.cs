namespace CodeGenerationPlayground.Generators;

public static class StaticSources {
    public const string PingableAttribute = @"
using System;

namespace CodeGenerationPlayground {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PingableAttribute : Attribute { }
}
";
}
