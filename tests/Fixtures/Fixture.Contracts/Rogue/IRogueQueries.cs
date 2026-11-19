namespace Fixture.Contracts.Rogue;

/// <summary>Violation: an interface in Contracts (cross-module calls are mediator messages, ADR-009).</summary>
public interface IRogueQueries
{
    RogueChanged Latest();
}
