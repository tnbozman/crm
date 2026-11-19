namespace Fixture.Rogue.Dtos;

/// <summary>Violation: a public DTO (only the IModule class is public in an implementation).</summary>
public sealed record RogueResponse(Guid Id);
