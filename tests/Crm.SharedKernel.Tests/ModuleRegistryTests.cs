using Crm.SharedKernel.Modules;
using Crm.Testing.Modules;
using Shouldly;
using Xunit;

namespace Crm.SharedKernel.Tests;

public sealed class ModuleRegistryTests
{
    [Fact]
    public void Constructor_DuplicateModuleNames_Throws()
    {
        var exception = Should.Throw<InvalidOperationException>(
            () => new ModuleRegistry([new TestModule("Directory"), new TestModule("Directory")]));

        exception.Message.ShouldContain("Directory");
    }

    [Theory]
    [InlineData("")]
    [InlineData("directory")]
    [InlineData("Deal-Roles")]
    [InlineData("Access2")]
    public void Constructor_NameNotPascalCaseLetters_Throws(string name)
    {
        var exception = Should.Throw<InvalidOperationException>(() => new ModuleRegistry([new TestModule(name)]));

        exception.Message.ShouldContain("PascalCase");
    }
}
