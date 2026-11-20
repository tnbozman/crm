using Crm.__Module__.Validation;
using Crm.SharedKernel.Errors;
using Shouldly;
using Xunit;

namespace Crm.__Module__.Tests.Validation;

public sealed class ItemRulesTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RequireValidName_Blank_ThrowsRequestValidationException(string name) =>
        Should.Throw<RequestValidationException>(() => ItemRules.RequireValidName(name))
            .Errors.ShouldContainKey(nameof(ItemRules.Name));

    [Fact]
    public void RequireValidName_LongerThanMaximum_ThrowsRequestValidationException() =>
        Should.Throw<RequestValidationException>(() => ItemRules.RequireValidName(new string('x', ItemRules.NameMaxLength + 1)));

    [Fact]
    public void RequireValidName_Valid_ReturnsItTrimmed() =>
        ItemRules.RequireValidName("  Example  ").ShouldBe("Example");
}
