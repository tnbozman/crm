using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Host.Tests;

/// <summary>
/// The host serves the Vue build from wwwroot. Needs a frontend build first (make build);
/// make test-int runs with SkipFrontend=true and relies on it.
/// </summary>
[Trait("Category", "Integration")]
public sealed class SpaHostingTests(TestHostFactory factory) : IClassFixture<TestHostFactory>
{
    [Fact]
    public async Task Get_Root_ReturnsSpaIndexHtml()
    {
        AssertFrontendIsBuilt();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("text/html");
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldContain("<div id=\"app\">");
    }

    [Fact]
    public async Task Get_ClientRoute_ReturnsIndexHtml()
    {
        AssertFrontendIsBuilt();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/some/client/route", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("text/html");
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldContain("<div id=\"app\">");
    }

    [Fact]
    public async Task Get_UnknownApiRoute_Returns404ProblemDetailsNotHtml()
    {
        AssertFrontendIsBuilt();
        using var client = factory.CreateSignedInClient();

        using var response = await client.GetAsync(new Uri("/api/v1/unknown", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task Get_UnknownApiRoute_WithoutAuth_Returns401NotHtml()
    {
        AssertFrontendIsBuilt();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/api/v1/unknown", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/problem+json");
    }

    private void AssertFrontendIsBuilt()
    {
        var webRoot = factory.Services.GetRequiredService<IWebHostEnvironment>().WebRootPath;
        File.Exists(Path.Combine(webRoot ?? string.Empty, "index.html"))
            .ShouldBeTrue($"wwwroot/index.html not found under '{webRoot}'. Run make build first.");
    }
}
