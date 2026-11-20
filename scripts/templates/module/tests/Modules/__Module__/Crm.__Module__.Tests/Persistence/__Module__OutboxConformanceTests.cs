using Crm.Testing;
using Crm.Testing.Outbox;
using Xunit;

namespace Crm.__Module__.Tests.Persistence;

/// <summary>The shared outbox, relay and inbox contract (Crm.Testing), run against this module's own DbContext.</summary>
[Trait("Category", TestCategories.Integration)]
public sealed class __Module__OutboxConformanceTests(__Module__Database database) : OutboxConformanceTests, IClassFixture<__Module__Database>
{
    protected override IOutboxHarness CreateHarness(TimeProvider clock) => new __Module__OutboxHarness(database, clock);
}
