using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator>();
        return services;
    }

    /// <summary>Registers <typeparamref name="THandler"/> for every request it handles. Call from IModule.AddServices.</summary>
    public static IServiceCollection AddRequestHandler<THandler>(this IServiceCollection services)
        where THandler : class
    {
        foreach (var handlerInterface in typeof(THandler).GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
        {
            services.AddScoped(handlerInterface, typeof(THandler));
        }

        return services;
    }
}
