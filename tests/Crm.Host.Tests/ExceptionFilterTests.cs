using System.Net;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace Crm.Host.Tests;

public sealed class ExceptionFilterTests(TestHostFactory factory) : IClassFixture<TestHostFactory>
{
    [Fact]
    public async Task ServiceException_IsConvertedToProblemDetails_ByExceptionFilter()
    {
        using var client = factory.CreateSignedInClient("Production");

        using var response = await client.GetAsync(new Uri(TestOnlyController.MissingPath, UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/problem+json");
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        problem.RootElement.GetProperty("status").GetInt32().ShouldBe(404);
    }
}
