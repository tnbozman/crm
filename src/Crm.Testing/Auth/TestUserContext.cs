using System.Security.Claims;
using Crm.SharedKernel.Security;

namespace Crm.Testing.Auth;

/// <summary>A user context a test sets per scope.</summary>
public sealed class TestUserContext : IUserContext
{
    public ClaimsPrincipal Principal { get; set; } = new(new ClaimsIdentity());
}
