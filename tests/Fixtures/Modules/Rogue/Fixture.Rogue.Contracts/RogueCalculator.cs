namespace Fixture.Rogue.Contracts;

/// <summary>Violation: a class with behaviour in Contracts.</summary>
public sealed class RogueCalculator
{
    public int Total { get; private set; }

    public void Add(int amount) => Total += amount;
}
