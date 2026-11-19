using Fixture.Rogue.Dtos;
using Fixture.Rogue.Models;
using Fixture.Rogue.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Fixture.Rogue.Controllers;

[ApiController]
[Route("api/v1/rogue/rogues")]
internal sealed class RoguesController : ControllerBase
{
    /// <summary>Violation: a controller reaching the DbContext and a Model.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<RogueResponse>(StatusCodes.Status200OK)]
    public ActionResult<RogueResponse> Get(Guid id)
    {
        var context = HttpContext.RequestServices.GetRequiredService<RogueDbContext>();
        var item = new RogueItem(id);
        return new RogueResponse(context.ContextId.InstanceId == Guid.Empty ? Guid.Empty : item.Id);
    }
}
