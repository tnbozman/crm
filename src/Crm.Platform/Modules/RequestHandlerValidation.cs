using Crm.Platform.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Modules;

/// <summary>Startup check: every request in a module's Contracts has exactly one registered handler (ADR-009).</summary>
internal static class RequestHandlerValidation
{
    public static void ThrowIfInvalid(IServiceCollection services, ModuleRegistry registry)
    {
        var problems = registry.Modules
            .SelectMany(m => m.ContractsAssembly.GetTypes())
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                .Select(i => typeof(IRequestHandler<,>).MakeGenericType(t, i.GetGenericArguments()[0])))
            .Select(handlerType => (Request: handlerType.GetGenericArguments()[0], Count: services.Count(d => d.ServiceType == handlerType)))
            .Where(x => x.Count != 1)
            .Select(x => x.Count == 0 ? $"{x.Request.FullName} has no handler" : $"{x.Request.FullName} has {x.Count} handlers")
            .ToList();

        if (problems.Count > 0)
        {
            throw new InvalidOperationException("Mediator handlers are misconfigured: " + string.Join("; ", problems) + ".");
        }
    }
}
