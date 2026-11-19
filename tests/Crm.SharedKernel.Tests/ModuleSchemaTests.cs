using Crm.SharedKernel.Modules;
using Crm.Testing.Modules;
using Shouldly;
using Xunit;

namespace Crm.SharedKernel.Tests;

public sealed class ModuleSchemaTests
{
    [Fact]
    public void Schema_PascalCaseName_IsLowercasedName()
    {
        new TestModule("Programmes").Schema().ShouldBe("programmes");
    }
}
