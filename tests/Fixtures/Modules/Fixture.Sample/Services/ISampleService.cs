using Fixture.Sample.Dtos;

namespace Fixture.Sample.Services;

internal interface ISampleService
{
    SampleResponse Get(Guid sampleId);
}
