using System.Reflection;
using Crm.Platform.Modules;
using Fixture.Sample;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fixture.Rogue;

public sealed class RogueModule : IModule
{
    public string Name => "Rogue";

    public Assembly ControllersAssembly => typeof(RogueModule).Assembly;

    // Violation: registers (and so can resolve) a type from another module's implementation.
    public void AddServices(IServiceCollection services, IConfiguration configuration) =>
        services.AddSingleton<SampleModule>();
}
