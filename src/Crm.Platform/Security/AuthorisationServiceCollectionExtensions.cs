using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Platform.Security;

public static class AuthorisationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the CRM's authorisation baseline: a fallback policy that requires a signed-in user on every endpoint
    /// (deny by default, ADR-011). Returns the builder so the host and modules can add named policies.
    /// </summary>
    public static AuthorizationBuilder AddCrmAuthorization(this IServiceCollection services) =>
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
}
