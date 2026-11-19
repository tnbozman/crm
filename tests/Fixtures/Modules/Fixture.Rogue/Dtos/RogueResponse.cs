using Fixture.Rogue.Models;

namespace Fixture.Rogue.Dtos;

/// <summary>Violations: a public DTO (only the IModule class is public), and a DTO that knows a Model.</summary>
public sealed record RogueResponse(Guid Id)
{
    internal static RogueResponse From(RogueItem item) => new(item.Id);
}
