using System.Net;
using Shouldly;
using Xunit;

namespace Crm.Host.Tests;

public sealed class ErrorHandlingTests(TestHostFactory factory) : IClassFixture<TestHostFactory>
{
    [Fact]
    public async Task GetFailingEndpoint_InProduction_Returns500ProblemDetailsWithoutStackTrace()
    {
        using var client = factory.CreateSignedInClient("Production");

        using var response = await client.GetAsync(new Uri(TestHostFactory.ThrowingPath, UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.ShouldNotContain(TestHostFactory.ExceptionDetail);
        body.ShouldNotContain(nameof(InvalidOperationException));
        body.ShouldNotContain("   at ");
    }
}
