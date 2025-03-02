using Xunit;

namespace CodeGenerationPlayground.Generators.Tests;

public class MethodDataTests {
    [Theory]
    [InlineData("Parent", "static", "Name", "Parent", "static", "Name", true)]
    [InlineData("Parent", "static", "Name", "Other", "static", "Name", false)]
    [InlineData("Parent", "static", "Name", "Parent", "private static", "Name", false)]
    [InlineData("Parent", "static", "Name", "Parent", "static", "Other", false)]
    public void EqualsByValue(string methodOwnerName1, string methodModifiers1, string methodName1,string methodOwnerName2, string methodModifiers2, string methodName2, bool expectedEquals) {
        var subject1 = new MethodData([new MethodOwnerData(methodOwnerName1, MethodOwnerType.Class)], methodModifiers1, methodName1);
        var subject2 = new MethodData([new MethodOwnerData(methodOwnerName2, MethodOwnerType.Class)], methodModifiers2, methodName2);

        Assert.Equal(expectedEquals, subject1.Equals(subject2));
    }
}
