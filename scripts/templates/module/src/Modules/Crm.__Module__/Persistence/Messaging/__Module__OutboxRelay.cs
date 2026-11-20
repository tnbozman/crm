using Crm.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Crm.__Module__.Persistence.Messaging;

/// <summary>
/// Publishes this module's outbox through the mediator, after the commit (ADR-009). Reads only this module's outbox.
/// Generated; keep in step with the template.
/// </summary>
internal sealed partial class __Module__OutboxRelay(
    __Module__DbContext db, IServiceScopeFactory scopes, TimeProvider clock, ILogger<__Module__OutboxRelay> logger)
{
    public const int BatchSize = 50;

    /// <summary>Publishes the due messages, oldest first, and returns how many were published.</summary>
    public async Task<int> RunOnceAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var due = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.DeadLetteredAt == null && m.NextAttemptAt <= now)
            .OrderBy(m => m.Sequence)
            .Take(BatchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var published = 0;
        foreach (var message in due)
        {
            // Each delivery gets its own scope, so subscribers get their own DbContexts and transactions.
            await using (var scope = scopes.CreateAsyncScope())
            {
                await scope.ServiceProvider.GetRequiredService<IEventPublisher>()
                    .Publish(message.ToEnvelope(), cancellationToken).ConfigureAwait(false);
            }

            message.MarkProcessed(clock.GetUtcNow());
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            published++;
            LogPublished(message.MessageId, message.Type);
        }

        return published;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Outbox relay __Module__ published {MessageId} ({MessageType})")]
    private partial void LogPublished(Guid messageId, string messageType);
}
