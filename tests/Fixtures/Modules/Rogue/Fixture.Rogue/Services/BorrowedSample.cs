using Fixture.Sample;

namespace Fixture.Rogue.Services;

/// <summary>Violation: uses a type from another module's implementation.</summary>
internal static class BorrowedSample
{
    public static Type Borrowed => typeof(SampleModule);
}
