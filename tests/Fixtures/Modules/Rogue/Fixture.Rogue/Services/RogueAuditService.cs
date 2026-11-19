using Microsoft.AspNetCore.Http;

namespace Fixture.Rogue.Services;

/// <summary>Violation: a service reading HttpContext (services get the user from ICurrentUser, never HTTP).</summary>
internal sealed class RogueAuditService(IHttpContextAccessor accessor)
{
    public string? Path() => accessor.HttpContext?.Request.Path;
}
