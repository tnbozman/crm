using System.Security.Claims;

namespace Crm.SharedKernel.Security;

/// <summary>
/// Supplies the principal that <see cref="ICurrentUser"/> is read from. In a request it is the signed-in user;
/// work outside a request (the outbox relay, S1-5) registers its own.
/// </summary>
public interface IUserContext
{
    ClaimsPrincipal Principal { get; }
}
