using Crm.Platform.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Platform.Tests;

public sealed class ModuleCompositionTests
{
    private static readonly IConfiguration EmptyConfiguration = new ConfigurationBuilder().Build();

    [Fact]
    public void AddModules_TwoModules_RegistersEachModulesServices()
    {
        var services = new ServiceCollection();

        services.AddModules(new ModuleRegistry([new TestModule("Directory"), new TestModule("Deals")]), EmptyConfiguration);

        using var provider = services.BuildServiceProvider();
        provider.GetServices<TestModuleMarker>().Select(m => m.ModuleName).ShouldBe(["Directory", "Deals"]);
    }
}
