using Fixture.Sample.Dtos;
using Fixture.Sample.Mapping;
using Fixture.Sample.Models;

namespace Fixture.Sample.Services;

internal sealed class SampleService(TimeProvider clock) : ISampleService
{
    public SampleResponse Get(Guid sampleId) => new SampleItem(sampleId, $"Sample {clock.GetUtcNow():yyyy}").ToResponse();
}
