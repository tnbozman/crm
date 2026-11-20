using Crm.__Module__.Models;
using Crm.__Module__.Persistence;
using Crm.__Module__.Persistence.Messaging;
using Crm.Mediator.Contracts.__Module__;
using Crm.SharedKernel.Security;
using Crm.Testing.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Crm.__Module__.Tests.Persistence;

/// <summary>Adapts the __Module__ outbox, relay and inbox to the shared conformance suite.</summary>
internal sealed class __Module__OutboxHarness(__Module__Database database) : IOutboxHarness
{
    private const string ConformanceItemName = "Outbox conformance";

    private readonly TimeProvider clock = TimeProvider.System;

    public async Task<Guid> SaveChangeWithEventAsync(bool commit, CancellationToken cancellationToken)
    {
        await using var db = database.CreateDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var item = new Item(Guid.CreateVersion7(), ConformanceItemName, clock.GetUtcNow());
        db.Items.Add(item);
        var messageId = new Outbox(db, new TestMessageContext(), CurrentUser.Anonymous, clock)
            .Enqueue(new __Module__ItemCreated(item.Id, item.Name));
        await db.SaveChangesAsync(cancellationToken);

        if (commit)
        {
            await transaction.CommitAsync(cancellationToken);
        }
        else
        {
            await transaction.RollbackAsync(cancellationToken);
        }

        return messageId;
    }

    public async Task<int> CountChangedEntitiesAsync(CancellationToken cancellationToken)
    {
        await using var db = database.CreateDbContext();
        return await db.Items.CountAsync(i => i.Name == ConformanceItemName, cancellationToken);
    }

    public async Task<OutboxMessageState?> FindOutboxMessageAsync(Guid messageId, CancellationToken cancellationToken)
    {
        await using var db = database.CreateDbContext();
        return await db.OutboxMessages.AsNoTracking()
            .Where(m => m.MessageId == messageId)
            .Select(m => new OutboxMessageState(m.MessageId, m.Attempts, m.NextAttemptAt, m.ProcessedAt, m.DeadLetteredAt))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
