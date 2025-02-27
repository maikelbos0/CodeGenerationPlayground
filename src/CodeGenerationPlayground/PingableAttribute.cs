using System;

namespace CodeGenerationPlayground;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class PingableAttribute : Attribute { }
