using System.Text.Json;

namespace Crm.Platform.Messaging;

/// <summary>An integration event as stored in an outbox and delivered to handlers (ADR-009).</summary>
/// <param name="MessageId">Unique per event; subscribers' inboxes key on it.</param>
/// <param name="Type">The event's full type name.</param>
/// <param name="Payload">The event as JSON (web defaults).</param>
/// <param name="CorrelationId">Shared by everything one user action caused.</param>
/// <param name="CausationId">The message that caused this one, if any.</param>
/// <param name="OriginatingUserId">Who caused it, for audit only; handlers run as the system.</param>
public sealed record MessageEnvelope(
    Guid MessageId,
    string Type,
    DateTimeOffset OccurredAt,
    string Payload,
    Guid CorrelationId,
    Guid? CausationId,
    Guid? OriginatingUserId)
{
    public static MessageEnvelope For(
        IIntegrationEvent integrationEvent, Guid correlationId, Guid? causationId, Guid? originatingUserId, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        ArgumentNullException.ThrowIfNull(clock);

        var type = integrationEvent.GetType();
        return new MessageEnvelope(
            Guid.NewGuid(),
            type.FullName!,
            clock.GetUtcNow(),
            JsonSerializer.Serialize(integrationEvent, type, JsonSerializerOptions.Web),
            correlationId,
            causationId,
            originatingUserId);
    }
}
