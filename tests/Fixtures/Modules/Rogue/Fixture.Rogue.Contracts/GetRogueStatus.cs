using Crm.Mediator;

namespace Fixture.Rogue.Contracts;

/// <summary>Violation: a request with no handler.</summary>
public sealed record GetRogueStatus(Guid RogueId) : IQuery<string>;
