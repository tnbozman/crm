using Fixture.Sample.Dtos;
using Fixture.Sample.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fixture.Sample.Controllers;

[ApiController]
[Route("api/v1/sample/samples")]
internal sealed class SamplesController(ISampleService samples) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<SampleResponse>(StatusCodes.Status200OK)]
    public ActionResult<SampleResponse> Get(Guid id, CancellationToken cancellationToken) => samples.Get(id);
}
