using System.Net.Http.Json;
using Crm.__Module__.Dtos;
using Crm.SharedKernel.Security;
using Crm.Testing;
using Crm.Testing.Builders;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Crm.__Module__.Tests.Persistence;

[Trait("Category", TestCategories.Integration)]
public sealed class __Module__OutboxRelayServiceTests(__Module__HostDatabase database) : IClassFixture<__Module__HostDatabase>, IDisposable
{
    private readonly __Module__ApiFactory factory = new(database.ConnectionString);

    [Fact]
    public async Task Host_RunsTheRelay_SoACreatedItemsEventIsPublished()
    {
        using var client = factory.CreateClientFor(new TestUserBuilder().WithRoles(Roles.Admin).Build());
        using var response = await client.PostAsJsonAsync("/api/v1/__module__/items", new CreateItemRequest("Relayed"), TestContext.Current.CancellationToken);
        var item = (await response.Content.ReadFromJsonAsync<ItemResponse>(TestContext.Current.CancellationToken)).ShouldNotBeNull();

        var deadline = DateTimeOffset.UtcNow.AddSeconds(30);
        DateTimeOffset? processedAt = null;
        while (processedAt is null && DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);
            await using var db = database.CreateDbContext();
            processedAt = await db.OutboxMessages
                .Where(m => m.Payload.Contains(item.Id.ToString()))
                .Select(m => m.ProcessedAt)
                .SingleOrDefaultAsync(TestContext.Current.CancellationToken);
        }

        processedAt.ShouldNotBeNull("the module registers a hosted relay that publishes its outbox");
    }

    public void Dispose() => factory.Dispose();
}
