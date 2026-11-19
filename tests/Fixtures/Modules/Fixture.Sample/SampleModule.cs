using System.Reflection;
using Crm.Mediator;
using Crm.SharedKernel.Modules;
using Fixture.Sample.Handlers;
using Fixture.Sample.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fixture.Sample;

public sealed class SampleModule : IModule
{
    public string Name => "Sample";

    public Assembly ControllersAssembly => typeof(SampleModule).Assembly;

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ISampleService, SampleService>();
        services.AddRequestHandler<GetSampleSummaryHandler>();
    }
}
