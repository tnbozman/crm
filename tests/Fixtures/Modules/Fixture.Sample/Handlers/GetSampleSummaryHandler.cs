using Crm.Mediator;
using Fixture.Contracts.Sample;
using Fixture.Sample.Services;

namespace Fixture.Sample.Handlers;

/// <summary>Thin: delegates to the module's service (ADR-009).</summary>
internal sealed class GetSampleSummaryHandler(ISampleService samples) : IRequestHandler<GetSampleSummary, SampleSummary>
{
    public Task<SampleSummary> Handle(GetSampleSummary request, CancellationToken cancellationToken)
    {
        var sample = samples.Get(request.SampleId);
        return Task.FromResult(new SampleSummary(sample.Id, sample.Name, SampleStatus.Active));
    }
}
