using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGenerationPlayground.Generators;

// TODO make sure this all is comparable
public record struct MethodData(List<MethodOwnerData> MethodOwnerData, string MethodModifiers, string MethodName) {
    public readonly string GetFileName() {
        var fileNameBuilder = new StringBuilder();

        foreach (var methodOwnerData in MethodOwnerData) {
            fileNameBuilder
                .Append(methodOwnerData.Name)
                .Append(".");
        }

        fileNameBuilder.Append("g.cs");

        return fileNameBuilder.ToString();
    }

    public readonly string GetSource() {
        var indentLevel = 0;
        var sourceBuilder = new StringBuilder("/*");

        foreach (var methodOwnerData in MethodOwnerData) {
            methodOwnerData.WriteStart(sourceBuilder, ref indentLevel);
        }

        sourceBuilder
            .Append(new string('\t', indentLevel))
            .Append(MethodModifiers)
            .Append(" string ")
            .Append(MethodName)
            .AppendLine("() => \"Ping!\";");

        foreach (var methodOwnerData in MethodOwnerData.AsEnumerable().Reverse()) {
            methodOwnerData.WriteEnd(sourceBuilder, ref indentLevel);
        }

        sourceBuilder.AppendLine("*/");

        return sourceBuilder.ToString();
    }
};
