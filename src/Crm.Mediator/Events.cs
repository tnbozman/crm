namespace Crm.Mediator;

/// <summary>Tells other modules something changed. Written to the module's outbox; published only by its relay (ADR-009).</summary>
public interface IIntegrationEvent;

public interface IEventSubscriber<in TEvent>
    where TEvent : IIntegrationEvent
{
    /// <summary>Handles one delivery. <paramref name="envelope"/> carries the message ID for the subscriber's inbox.</summary>
    Task Handle(TEvent integrationEvent, MessageEnvelope envelope, CancellationToken cancellationToken);
}
