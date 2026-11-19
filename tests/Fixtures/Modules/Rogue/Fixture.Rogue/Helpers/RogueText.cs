namespace Fixture.Rogue.Helpers;

/// <summary>Violation: a folder outside the module layout (docs/architecture.md §1).</summary>
internal static class RogueText
{
    public static string Shout(string text) => text.ToUpperInvariant();
}
