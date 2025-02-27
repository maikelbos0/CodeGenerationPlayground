namespace CodeGenerationPlayground.Generators;

public static class StaticSources {
    public const string PingableAttribute2 = @"
using System;

namespace CodeGenerationPlayground {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PingableAttribute2 : Attribute { }
}
";
}
