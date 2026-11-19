using System.Reflection;
using Crm.Platform.Errors;
using Crm.Platform.Security;
using Microsoft.AspNetCore.Authorization;

namespace Crm.Platform.Messaging;

/// <summary>
/// Pipeline step 4: authorise the request as the current user. The request's <see cref="RequiresPolicyAttribute"/>
/// names the policy; without one the caller must still be signed in (fail closed).
/// </summary>
internal sealed class AuthorisationBehaviour(IUserContext userContext, IAuthorizationService authorisation) : IRequestBehaviour
{
    private static readonly AuthorizationPolicy SignedIn = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

    public async Task<TResponse> Handle<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> proceed, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(proceed);

        var principal = userContext.Principal;
        var result = typeof(TRequest).GetCustomAttribute<RequiresPolicyAttribute>() is { } required
            ? await authorisation.AuthorizeAsync(principal, request, required.Policy).ConfigureAwait(false)
            : await authorisation.AuthorizeAsync(principal, request, SignedIn).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            throw new ForbiddenException($"The current user may not send {typeof(TRequest).Name}.");
        }

        return await proceed().ConfigureAwait(false);
    }
}
