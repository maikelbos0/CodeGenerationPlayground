using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace CodeGenerationPlayground.Generators.Tests;

public class MethodOwnerDataTests {
    [Theory]
    [InlineData("Owner", "Name", MethodOwnerType.Namespace, "Owner", "Name", MethodOwnerType.Namespace, true)]
    [InlineData("Owner", "Name", MethodOwnerType.Class, "Owner", "Name", MethodOwnerType.Class, true)]
    [InlineData("Owner", "Name", MethodOwnerType.Struct, "Owner", "Name", MethodOwnerType.Struct, true)]
    [InlineData("Owner", "Name", MethodOwnerType.Class, "Owner", "Other", MethodOwnerType.Class, false)]
    [InlineData("Owner", "Name", MethodOwnerType.Class, "Owner", "Name", MethodOwnerType.Struct, false)]
    [InlineData("Owner", "Name", MethodOwnerType.Class, "Other", "Name", MethodOwnerType.Class, false)]
    public void EqualsByValue(string ownerName1, string name1, MethodOwnerType type1, string ownerName2, string name2, MethodOwnerType type2, bool expectedEquals) {
        var subject1 = new MethodOwnerData(new MethodOwnerData(null, ownerName1, MethodOwnerType.Namespace), name1, type1);
        var subject2 = new MethodOwnerData(new MethodOwnerData(null, ownerName2, MethodOwnerType.Namespace), name2, type2);

        Assert.Equal(expectedEquals, subject1.Equals(subject2));
    }

    [Fact]
    public Task WriteStart() {
        var indentLevel = 0;
        var sourceBuilder = new StringBuilder();

        var subject = new MethodOwnerData(new MethodOwnerData(null, "CodeGenerationPlayground.Generators.Tests", MethodOwnerType.Namespace), "MethodOwnerDataTests", MethodOwnerType.Class);

        subject.WriteStart(sourceBuilder, ref indentLevel);

        return Verifier.Verify(sourceBuilder.ToString());
    }

    [Fact]
    public Task WriteEnd() {
        var indentLevel = 2;
        var sourceBuilder = new StringBuilder();

        var subject = new MethodOwnerData(new MethodOwnerData(null, "CodeGenerationPlayground.Generators.Tests", MethodOwnerType.Namespace), "MethodOwnerDataTests", MethodOwnerType.Class);

        subject.WriteEnd(sourceBuilder, ref indentLevel);

        return Verifier.Verify(sourceBuilder.ToString());
    }
}
