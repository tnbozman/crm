using Crm.Platform.Modules;
using Shouldly;
using Xunit;

namespace Crm.Platform.Tests;

public sealed class ModuleRegistryTests
{
    [Fact]
    public void Constructor_DuplicateModuleNames_Throws()
    {
        var exception = Should.Throw<InvalidOperationException>(
            () => new ModuleRegistry([new TestModule("Directory"), new TestModule("Directory")]));

        exception.Message.ShouldContain("Directory");
    }
}
