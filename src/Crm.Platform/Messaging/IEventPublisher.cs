namespace Crm.Platform.Messaging;

/// <summary>
/// Delivers an integration event to every subscriber. Only a module's outbox relay uses this, after the commit
/// (ADR-009); services write events to their outbox instead. Enforced by Services_DoNotCall_MediatorPublish.
/// </summary>
public interface IEventPublisher
{
    Task Publish(MessageEnvelope envelope, CancellationToken cancellationToken = default);
}
