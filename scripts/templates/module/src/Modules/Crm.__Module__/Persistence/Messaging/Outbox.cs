using Crm.Mediator;
using Crm.SharedKernel.Security;

namespace Crm.__Module__.Persistence.Messaging;

/// <summary>Writes integration events to this module's outbox. Services use it; only the relay publishes (ADR-009).</summary>
internal interface IOutbox
{
    /// <summary>
    /// Adds the event to the DbContext, so the caller's next SaveChanges writes it in the same transaction as the change.
    /// Returns its message ID.
    /// </summary>
    Guid Enqueue(IIntegrationEvent integrationEvent);
}

/// <summary>Generated; keep in step with the template.</summary>
internal sealed class Outbox(__Module__DbContext db, IMessageContext messages, ICurrentUser user, TimeProvider clock) : IOutbox
{
    public Guid Enqueue(IIntegrationEvent integrationEvent)
    {
        var correlationId = messages.CorrelationId == Guid.Empty ? Guid.NewGuid() : messages.CorrelationId;
        var envelope = MessageEnvelope.For(
            integrationEvent, correlationId, messages.CausationId, user.IsAuthenticated ? user.UserId : null, clock);
        db.OutboxMessages.Add(new OutboxMessage(envelope));
        return envelope.MessageId;
    }
}
