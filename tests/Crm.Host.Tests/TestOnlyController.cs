using Crm.Platform.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Host.Tests;

/// <summary>Throwaway endpoints that exist only in the test host (added with AddApplicationPart).</summary>
[ApiController]
[Route("api/v1/test-only")]
public sealed class TestOnlyController : ControllerBase
{
    public const string MissingPath = "/api/v1/test-only/missing";

    /// <summary>No policy: the fallback policy must protect it.</summary>
    [HttpGet("protected")]
    public IActionResult Protected() => Ok("secret");

    /// <summary>An unexpected failure the exception filter does not handle.</summary>
    [HttpGet("throws")]
    public IActionResult Throws() => throw new InvalidOperationException($"{TestHostFactory.ExceptionDetail} at {Request.Path}");

    [HttpGet("missing")]
    public IActionResult Missing() => throw new NotFoundException($"No test record at {Request.Path}.");
}
