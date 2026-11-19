using Crm.Platform.Errors;
using Crm.Platform.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Host.Tests;

/// <summary>Throwaway endpoints that exist only in the test host (added with AddApplicationPart).</summary>
[ApiController]
[Route("api/v1/test-only")]
public sealed class TestOnlyController : ControllerBase
{
    public const string MissingPath = "/api/v1/test-only/missing";
    public const string MePath = "/api/v1/test-only/me";
    public const string AdminOnlyPath = "/api/v1/test-only/admin-only";
    public const string ForbiddenPath = "/api/v1/test-only/forbidden";
    public const string InvalidPath = "/api/v1/test-only/invalid";
    public const string ForbiddenWhileWritingPath = "/api/v1/test-only/forbidden-while-writing";

    /// <summary>The signed-in user as services see it, through ICurrentUser.</summary>
    [HttpGet("me")]
    public IActionResult Me([FromServices] ICurrentUser user) => Ok(new
    {
        Id = user.UserId,
        Roles = user.Roles.Order(StringComparer.Ordinal).ToArray(),
        user.TeamIds,
        user.BusinessUnitIds,
    });

    /// <summary>Admins only, through the platform's role policy.</summary>
    [HttpGet("admin-only")]
    [Authorize(Policy = Policies.Admin)]
    public IActionResult AdminOnly() => Ok("admin");

    /// <summary>No policy: the fallback policy must protect it.</summary>
    [HttpGet("protected")]
    public IActionResult Protected() => Ok("secret");

    /// <summary>An unexpected failure the exception filter does not handle.</summary>
    [HttpGet("throws")]
    public IActionResult Throws() => throw new InvalidOperationException($"{TestHostFactory.ExceptionDetail} at {Request.Path}");

    [HttpGet("forbidden")]
    public IActionResult Forbidden() => throw new ForbiddenException($"Not for this user at {Request.Path}.");

    [HttpGet("invalid")]
    public IActionResult Invalid() => throw new RequestValidationException(new Dictionary<string, string[]> { ["Name"] = [$"Too long for {Request.Method}."] });

    /// <summary>Throws while the response is written: outside MVC exception filters, so only a global handler sees it.</summary>
    [HttpGet("forbidden-while-writing")]
    public IActionResult ForbiddenWhileWriting() => new ThrowingResult(new ForbiddenException($"Not for this user at {Request.Path}."));

    [HttpGet("missing")]
    public IActionResult Missing() => throw new NotFoundException($"No test record at {Request.Path}.");
}

internal sealed class ThrowingResult(Exception exception) : IActionResult
{
    public Task ExecuteResultAsync(ActionContext context) => throw exception;
}
