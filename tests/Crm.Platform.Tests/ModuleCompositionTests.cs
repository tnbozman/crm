using Crm.Platform.Modules;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
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

    [Fact]
    public void AddModules_Module_AddsItsControllersAssemblyAsApplicationPart()
    {
        var controllersAssembly = typeof(Should).Assembly; // any assembly other than the test assembly, which MVC adds anyway
        var services = new ServiceCollection();

        services.AddModules(new ModuleRegistry([new TestModule("Directory", controllersAssembly)]), EmptyConfiguration);

        var partManager = (ApplicationPartManager)services.Single(d => d.ServiceType == typeof(ApplicationPartManager)).ImplementationInstance!;
        partManager.ApplicationParts.OfType<AssemblyPart>().Select(p => p.Assembly).ShouldContain(controllersAssembly);
    }
}
