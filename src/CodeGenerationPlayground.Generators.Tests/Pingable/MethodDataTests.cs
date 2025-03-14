using CodeGenerationPlayground.Generators.Pingable;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace CodeGenerationPlayground.Generators.Tests.Pingable;

public class MethodDataTests {
    [Theory]
    [InlineData("Owner", "static", "Name", "Owner", "static", "Name", true)]
    [InlineData("Owner", "static", "Name", "Other", "static", "Name", false)]
    [InlineData("Owner", "static", "Name", "Owner", "private static", "Name", false)]
    [InlineData("Owner", "static", "Name", "Owner", "static", "Other", false)]
    public void EqualsByValue(string methodOwnerName1, string methodModifiers1, string methodName1, string methodOwnerName2, string methodModifiers2, string methodName2, bool expectedEquals) {
        var subject1 = new MethodData(new MethodOwnerData(null, methodOwnerName1, MethodOwnerType.Class), methodModifiers1, methodName1);
        var subject2 = new MethodData(new MethodOwnerData(null, methodOwnerName2, MethodOwnerType.Class), methodModifiers2, methodName2);

        Assert.Equal(expectedEquals, subject1.Equals(subject2));
    }

    [Fact]
    public Task WriteSource() {
        var indentLevel = 0;
        var sourceBuilder = new StringBuilder();

        var subject = new MethodData(
            new MethodOwnerData(new MethodOwnerData(null, "CodeGenerationPlayground.Generators.Tests", MethodOwnerType.Namespace), "MethodDataTests", MethodOwnerType.Class),
            "public",
            "WriteSource"
        );

        subject.WriteSource(sourceBuilder, ref indentLevel);

        return Verifier.Verify(sourceBuilder);
    }
}
