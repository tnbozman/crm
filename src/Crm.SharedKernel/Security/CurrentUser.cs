using System.Security.Claims;

namespace Crm.SharedKernel.Security;

/// <summary>An <see cref="ICurrentUser"/> read once from a claims principal.</summary>
public sealed class CurrentUser : ICurrentUser
{
    private CurrentUser(bool isAuthenticated, Guid userId, IEnumerable<string> roles, IEnumerable<Guid> teamIds, IEnumerable<Guid> businessUnitIds)
    {
        IsAuthenticated = isAuthenticated;
        UserId = userId;
        Roles = roles.ToHashSet(StringComparer.Ordinal);
        TeamIds = teamIds.ToHashSet();
        BusinessUnitIds = businessUnitIds.ToHashSet();
    }

    /// <summary>Nobody signed in: no ID, no roles, no teams.</summary>
    public static CurrentUser Anonymous { get; } = new(isAuthenticated: false, Guid.Empty, [], [], []);

    public bool IsAuthenticated { get; }

    public Guid UserId { get; }

    public IReadOnlySet<string> Roles { get; }

    public IReadOnlySet<Guid> TeamIds { get; }

    public IReadOnlySet<Guid> BusinessUnitIds { get; }

    public bool IsInRole(string role) => Roles.Contains(role);

    public static CurrentUser FromPrincipal(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.Identity?.IsAuthenticated != true)
        {
            return Anonymous;
        }

        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            throw new InvalidOperationException(
                $"The signed-in principal has no GUID user ID in claim '{ClaimTypes.NameIdentifier}'. Sign-in must set it.");
        }

        return new CurrentUser(
            isAuthenticated: true,
            userId,
            principal.FindAll(ClaimTypes.Role).Select(c => c.Value),
            Guids(principal, CrmClaimTypes.Team),
            Guids(principal, CrmClaimTypes.BusinessUnit));
    }

    private static IEnumerable<Guid> Guids(ClaimsPrincipal principal, string claimType) =>
        principal.FindAll(claimType).Select(c => Guid.Parse(c.Value));
}
