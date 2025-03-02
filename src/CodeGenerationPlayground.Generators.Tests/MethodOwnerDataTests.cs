using Xunit;

namespace CodeGenerationPlayground.Generators.Tests;

public class MethodOwnerDataTests {
    [Theory]
    [InlineData("Name", MethodOwnerType.Namespace, "Name", MethodOwnerType.Namespace, true)]
    [InlineData("Name", MethodOwnerType.Class, "Name", MethodOwnerType.Class, true)]
    [InlineData("Name", MethodOwnerType.Struct, "Name", MethodOwnerType.Struct, true)]
    [InlineData("Name", MethodOwnerType.Class, "Other", MethodOwnerType.Class, false)]
    [InlineData("Name", MethodOwnerType.Class, "Name", MethodOwnerType.Struct, false)]
    public void EqualsByValue(string name1, MethodOwnerType type1, string name2, MethodOwnerType type2, bool expectedEquals) {
        var subject1 = new MethodOwnerData(name1, type1);
        var subject2 = new MethodOwnerData(name2, type2);

        Assert.Equal(expectedEquals, subject1.Equals(subject2));
    }
}
