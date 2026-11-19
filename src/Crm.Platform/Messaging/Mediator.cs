using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Messaging;

/// <summary>Resolves handlers from the caller's scope, so they see the caller's ICurrentUser.</summary>
internal sealed class Mediator(IServiceProvider services, EventTypes eventTypes, MessageContext context) : IMediator, IEventPublisher
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var invoker = (RequestInvoker<TResponse>)Activator.CreateInstance(
            typeof(RequestInvoker<,>).MakeGenericType(request.GetType(), typeof(TResponse)))!;
        return invoker.Invoke(request, services, cancellationToken);
    }

    public Task Publish(MessageEnvelope envelope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        context.CorrelationId = envelope.CorrelationId;
        context.CausationId = envelope.MessageId;
        if (eventTypes.Find(envelope.Type) is not { } eventType)
        {
            return Task.CompletedTask; // no module subscribes to this event
        }

        var invoker = (EventInvoker)Activator.CreateInstance(typeof(EventInvoker<>).MakeGenericType(eventType))!;
        return invoker.Invoke(envelope, services, cancellationToken);
    }
}

internal abstract class EventInvoker
{
    public abstract Task Invoke(MessageEnvelope envelope, IServiceProvider services, CancellationToken cancellationToken);
}

internal sealed class EventInvoker<TEvent> : EventInvoker
    where TEvent : IIntegrationEvent
{
    public override async Task Invoke(MessageEnvelope envelope, IServiceProvider services, CancellationToken cancellationToken)
    {
        var integrationEvent = JsonSerializer.Deserialize<TEvent>(envelope.Payload, JsonSerializerOptions.Web)!;
        foreach (var subscriber in services.GetServices<IEventSubscriber<TEvent>>())
        {
            await subscriber.Handle(integrationEvent, envelope, cancellationToken).ConfigureAwait(false);
        }
    }
}

internal abstract class RequestInvoker<TResponse>
{
    public abstract Task<TResponse> Invoke(object request, IServiceProvider services, CancellationToken cancellationToken);
}

internal sealed class RequestInvoker<TRequest, TResponse> : RequestInvoker<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Invoke(object request, IServiceProvider services, CancellationToken cancellationToken)
    {
        var handler = services.GetService<IRequestHandler<TRequest, TResponse>>()
            ?? throw new InvalidOperationException($"No handler is registered for request {typeof(TRequest).FullName}.");
        var typedRequest = (TRequest)request;

        Func<Task<TResponse>> next = () => handler.Handle(typedRequest, cancellationToken);
        foreach (var behaviour in services.GetServices<IRequestBehaviour>().Reverse())
        {
            var inner = next;
            next = () => behaviour.Handle(typedRequest, inner, cancellationToken);
        }

        return next();
    }
}
