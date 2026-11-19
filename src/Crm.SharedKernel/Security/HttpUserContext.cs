using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Crm.SharedKernel.Security;

/// <summary>The request's user, or an anonymous principal outside a request.</summary>
internal sealed class HttpUserContext(IHttpContextAccessor accessor) : IUserContext
{
    public ClaimsPrincipal Principal => accessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
}
