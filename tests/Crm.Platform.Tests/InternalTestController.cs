using Microsoft.AspNetCore.Mvc;

namespace Crm.Platform.Tests;

[ApiController]
[Route("api/v1/test/internal")]
internal sealed class InternalTestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => NoContent();
}
