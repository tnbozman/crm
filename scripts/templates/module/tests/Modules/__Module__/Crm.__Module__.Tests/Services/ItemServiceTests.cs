using Bogus;
using Crm.__Module__.Dtos;
using Crm.__Module__.Services;
using Crm.SharedKernel.Errors;
using Crm.Testing;
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
            created = await new ItemService(db, clock).CreateAsync(new CreateItemRequest(name), TestContext.Current.CancellationToken);
        }

        await using var readDb = database.CreateDbContext();
        var read = await new ItemService(readDb, clock).GetAsync(created.Id, TestContext.Current.CancellationToken);

        read.ShouldBe(new ItemResponse(created.Id, name, clock.GetUtcNow()));
    }

    [Fact]
    public async Task GetAsync_UnknownId_ThrowsNotFoundException()
    {
        await using var db = database.CreateDbContext();

        await Should.ThrowAsync<NotFoundException>(
            () => new ItemService(db, clock).GetAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken));
    }
}
