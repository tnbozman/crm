using Crm.Mediator;

namespace Fixture.Contracts.Rogue;

/// <summary>Violation: a request with no handler.</summary>
public sealed record GetRogueStatus(Guid RogueId) : IQuery<string>;
