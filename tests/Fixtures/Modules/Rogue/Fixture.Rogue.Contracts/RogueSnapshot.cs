using Microsoft.EntityFrameworkCore;

namespace Fixture.Rogue.Contracts;

/// <summary>Violation: a Contracts type depending on EF Core.</summary>
public sealed record RogueSnapshot(EntityState State);
