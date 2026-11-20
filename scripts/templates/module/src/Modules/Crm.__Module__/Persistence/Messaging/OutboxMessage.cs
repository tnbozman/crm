using Crm.Mediator;

namespace Crm.__Module__.Persistence.Messaging;

/// <summary>An integration event waiting in this module's outbox for the relay (ADR-009). Generated; keep in step with the template.</summary>
internal sealed class OutboxMessage
{
    public OutboxMessage(MessageEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        MessageId = envelope.MessageId;
        Type = envelope.Type;
        OccurredAt = envelope.OccurredAt;
        Payload = envelope.Payload;
        CorrelationId = envelope.CorrelationId;
        CausationId = envelope.CausationId;
        OriginatingUserId = envelope.OriginatingUserId;
        NextAttemptAt = envelope.OccurredAt;
    }

    /// <summary>For EF Core.</summary>
    private OutboxMessage()
    {
        Type = string.Empty;
        Payload = string.Empty;
    }

    public Guid MessageId { get; private set; }

    public string Type { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public string Payload { get; private set; }

    public Guid CorrelationId { get; private set; }

    public Guid? CausationId { get; private set; }

    public Guid? OriginatingUserId { get; private set; }

    public int Attempts { get; private set; }

    public DateTimeOffset NextAttemptAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public DateTimeOffset? DeadLetteredAt { get; private set; }

    public string? LastError { get; private set; }

    public MessageEnvelope ToEnvelope() =>
        new(MessageId, Type, OccurredAt, Payload, CorrelationId, CausationId, OriginatingUserId);
}
