namespace Fixture.Sample.Contracts;

public readonly record struct SampleSummary(Guid SampleId, string Name, SampleStatus Status);
