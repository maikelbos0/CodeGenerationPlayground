using System;
using System.Text;

namespace CodeGenerationPlayground.Generators;

public record struct MethodOwnerData(IMethodOwnerData? Owner, string Name, MethodOwnerType Type) : IMethodOwnerData {
    public readonly string TypeName => Type switch {
        MethodOwnerType.Namespace => "namespace",
        MethodOwnerType.Class => "partial class",
        MethodOwnerType.Struct => "partial struct",
        MethodOwnerType.Record => "partial record",
        _ => throw new NotImplementedException()
    };

    public readonly string GetFileName() {
        var fileNameBuilder = new StringBuilder();
        IMethodOwnerData? owner = this;

        while (owner != null) {
            fileNameBuilder
                .Insert(0, ".")
                .Insert(0, owner.Name);

            owner = owner.Owner;
        }

        fileNameBuilder.Append("g.cs");

        return fileNameBuilder.ToString();
    }

    public readonly void WriteStart(StringBuilder sourceBuilder, ref int indentLevel) {
        Owner?.WriteStart(sourceBuilder, ref indentLevel);

        sourceBuilder.Append(new string('\t', indentLevel++))
            .Append(TypeName)
            .Append(" ")
            .Append(Name)
            .AppendLine(" {");
    }

    public readonly void WriteEnd(StringBuilder sourceBuilder, ref int indentLevel) {
        sourceBuilder
            .Append(new string('\t', --indentLevel))
            .AppendLine("}");

        Owner?.WriteEnd(sourceBuilder, ref indentLevel);
    }
};
