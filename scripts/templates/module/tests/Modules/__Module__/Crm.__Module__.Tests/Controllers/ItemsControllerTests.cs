using System.Net;
using System.Net.Http.Json;
using Crm.__Module__.Dtos;
using Crm.SharedKernel.Security;
using Crm.Testing;
using Crm.Testing.Builders;
using Shouldly;
using Xunit;

namespace Crm.__Module__.Tests.Controllers;

[Trait("Category", TestCategories.Integration)]
public sealed class ItemsControllerTests(__Module__Database database) : IClassFixture<__Module__Database>, IDisposable
{
    private const string ItemsPath = "/api/v1/__module__/items";

    private readonly __Module__ApiFactory factory = new(database.ConnectionString);

    [Fact]
    public async Task Post_AsAdmin_Returns201_AndTheItemCanBeRead()
    {
        using var client = factory.CreateClientFor(new TestUserBuilder().WithRoles(Roles.Admin).Build());

        using var created = await client.PostAsJsonAsync(ItemsPath, new CreateItemRequest("Example"), TestContext.Current.CancellationToken);

        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var item = (await created.Content.ReadFromJsonAsync<ItemResponse>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
        var read = await client.GetFromJsonAsync<ItemResponse>(created.Headers.Location, TestContext.Current.CancellationToken);
        read.ShouldBe(item);
    }

    [Fact]
    public async Task Post_WithBlankName_Returns400()
    {
        using var client = factory.CreateClientFor(new TestUserBuilder().WithRoles(Roles.Admin).Build());

        using var response = await client.PostAsJsonAsync(ItemsPath, new CreateItemRequest(""), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_AsReadOnly_Returns403()
    {
        using var client = factory.CreateClientFor(new TestUserBuilder().WithRoles(Roles.ReadOnly).Build());

        using var response = await client.GetAsync(new Uri($"{ItemsPath}/{Guid.CreateVersion7()}", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Get_Anonymous_Returns401()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri($"{ItemsPath}/{Guid.CreateVersion7()}", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    public void Dispose() => factory.Dispose();
}
