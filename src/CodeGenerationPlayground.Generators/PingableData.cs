using System.Collections.Generic;
using System.Text;

namespace CodeGenerationPlayground.Generators;

public record struct PingableData(string? NamespaceName, List<string> ParentClassNames, string? ClassName, string MethodName) {
    public readonly string GetFileName() {
        var fileNameBuilder = new StringBuilder(NamespaceName);

        foreach (var parentClassName in ParentClassNames) {
            fileNameBuilder
                .Append(".")
                .Append(parentClassName);
        }

        fileNameBuilder
            .Append(".")
            .Append(ClassName)
            .Append(".g.cs");

        return fileNameBuilder.ToString();
    }

    public readonly string GetSource() {
        var indentLevel = 1;
        var sourceBuilder = new StringBuilder("/* namespace ")
            .Append(NamespaceName)
            .AppendLine(" {");

        foreach (var parentClassName in ParentClassNames) {
            sourceBuilder
                .Append(new string('\t', indentLevel++))
                .Append("partial class ")
                .Append(parentClassName)
                .AppendLine(" {");
        }

        sourceBuilder
            .Append(new string('\t', indentLevel++))
            .Append("partial class ")
            .Append(ClassName)
            .AppendLine("{")
            .Append(new string('\t', indentLevel))
            .Append("public string ")
            .Append(MethodName)
            .AppendLine("() => \"Ping!\";")
            .Append(new string('\t', --indentLevel))
            .AppendLine("}");

        foreach (var parentClassName in ParentClassNames) {
            sourceBuilder
                .Append(new string('\t', --indentLevel))
                .AppendLine("}");
        }

        sourceBuilder.AppendLine("} */");

        return sourceBuilder.ToString();
    }
};
