using System.Text.Json;
using Bogus;
using Crm.__Module__.Dtos;
using Crm.__Module__.Persistence;
using Crm.__Module__.Persistence.Messaging;
using Crm.__Module__.Services;
using Crm.Mediator.Contracts.__Module__;
using Crm.SharedKernel.Errors;
using Crm.SharedKernel.Security;
using Crm.Testing;
using Crm.Testing.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Xunit;

namespace Crm.__Module__.Tests.Services;

[Trait("Category", TestCategories.Integration)]
public sealed class ItemServiceTests(__Module__Database database) : IClassFixture<__Module__Database>
{
    private readonly FakeTimeProvider clock = new(new DateTimeOffset(2026, 12, 14, 9, 0, 0, TimeSpan.Zero));
    private readonly Faker faker = new("en_AU");

    [Fact]
    public async Task CreateAsync_ThenGetAsync_ReturnsTheStoredItem()
    {
        await database.ResetAsync();
        var name = faker.Commerce.ProductName();
        ItemResponse created;
        await using (var db = database.CreateDbContext())
        {
            created = await NewService(db).CreateAsync(new CreateItemRequest(name), TestContext.Current.CancellationToken);
        }

        await using var readDb = database.CreateDbContext();
        var read = await NewService(readDb).GetAsync(created.Id, TestContext.Current.CancellationToken);

        read.ShouldBe(new ItemResponse(created.Id, name, clock.GetUtcNow()));
    }

    [Fact]
    public async Task CreateAsync_WritesItemCreatedToTheOutbox()
    {
        await using var db = database.CreateDbContext();

        var created = await NewService(db).CreateAsync(new CreateItemRequest("Example"), TestContext.Current.CancellationToken);

        await using var readDb = database.CreateDbContext();
        var message = await readDb.OutboxMessages.SingleAsync(
            m => m.Type == typeof(__Module__ItemCreated).FullName && m.Payload.Contains(created.Id.ToString()), TestContext.Current.CancellationToken);
        JsonSerializer.Deserialize<__Module__ItemCreated>(message.Payload, JsonSerializerOptions.Web)
            .ShouldBe(new __Module__ItemCreated(created.Id, "Example"));
    }

    [Fact]
    public async Task GetAsync_UnknownId_ThrowsNotFoundException()
    {
        await using var db = database.CreateDbContext();

        await Should.ThrowAsync<NotFoundException>(
            () => NewService(db).GetAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken));
    }

    private ItemService NewService(__Module__DbContext db) =>
        new(db, new Outbox(db, new TestMessageContext(), CurrentUser.Anonymous, clock), clock);
}
