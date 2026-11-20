namespace Crm.__Module__.Persistence.Messaging;

/// <summary>An integration event a subscriber in this module has handled (ADR-009). Generated; keep in step with the template.</summary>
internal sealed class InboxMessage(Guid messageId, string subscriber, DateTimeOffset receivedAt)
{
    public Guid MessageId { get; private set; } = messageId;

    /// <summary>The subscriber's name: one event can reach several subscribers in a module, each runs it once.</summary>
    public string Subscriber { get; private set; } = subscriber;

    public DateTimeOffset ReceivedAt { get; private set; } = receivedAt;
}
