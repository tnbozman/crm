using Fixture.Sample.Contracts;

namespace Fixture.Sample.Services;

internal sealed class SampleService(TimeProvider clock) : ISampleService
{
    public SampleSummary Get(Guid sampleId) =>
        new(sampleId, $"Sample {clock.GetUtcNow():yyyy}", SampleStatus.Active);
}
