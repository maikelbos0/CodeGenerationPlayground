using System.Text;

namespace CodeGenerationPlayground.Generators;
public interface IMethodOwnerData {
    string Name { get; set; }
    IMethodOwnerData? Owner { get; set; }
    MethodOwnerType Type { get; set; }
    string TypeName { get; }

    void Deconstruct(out IMethodOwnerData? Owner, out string Name, out MethodOwnerType Type);
    bool Equals(MethodOwnerData other);
    bool Equals(object obj);
    int GetHashCode();
    string ToString();
    void WriteEnd(StringBuilder sourceBuilder, ref int indentLevel);
    void WriteStart(StringBuilder sourceBuilder, ref int indentLevel);
}