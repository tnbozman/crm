using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.SharedKernel.Security;

public static class AuthorisationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the CRM's authorisation baseline: a fallback policy that requires a signed-in user on every endpoint
    /// (deny by default, ADR-011) and one policy per application role (<see cref="Policies"/>).
    /// Returns the builder so the host and modules can add further named policies.
    /// </summary>
    public static AuthorizationBuilder AddCrmAuthorization(this IServiceCollection services)
    {
        var builder = services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        foreach (var role in Roles.All)
        {
            builder.AddRolePolicy(Policies.ForRole(role), role);
        }

        return builder;
    }

    /// <summary>Adds a policy that requires a signed-in user holding any of <paramref name="roles"/>.</summary>
    public static AuthorizationBuilder AddRolePolicy(this AuthorizationBuilder builder, string name, params string[] roles)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddPolicy(name, policy => policy.RequireAuthenticatedUser().RequireRole(roles));
    }
}
