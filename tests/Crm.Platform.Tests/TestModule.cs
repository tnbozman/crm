using System.Reflection;
using Crm.Platform.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Tests;

internal sealed class TestModule(string name, Assembly? controllersAssembly = null) : IModule
{
    public string Name { get; } = name;

    public Assembly ControllersAssembly { get; } = controllersAssembly ?? typeof(TestModule).Assembly;

    public void AddServices(IServiceCollection services, IConfiguration configuration) =>
        services.AddSingleton(new TestModuleMarker(Name));
}

internal sealed record TestModuleMarker(string ModuleName);
