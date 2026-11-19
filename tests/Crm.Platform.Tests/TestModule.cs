using System.Reflection;
using Crm.Platform.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Tests;

internal sealed class TestModule(string name) : IModule
{
    public string Name { get; } = name;

    public Assembly ControllersAssembly => typeof(TestModule).Assembly;

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}
