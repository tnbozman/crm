namespace Crm.Testing;

/// <summary>Values for <c>[Trait("Category", …)]</c>. make test-unit excludes Integration and Architecture.</summary>
public static class TestCategories
{
    /// <summary>Needs SQL Server (make test-int).</summary>
    public const string Integration = nameof(Integration);

    /// <summary>Architecture rules (make test-arch).</summary>
    public const string Architecture = nameof(Architecture);

    /// <summary>Calls real AI providers; skipped without keys (make ai-smoke).</summary>
    public const string AiSmoke = nameof(AiSmoke);
}
