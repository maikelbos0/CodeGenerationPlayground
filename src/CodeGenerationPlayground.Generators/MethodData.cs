using System.Text;

namespace CodeGenerationPlayground.Generators;

public record struct MethodData(MethodOwnerData Owner, string MethodModifiers, string MethodName) {
    public readonly string GetSource() {
        var indentLevel = 0;
        var sourceBuilder = new StringBuilder("/*");

        Owner.WriteStart(sourceBuilder, ref indentLevel);

        sourceBuilder
            .Append(new string('\t', indentLevel))
            .Append(MethodModifiers)
            .Append(" string ")
            .Append(MethodName)
            .AppendLine("() => \"Ping!\";");

        Owner.WriteEnd(sourceBuilder, ref indentLevel);

        sourceBuilder.AppendLine("*/");

        return sourceBuilder.ToString();
    }
};
