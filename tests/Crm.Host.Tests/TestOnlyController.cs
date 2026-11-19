using Crm.Platform.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Host.Tests;

/// <summary>Throwaway endpoints that exist only in the test host (added with AddApplicationPart).</summary>
[ApiController]
[Route("api/v1/test-only")]
public sealed class TestOnlyController : ControllerBase
{
    public const string MissingPath = "/api/v1/test-only/missing";

    [HttpGet("missing")]
    public IActionResult Missing() => throw new NotFoundException($"No test record at {Request.Path}.");
}
