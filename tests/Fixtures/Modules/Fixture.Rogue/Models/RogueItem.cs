namespace Fixture.Rogue.Models;

internal sealed class RogueItem(Guid id)
{
    public Guid Id { get; private set; } = id;
}
