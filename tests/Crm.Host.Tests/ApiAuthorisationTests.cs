using System.Net;
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
}
