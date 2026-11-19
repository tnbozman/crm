using System.Reflection;
using Crm.SharedKernel.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Testing.Modules;

/// <summary>
/// A module for composition tests. Its Contracts default to an assembly with no requests; pass this test assembly
/// with requests to make them the module's own, and <paramref name="addServices"/> to register handlers.
/// </summary>
public sealed class TestModule(
    string name,
    Assembly? controllersAssembly = null,
    Assembly? contractsAssembly = null,
    Action<IServiceCollection>? addServices = null) : IModule
{
    public string Name { get; } = name;

    public Assembly ControllersAssembly { get; } = controllersAssembly ?? typeof(TestModule).Assembly;

    public Assembly ContractsAssembly { get; } = contractsAssembly ?? typeof(object).Assembly;

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(new TestModuleMarker(Name));
        addServices?.Invoke(services);
    }
}

public sealed record TestModuleMarker(string ModuleName);
