using Crm.Mediator.Tests.Contracts.Directory;
using Crm.SharedKernel.Modules;
using Crm.Testing.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Mediator.Tests
{
    /// <summary>Startup validation over a contracts namespace (ADR-013); production uses Crm.Mediator.Contracts.</summary>
    public sealed class ModuleHandlerValidationTests
    {
        private const string ContractsNamespace = "Crm.Mediator.Tests.Contracts";
        private static readonly IConfiguration EmptyConfiguration = new ConfigurationBuilder().Build();

        [Fact]
        public void Startup_RequestWithNoHandler_FailsValidation()
        {
            var exception = Should.Throw<InvalidOperationException>(() => Compose(new TestModule("Directory")));

            exception.Message.ShouldContain("Crm.Mediator.Tests.Contracts.Directory.CountEntries has no handler");
        }

        [Fact]
        public void Startup_RequestWithTwoHandlers_FailsValidation()
        {
            var module = new TestModule("Directory", addServices: services => services
                .AddRequestHandler<CountEntriesHandler>()
                .AddRequestHandler<CountEntriesHandler>());

            var exception = Should.Throw<InvalidOperationException>(() => Compose(module));

            exception.Message.ShouldContain("Crm.Mediator.Tests.Contracts.Directory.CountEntries has 2 handlers");
        }

        [Fact]
        public void Startup_EveryRequestHasOneHandler_Succeeds()
        {
            var module = new TestModule("Directory", addServices: services => services.AddRequestHandler<CountEntriesHandler>());

            Should.NotThrow(() => Compose(module));
        }

        private static IServiceCollection Compose(TestModule module)
        {
            var registry = new ModuleRegistry([module]);
            return new ServiceCollection()
                .AddModules(registry, EmptyConfiguration)
                .AddMediator(registry, typeof(CountEntries).Assembly, ContractsNamespace);
        }
    }

    internal sealed class CountEntriesHandler : IRequestHandler<CountEntries, int>
    {
        public Task<int> Handle(CountEntries request, CancellationToken cancellationToken) => Task.FromResult(0);
    }
}

namespace Crm.Mediator.Tests.Contracts.Directory
{
    /// <summary>A test stand-in for a message in Crm.Mediator.Contracts.Directory.</summary>
    public sealed record CountEntries : IQuery<int>;
}
