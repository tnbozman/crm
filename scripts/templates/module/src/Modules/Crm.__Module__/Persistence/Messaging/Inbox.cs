using Crm.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Crm.__Module__.Persistence.Messaging;

/// <summary>Makes a subscriber run once per message, however often the relay delivers it (ADR-009).</summary>
internal interface IInbox
{
    /// <summary>
    /// False when <paramref name="subscriber"/> already handled this message: return without doing anything. Otherwise
    /// adds the inbox record to the DbContext, so the subscriber's own SaveChanges commits it with its change; a delivery
    /// that fails before saving is therefore not recorded and runs again.
    /// </summary>
    Task<bool> TryReceiveAsync(MessageEnvelope envelope, string subscriber, CancellationToken cancellationToken);
}

/// <summary>Generated; keep in step with the template.</summary>
internal sealed class Inbox(__Module__DbContext db, TimeProvider clock) : IInbox
{
    public async Task<bool> TryReceiveAsync(MessageEnvelope envelope, string subscriber, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        if (await db.InboxMessages.AnyAsync(m => m.MessageId == envelope.MessageId && m.Subscriber == subscriber, cancellationToken)
            .ConfigureAwait(false))
        {
            return false;
        }

        db.InboxMessages.Add(new InboxMessage(envelope.MessageId, subscriber, clock.GetUtcNow()));
        return true;
    }
}
