using System.Security.Claims;

namespace Crm.Platform.Security;

/// <summary>An <see cref="ICurrentUser"/> read once from a claims principal.</summary>
public sealed class CurrentUser : ICurrentUser
{
    private CurrentUser(Guid userId, IEnumerable<string> roles, IEnumerable<Guid> teamIds, IEnumerable<Guid> businessUnitIds)
    {
        UserId = userId;
        Roles = roles.ToHashSet(StringComparer.Ordinal);
        TeamIds = teamIds.ToHashSet();
        BusinessUnitIds = businessUnitIds.ToHashSet();
    }

    public bool IsAuthenticated => true;

    public Guid UserId { get; }

    public IReadOnlySet<string> Roles { get; }

    public IReadOnlySet<Guid> TeamIds { get; }

    public IReadOnlySet<Guid> BusinessUnitIds { get; }

    public bool IsInRole(string role) => Roles.Contains(role);

    public static CurrentUser FromPrincipal(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return new CurrentUser(
            Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!),
            principal.FindAll(ClaimTypes.Role).Select(c => c.Value),
            Guids(principal, CrmClaimTypes.Team),
            Guids(principal, CrmClaimTypes.BusinessUnit));
    }

    private static IEnumerable<Guid> Guids(ClaimsPrincipal principal, string claimType) =>
        principal.FindAll(claimType).Select(c => Guid.Parse(c.Value));
}
