using System.Text;

namespace CodeGenerationPlayground.Generators;

public record struct MethodData(MethodOwnerData Owner, string MethodModifiers, string MethodName) {
    public readonly string WriteSource(StringBuilder sourceBuilder, ref int indentLevel) {
        sourceBuilder
            .Append(new string('\t', indentLevel))
            .Append(MethodModifiers)
            .Append(" string ")
            .Append(MethodName)
            .AppendLine("() => \"Ping!\";");

        return sourceBuilder.ToString();
    }
};
