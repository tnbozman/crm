using System.Reflection;
using Crm.Platform.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fixture.Rogue;

public sealed class RogueModule : IModule
{
    public string Name => "Rogue";

    public Assembly ControllersAssembly => typeof(RogueModule).Assembly;

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}
