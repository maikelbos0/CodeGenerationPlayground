using System.Collections.Generic;
using System.Text;

namespace CodeGenerationPlayground.Generators;

public record struct PingableData(string? NamespaceName, List<string> ParentClassNames, string? ClassName, string MethodName) {
    public string GetFileName() {
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
};
