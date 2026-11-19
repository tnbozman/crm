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

    [Fact]
    public async Task ForbiddenException_IsConvertedTo403ProblemDetails()
    {
        using var client = factory.CreateSignedInClient("Production");

        using var response = await client.GetAsync(new Uri(TestOnlyController.ForbiddenPath, UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/problem+json");
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldNotContain("Not for this user");
    }

    [Fact]
    public async Task RequestValidationException_IsConvertedTo400ProblemDetails_WithFieldErrors()
    {
        using var client = factory.CreateSignedInClient("Production");

        using var response = await client.GetAsync(new Uri(TestOnlyController.InvalidPath, UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/problem+json");
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        problem.RootElement.GetProperty("errors").GetProperty("Name")[0].GetString().ShouldBe("Too long for GET.");
    }

    [Fact]
    public async Task ForbiddenException_ThrownOutsideMvcFilters_IsConvertedTo403ProblemDetails()
    {
        using var client = factory.CreateSignedInClient("Production");

        using var response = await client.GetAsync(new Uri(TestOnlyController.ForbiddenWhileWritingPath, UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/problem+json");
    }
}
