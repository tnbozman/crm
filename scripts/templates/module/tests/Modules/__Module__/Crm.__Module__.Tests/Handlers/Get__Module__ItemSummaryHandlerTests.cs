using Crm.__Module__.Dtos;
using Crm.__Module__.Handlers;
using Crm.__Module__.Persistence.Messaging;
using Crm.__Module__.Services;
using Crm.Mediator.Contracts.__Module__;
using Crm.SharedKernel.Security;
using Crm.Testing;
using Crm.Testing.Outbox;
using Shouldly;
using Xunit;

namespace Crm.__Module__.Tests.Handlers;

[Trait("Category", TestCategories.Integration)]
public sealed class Get__Module__ItemSummaryHandlerTests(__Module__Database database) : IClassFixture<__Module__Database>
{
    [Fact]
    public async Task Handle_KnownItem_ReturnsItsSummary()
    {
        await using var db = database.CreateDbContext();
        var items = new ItemService(db, new Outbox(db, new TestMessageContext(), CurrentUser.Anonymous, TimeProvider.System), TimeProvider.System);
        var created = await items.CreateAsync(new CreateItemRequest("Example"), TestContext.Current.CancellationToken);

        var summary = await new Get__Module__ItemSummaryHandler(items)
            .Handle(new Get__Module__ItemSummary(created.Id), TestContext.Current.CancellationToken);

        summary.ShouldBe(new __Module__ItemSummary(created.Id, "Example"));
    }
}
