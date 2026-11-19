using Crm.Host.Dtos;
using Crm.Host.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Host.Controllers;

/// <summary>Liveness for monitoring and the SPA shell. The only anonymous API (ADR-011).</summary>
[ApiController]
[Route("api/v1/health")]
[Authorize(Policy = HostPolicies.Anonymous)]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public HealthResponse Get() => new("ok");
}
