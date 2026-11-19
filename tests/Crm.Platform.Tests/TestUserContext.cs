using System.Security.Claims;
using Crm.Platform.Security;

namespace Crm.Platform.Tests;

/// <summary>A user context a test sets per scope.</summary>
internal sealed class TestUserContext : IUserContext
{
    public ClaimsPrincipal Principal { get; set; } = new(new ClaimsIdentity());
}
