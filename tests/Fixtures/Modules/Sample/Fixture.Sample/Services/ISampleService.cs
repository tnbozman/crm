using Fixture.Sample.Contracts;

namespace Fixture.Sample.Services;

internal interface ISampleService
{
    SampleSummary Get(Guid sampleId);
}
