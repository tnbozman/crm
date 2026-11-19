using Crm.Platform.Modules;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Host.Tests;

public sealed class ModuleCompositionTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public void Host_Started_ComposesHostModules()
    {
        var registry = factory.Services.GetService<ModuleRegistry>();

        registry.ShouldNotBeNull("the host composes its modules with AddModules").Modules.ShouldBe(HostModules.All);
    }
}
