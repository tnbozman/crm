using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Crm.SharedKernel.Modules;
using Crm.SharedKernel.Security;
using Crm.Testing;
using Crm.Testing.Builders;
using Shouldly;
using Xunit;

namespace Crm.Host.Tests;

/// <summary>
/// S1-5 done-when: two scaffolded modules (Zzz and Yyy, in new-module.sh's temporary copy) exchange an event and a query
/// through the mediator, with neither referencing the other. Needs ConnectionStrings__Crm for a migrated database.
/// </summary>
[Trait("Category", TestCategories.Integration)]
public sealed class ModuleExchangeTests(TestHostFactory factory) : IClassFixture<TestHostFactory>
{
    [Fact]
    public async Task ZzzItemCreated_ReachesYyyThroughTheOutbox_AndYyyQueriesZzzThroughTheMediator()
    {
        Environment.GetEnvironmentVariable($"ConnectionStrings__{ModuleDatabase.ConnectionStringName}")
            .ShouldNotBeNullOrEmpty("new-module.sh sets the connection string of the database it migrated");
        var cancellation = TestContext.Current.CancellationToken;
        using var client = factory.CreateClientFor(new TestUserBuilder().WithRoles(Roles.Admin).Build());
        var name = $"Exchange {Guid.NewGuid():N}";

        using var created = await client.PostAsJsonAsync("/api/v1/zzz/items", new { name }, cancellation);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>(cancellation)).GetProperty("id").GetGuid();

        // Event: Zzz's outbox, Zzz's hosted relay, the mediator, then Yyy's subscriber (guarded by Yyy's inbox).
        var mirrored = await PollAsync(client, new Uri($"/api/v1/yyy/items/{id}", UriKind.Relative), cancellation);
        mirrored.GetProperty("name").GetString().ShouldBe($"Seen {name}");

        // Query: Yyy's endpoint sends Zzz's GetZzzItemSummary through the mediator as the signed-in user.
        var summary = await client.GetFromJsonAsync<JsonElement>(new Uri($"/api/v1/yyy/zzz-items/{id}", UriKind.Relative), cancellation);
        summary.GetProperty("name").GetString().ShouldBe(name);
    }

    private static async Task<JsonElement> PollAsync(HttpClient client, Uri path, CancellationToken cancellation)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(30);
        while (true)
        {
            using var response = await client.GetAsync(path, cancellation);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadFromJsonAsync<JsonElement>(cancellation);
            }

            DateTimeOffset.UtcNow.ShouldBeLessThan(deadline, $"{path} did not appear within 30 seconds (last status {response.StatusCode})");
            await Task.Delay(TimeSpan.FromMilliseconds(200), cancellation);
        }
    }
}
