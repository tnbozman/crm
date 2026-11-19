using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace Crm.Host.Tests;

public sealed class HealthEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetHealth_Anonymous_Returns200WithStatusOk()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/api/v1/health", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<HealthBody>(TestContext.Current.CancellationToken);
        body.ShouldNotBeNull().Status.ShouldBe("ok");
    }

    private sealed record HealthBody(string Status);
}
