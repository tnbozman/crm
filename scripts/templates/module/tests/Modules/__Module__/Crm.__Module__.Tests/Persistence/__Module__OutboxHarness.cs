using Crm.__Module__.Models;
using Crm.__Module__.Persistence;
using Crm.__Module__.Persistence.Messaging;
using Crm.Mediator;
using Crm.Mediator.Contracts.__Module__;
using Crm.SharedKernel.Security;
using Crm.Testing.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Crm.__Module__.Tests.Persistence;

/// <summary>Adapts the __Module__ outbox, relay and inbox to the shared conformance suite.</summary>
internal sealed class __Module__OutboxHarness(__Module__Database database, TimeProvider clock) : IOutboxHarness
{
    private const string ConformanceItemName = "Outbox conformance";

    public Task ResetAsync() => database.ResetAsync();

    public async Task<int> RunRelayOnceAsync(IEventPublisher publisher, CancellationToken cancellationToken)
    {
        await using var services = new ServiceCollection().AddSingleton(publisher).BuildServiceProvider();
        await using var db = database.CreateDbContext();
        var relay = new __Module__OutboxRelay(db, services.GetRequiredService<IServiceScopeFactory>(), clock, NullLogger<__Module__OutboxRelay>.Instance);
        return await relay.RunOnceAsync(cancellationToken);
    }

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
