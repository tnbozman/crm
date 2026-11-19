namespace Fixture.Rogue.Services;

/// <summary>Violation: a public service in a module implementation (only Contracts types cross the boundary).</summary>
public sealed class RogueService(string label)
{
    public string Describe() => label;
}
