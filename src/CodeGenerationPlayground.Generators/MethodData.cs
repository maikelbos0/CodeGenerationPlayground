using System.Text;

namespace CodeGenerationPlayground.Generators;

public record struct MethodData(MethodOwnerData? Owner, string MethodModifiers, string MethodName) {
    public readonly string GetFileName() {
        var fileNameBuilder = new StringBuilder();
        IMethodOwnerData? owner = Owner;

        while (owner != null) {
            fileNameBuilder
                .Insert(0, ".")
                .Insert(0, owner.Name);

            owner = owner.Owner;
        }

        fileNameBuilder.Append("g.cs");

        return fileNameBuilder.ToString();
    }

    public readonly string GetSource() {
        var indentLevel = 0;
        var sourceBuilder = new StringBuilder("/*");

        Owner?.WriteStart(sourceBuilder, ref indentLevel);

        sourceBuilder
            .Append(new string('\t', indentLevel))
            .Append(MethodModifiers)
            .Append(" string ")
            .Append(MethodName)
            .AppendLine("() => \"Ping!\";");

        Owner?.WriteEnd(sourceBuilder, ref indentLevel);

        sourceBuilder.AppendLine("*/");

        return sourceBuilder.ToString();
    }
};
