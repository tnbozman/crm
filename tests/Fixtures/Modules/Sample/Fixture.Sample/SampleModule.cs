using System.Reflection;
using Crm.Platform.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fixture.Sample;

public sealed class SampleModule : IModule
{
    public string Name => "Sample";

    public Assembly ControllersAssembly => typeof(SampleModule).Assembly;

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}
