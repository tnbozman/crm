using Crm.Mediator;
using Crm.SharedKernel.Modules;
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

    [Fact]
    public async Task Host_Started_ProvidesTheMediatorPerScope()
    {
        await using var scope = factory.Services.CreateAsyncScope();

        scope.ServiceProvider.GetService<IMediator>().ShouldNotBeNull("modules call each other only through the mediator (ADR-009)");
        scope.ServiceProvider.GetService<IEventPublisher>().ShouldNotBeNull();
    }
}
