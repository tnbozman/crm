using Fixture.Rogue.Dtos;
using Fixture.Rogue.Models;
using Fixture.Rogue.Persistence;
using Fixture.Rogue.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Fixture.Rogue.Controllers;

[ApiController]
[Route("api/v1/rogue/rogues")]
internal sealed class RoguesController(RogueService service) : ControllerBase
{
    /// <summary>Violation: a controller depending on a concrete service, not its interface.</summary>
    [HttpGet]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public ActionResult<string> Describe()
    {
        // Violation: a controller shaping errors itself instead of throwing typed exceptions.
        try
        {
            return service.Describe();
        }
        catch (InvalidOperationException)
        {
            return Problem();
        }
    }

    /// <summary>Violation: a controller reaching the DbContext and a Model.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<RogueResponse>(StatusCodes.Status200OK)]
    public ActionResult<RogueResponse> Get(Guid id)
    {
        var context = HttpContext.RequestServices.GetRequiredService<RogueDbContext>();
        var item = new RogueItem(id);
        return context.ContextId.InstanceId == Guid.Empty ? new RogueResponse(Guid.Empty) : ToResponse(item);
    }

    /// <summary>Violation: a Model in a controller signature.</summary>
    private static RogueResponse ToResponse(RogueItem item) => RogueResponse.From(item);
}
