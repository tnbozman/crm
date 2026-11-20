using Crm.SharedKernel.Security;
using Crm.Yyy.Dtos;
using Crm.Yyy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Yyy.Controllers;

[ApiController]
[Route("api/v1/yyy/zzz-items")]
[Authorize(Policy = Policies.Admin)]
internal sealed class ZzzItemsController(IZzzMirrorService mirror) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ZzzItemResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ZzzItemResponse> Get(Guid id, CancellationToken cancellationToken) => mirror.GetZzzItemAsync(id, cancellationToken);
}
