using System.Net;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace Crm.Host.Tests;

public sealed class ApiAuthorisationTests(TestHostFactory factory) : IClassFixture<TestHostFactory>
{
    [Fact]
    public async Task GetAnyOtherApiRoute_WithoutAuth_Returns401()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri(TestHostFactory.ProtectedPath, UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUnknownApiRoute_WithoutAuth_Returns401()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/api/v1/no-such-route", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUnknownApiRoute_Returns404ProblemDetails()
    {
        using var client = factory.CreateSignedInClient();

        using var response = await client.GetAsync(new Uri("/api/v1/no-such-route", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/problem+json");
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        problem.RootElement.GetProperty("status").GetInt32().ShouldBe(404);
    }
}
