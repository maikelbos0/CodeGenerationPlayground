using System;
using System.Text;

namespace CodeGenerationPlayground.Generators;

public record struct MethodOwnerData(IMethodOwnerData? Owner, string Name, MethodOwnerType Type) : IMethodOwnerData {
    public readonly string TypeName => Type switch {
        MethodOwnerType.Namespace => "namespace",
        MethodOwnerType.Class => "partial class",
        MethodOwnerType.Struct => "partial struct",
        _ => throw new NotImplementedException()
    };

    public readonly void WriteStart(StringBuilder sourceBuilder, ref int indentLevel) {
        Owner?.WriteStart(sourceBuilder, ref indentLevel);

        sourceBuilder.Append(new string('\t', indentLevel++))
            .Append(TypeName)
            .Append(" ")
            .Append(Name)
            .AppendLine(" {{");
    }

    public readonly void WriteEnd(StringBuilder sourceBuilder, ref int indentLevel) {
        sourceBuilder
            .Append(new string('\t', --indentLevel))
            .AppendLine("}");

        Owner?.WriteEnd(sourceBuilder, ref indentLevel);
    }
};
