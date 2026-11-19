using Crm.Mediator;

namespace Fixture.Sample.Contracts;

public sealed record GetSampleSummary(Guid SampleId) : IQuery<SampleSummary>;
