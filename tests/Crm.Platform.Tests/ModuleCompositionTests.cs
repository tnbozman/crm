using Crm.Platform.Messaging;
using Crm.Platform.Modules;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
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

    [Fact]
    public void AddModules_Registry_IsResolvable()
    {
        var registry = new ModuleRegistry([new TestModule("Directory")]);
        var services = new ServiceCollection();

        services.AddModules(registry, EmptyConfiguration);

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ModuleRegistry>().ShouldBeSameAs(registry);
    }

    [Fact]
    public void AddModules_InternalControllerInModuleAssembly_IsDiscovered()
    {
        var services = new ServiceCollection();

        services.AddModules(new ModuleRegistry([new TestModule("Directory", typeof(InternalTestController).Assembly)]), EmptyConfiguration);

        DiscoveredControllers(services).ShouldContain(typeof(InternalTestController));
    }

    [Fact]
    public void AddModules_InternalControllerOutsideModuleAssemblies_IsNotDiscovered()
    {
        var services = new ServiceCollection();

        services.AddModules(new ModuleRegistry([new TestModule("Directory", typeof(Should).Assembly)]), EmptyConfiguration);
        services.AddControllers().AddApplicationPart(typeof(InternalTestController).Assembly); // a part, but not a module

        DiscoveredControllers(services).ShouldNotContain(typeof(InternalTestController));
    }

    [Fact]
    public void Startup_RequestWithNoHandler_FailsValidation()
    {
        var module = new TestModule("Directory", contractsAssembly: typeof(EchoQuery).Assembly);

        var exception = Should.Throw<InvalidOperationException>(
            () => new ServiceCollection().AddModules(new ModuleRegistry([module]), EmptyConfiguration));

        exception.Message.ShouldContain("Crm.Platform.Tests.EchoQuery has no handler");
    }

    [Fact]
    public void Startup_RequestWithTwoHandlers_FailsValidation()
    {
        var module = new TestModule("Directory", contractsAssembly: typeof(EchoQuery).Assembly, addServices: services => services
            .AddRequestHandler<EchoQueryHandler>()
            .AddRequestHandler<EchoQueryHandler>());

        var exception = Should.Throw<InvalidOperationException>(
            () => new ServiceCollection().AddModules(new ModuleRegistry([module]), EmptyConfiguration));

        exception.Message.ShouldContain("Crm.Platform.Tests.EchoQuery has 2 handlers");
    }

    private static List<Type> DiscoveredControllers(ServiceCollection services)
    {
        var partManager = (ApplicationPartManager)services.Single(d => d.ServiceType == typeof(ApplicationPartManager)).ImplementationInstance!;
        var feature = new ControllerFeature();
        partManager.PopulateFeature(feature);
        return feature.Controllers.Select(c => c.AsType()).ToList();
    }
}
