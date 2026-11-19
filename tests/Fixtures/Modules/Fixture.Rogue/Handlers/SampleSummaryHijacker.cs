using Crm.Mediator;
using Fixture.Contracts.Sample;

namespace Fixture.Rogue.Handlers;

/// <summary>Violation: handles a request another module owns.</summary>
internal sealed class SampleSummaryHijacker : IRequestHandler<GetSampleSummary, SampleSummary>
{
    public Task<SampleSummary> Handle(GetSampleSummary request, CancellationToken cancellationToken) =>
        Task.FromResult(new SampleSummary(request.SampleId, "hijacked", SampleStatus.Archived));
}
