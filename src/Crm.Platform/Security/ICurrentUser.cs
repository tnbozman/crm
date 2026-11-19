namespace Crm.Platform.Security;

/// <summary>The signed-in user, for services and handlers. Never read HttpContext for this.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid UserId { get; }

    IReadOnlySet<string> Roles { get; }

    IReadOnlySet<Guid> TeamIds { get; }

    IReadOnlySet<Guid> BusinessUnitIds { get; }

    bool IsInRole(string role);
}
