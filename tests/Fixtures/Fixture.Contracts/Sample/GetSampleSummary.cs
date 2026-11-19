using Crm.Mediator;

namespace Fixture.Contracts.Sample;

public sealed record GetSampleSummary(Guid SampleId) : IQuery<SampleSummary>;
