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

    private void AssertFrontendIsBuilt()
    {
        var webRoot = factory.Services.GetRequiredService<IWebHostEnvironment>().WebRootPath;
        File.Exists(Path.Combine(webRoot ?? string.Empty, "index.html"))
            .ShouldBeTrue($"wwwroot/index.html not found under '{webRoot}'. Run make build first.");
    }
}
