using Crm.Platform.Messaging;

namespace Fixture.Sample.Contracts;

public sealed record GetSampleSummary(Guid SampleId) : IQuery<SampleSummary>;
