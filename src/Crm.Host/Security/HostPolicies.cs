namespace Crm.Host.Security;

/// <summary>Named authorisation policies. Every endpoint requires an authenticated user unless it names one of these.</summary>
internal static class HostPolicies
{
    /// <summary>Open to unauthenticated callers. Only <c>/api/v1/health</c> uses it.</summary>
    public const string Anonymous = nameof(Anonymous);
}
