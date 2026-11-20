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

    /// <summary>Write order, assigned by the database; the relay publishes in this order.</summary>
    public long Sequence { get; private set; }

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

    public void MarkProcessed(DateTimeOffset at) => ProcessedAt = at;

    /// <summary>
    /// Records a failed delivery: retried after an exponential backoff, dead-lettered after <paramref name="maxAttempts"/>.
    /// Only the exception type is stored: messages can hold data that does not belong in the outbox.
    /// </summary>
    public void RecordFailure(Exception error, DateTimeOffset at, int maxAttempts, TimeSpan firstBackoff, TimeSpan maxBackoff)
    {
        ArgumentNullException.ThrowIfNull(error);
        Attempts++;
        LastError = error.GetType().FullName;
        if (Attempts >= maxAttempts)
        {
            DeadLetteredAt = at;
            return;
        }

        var backoff = firstBackoff * Math.Pow(2, Attempts - 1);
        NextAttemptAt = at + (backoff < maxBackoff ? backoff : maxBackoff);
    }

    public MessageEnvelope ToEnvelope() =>
        new(MessageId, Type, OccurredAt, Payload, CorrelationId, CausationId, OriginatingUserId);
}
