using System.Text;

namespace CodeGenerationPlayground.Generators;

public record struct MethodData(MethodOwnerData Owner, string MethodModifiers, string MethodName) {
    public readonly string GetFileName() {
        var fileNameBuilder = new StringBuilder();
        IMethodOwnerData? owner = Owner;

        while (owner != null) {
            fileNameBuilder
                .Insert(0, ".")
                .Insert(0, owner.Name);

            owner = owner.Owner;
        }

        fileNameBuilder
            .Append(MethodName)
            .Append(".g.cs");

        return fileNameBuilder.ToString();
    }

    public readonly string WriteSource(StringBuilder sourceBuilder, ref int indentLevel) {
        Owner.WriteStart(sourceBuilder, ref indentLevel);

        sourceBuilder
            .Append(new string('\t', indentLevel))
            .Append(MethodModifiers)
            .Append(" string ")
            .Append(MethodName)
            .AppendLine("() => \"Ping!\";");

        Owner.WriteEnd(sourceBuilder, ref indentLevel);

        return sourceBuilder.ToString();
    }
};
