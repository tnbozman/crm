using Crm.SharedKernel.Modules;
using Crm.Testing.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Mediator.Tests;

public sealed class ModuleHandlerValidationTests
{
    private static readonly IConfiguration EmptyConfiguration = new ConfigurationBuilder().Build();

    [Fact]
    public void Startup_RequestWithNoHandler_FailsValidation()
    {
        var module = new TestModule("Directory", contractsAssembly: typeof(EchoQuery).Assembly);

        var exception = Should.Throw<InvalidOperationException>(
            () => AddModulesThenMediator(new ModuleRegistry([module])));

        exception.Message.ShouldContain("Crm.Mediator.Tests.EchoQuery has no handler");
    }

    [Fact]
    public void Startup_RequestWithTwoHandlers_FailsValidation()
    {
        var module = new TestModule("Directory", contractsAssembly: typeof(EchoQuery).Assembly, addServices: services => services
            .AddRequestHandler<EchoQueryHandler>()
            .AddRequestHandler<EchoQueryHandler>());

        var exception = Should.Throw<InvalidOperationException>(
            () => AddModulesThenMediator(new ModuleRegistry([module])));

        exception.Message.ShouldContain("Crm.Mediator.Tests.EchoQuery has 2 handlers");
    }

    private static IServiceCollection AddModulesThenMediator(ModuleRegistry registry) =>
        new ServiceCollection().AddModules(registry, EmptyConfiguration).AddMediator(registry);
}
