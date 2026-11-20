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

    public const int MaxAttempts = 10;

    public static readonly TimeSpan FirstBackoff = TimeSpan.FromSeconds(5);

    public static readonly TimeSpan MaxBackoff = TimeSpan.FromHours(1);

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
            try
            {
                // Each delivery gets its own scope, so subscribers get their own DbContexts and transactions.
                await using (var scope = scopes.CreateAsyncScope())
                {
                    await scope.ServiceProvider.GetRequiredService<IEventPublisher>()
                        .Publish(message.ToEnvelope(), cancellationToken).ConfigureAwait(false);
                }

                message.MarkProcessed(clock.GetUtcNow());
                published++;
                LogPublished(message.MessageId, message.Type);
            }
            catch (Exception error) when (!cancellationToken.IsCancellationRequested)
            {
                // A failing subscriber must not stop the relay: the message is retried, then dead-lettered.
                message.RecordFailure(error, clock.GetUtcNow(), MaxAttempts, FirstBackoff, MaxBackoff);
                LogFailed(error, message.MessageId, message.Type, message.Attempts, message.DeadLetteredAt is not null);
            }

            // Saved per message, even when stopping, so a restart neither loses nor repeats this one.
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
        }

        return published;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Outbox relay __Module__ published {MessageId} ({MessageType})")]
    private partial void LogPublished(Guid messageId, string messageType);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Outbox relay __Module__ failed to publish {MessageId} ({MessageType}), attempt {Attempts}; dead-lettered: {DeadLettered}")]
    private partial void LogFailed(Exception error, Guid messageId, string messageType, int attempts, bool deadLettered);
}
