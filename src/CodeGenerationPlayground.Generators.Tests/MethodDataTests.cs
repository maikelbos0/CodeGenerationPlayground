using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace CodeGenerationPlayground.Generators.Tests;

public class MethodDataTests {
    // TODO add tests for location equality
    [Theory]
    [InlineData("Owner", "static", "Name", "Owner", "static", "Name", true)]
    [InlineData("Owner", "static", "Name", "Other", "static", "Name", false)]
    [InlineData("Owner", "static", "Name", "Owner", "private static", "Name", false)]
    [InlineData("Owner", "static", "Name", "Owner", "static", "Other", false)]
    public void EqualsByValue(string methodOwnerName1, string methodModifiers1, string methodName1, string methodOwnerName2, string methodModifiers2, string methodName2, bool expectedEquals) {
        var subject1 = new MethodData(new MethodOwnerData(null, methodOwnerName1, MethodOwnerType.Class), methodModifiers1, methodName1, new LocationData("MethodDataTests.cs", new TextSpan()));
        var subject2 = new MethodData(new MethodOwnerData(null, methodOwnerName2, MethodOwnerType.Class), methodModifiers2, methodName2, new LocationData("MethodDataTests.cs", new TextSpan()));

        Assert.Equal(expectedEquals, subject1.Equals(subject2));
    }

    [Fact]
    public Task WriteSource() {
        var indentLevel = 0;
        var sourceBuilder = new StringBuilder();

        var subject = new MethodData(
            new MethodOwnerData(new MethodOwnerData(null, "CodeGenerationPlayground.Generators.Tests", MethodOwnerType.Namespace), "MethodDataTests", MethodOwnerType.Class),
            "public",
            "WriteSource", 
            new LocationData("MethodDataTests.cs", new TextSpan())
        );

        subject.WriteSource(sourceBuilder, ref indentLevel);

        return Verifier.Verify(sourceBuilder);
    }
}
