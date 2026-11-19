using Crm.Platform.Modules;
using Shouldly;
using Xunit;

namespace Crm.Platform.Tests;

public sealed class ModuleSchemaTests
{
    [Fact]
    public void Schema_PascalCaseName_IsLowercasedName()
    {
        new TestModule("Programmes").Schema().ShouldBe("programmes");
    }
}
