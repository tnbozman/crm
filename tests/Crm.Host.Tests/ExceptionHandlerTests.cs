using System.Net;
using System.Text.Json;
using Crm.Platform.Errors;
using Crm.Platform.Testing.Builders;
using Crm.Platform.Testing.Logging;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace Crm.Host.Tests;

public sealed class ExceptionHandlerTests(TestHostFactory factory) : IClassFixture<TestHostFactory>
{
    [Fact]
    public async Task ServiceException_IsConvertedToProblemDetails_ByExceptionHandler()
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

    [Fact]
    public async Task ExpectedException_IsNotLoggedAsAWarningOrError()
    {
        using var logs = new CapturingLoggerProvider();
        using var client = factory.CreateClientFor(new TestUserBuilder().Build(), "Production", logs);

        using var response = await client.GetAsync(new Uri(TestOnlyController.ForbiddenPath, UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        logs.Entries
            .Where(e => e.Level >= LogLevel.Warning)
            .Where(e => e.Category.StartsWith("Microsoft.AspNetCore.Diagnostics", StringComparison.Ordinal)
                || e.Line.Contains(nameof(ForbiddenException), StringComparison.Ordinal))
            .ShouldBeEmpty("a 4xx the caller caused is not a server fault");
    }

    [Fact]
    public async Task UnexpectedException_IsLoggedAsAnError()
    {
        using var logs = new CapturingLoggerProvider();
        using var client = factory.CreateClientFor(new TestUserBuilder().Build(), "Production", logs);

        using var response = await client.GetAsync(new Uri(TestHostFactory.ThrowingPath, UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        logs.Entries.ShouldContain(e => e.Level == LogLevel.Error);
    }
}
