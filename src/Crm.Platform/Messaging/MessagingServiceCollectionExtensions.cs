using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Messaging;

public static class MessagingServiceCollectionExtensions
{
    /// <summary>Registers the mediator and its pipeline. Call once, in the host.</summary>
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddScoped<Mediator>();
        services.AddScoped<IMediator>(sp => sp.GetRequiredService<Mediator>());
        services.AddScoped<IEventPublisher>(sp => sp.GetRequiredService<Mediator>());
        services.AddScoped<MessageContext>();
        services.AddScoped<IMessageContext>(sp => sp.GetRequiredService<MessageContext>());

        // The pipeline, outermost first (ADR-009).
        services.AddScoped<IRequestBehaviour, CorrelationBehaviour>();
        services.AddScoped<IRequestBehaviour, LoggingBehaviour>();
        EventTypesIn(services);
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

    /// <summary>Registers <typeparamref name="TSubscriber"/> for every integration event it handles. Call from IModule.AddServices.</summary>
    public static IServiceCollection AddEventSubscriber<TSubscriber>(this IServiceCollection services)
        where TSubscriber : class
    {
        var eventTypes = EventTypesIn(services);
        foreach (var subscriberInterface in typeof(TSubscriber).GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventSubscriber<>)))
        {
            services.AddScoped(subscriberInterface, typeof(TSubscriber));
            eventTypes.Add(subscriberInterface.GetGenericArguments()[0]);
        }

        return services;
    }

    /// <summary>The one EventTypes instance for this service collection, added on first use.</summary>
    private static EventTypes EventTypesIn(IServiceCollection services)
    {
        if (services.FirstOrDefault(d => d.ServiceType == typeof(EventTypes))?.ImplementationInstance is EventTypes existing)
        {
            return existing;
        }

        var eventTypes = new EventTypes();
        services.AddSingleton(eventTypes);
        return eventTypes;
    }
}
