using Crm.__Module__.Models;
using Crm.__Module__.Persistence.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Crm.__Module__.Persistence;

/// <summary>The module's only DbContext, in its own schema (ADR-013). Its migrations history lives in that schema too.</summary>
internal sealed class __Module__DbContext(DbContextOptions<__Module__DbContext> options) : DbContext(options)
{
    public const string Schema = "__module__";

    public DbSet<Item> Items => Set<Item>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(__Module__DbContext).Assembly);
    }
}
