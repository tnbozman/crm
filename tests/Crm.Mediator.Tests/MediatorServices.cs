using System.Security.Claims;
using Crm.Mediator;
using Crm.SharedKernel.Security;
using Crm.Testing.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Mediator.Tests;

/// <summary>Services for mediator tests: the mediator, ICurrentUser, CRM authorisation and a per-scope test user.</summary>
internal static class MediatorServices
{
    /// <summary>Each scope starts signed in as a fresh user holding <paramref name="roles"/>.</summary>
    public static ServiceCollection Create(params string[] roles)
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddCurrentUser();
        services.AddCrmAuthorization();
        services.AddScoped(_ => new TestUserContext { Principal = SignedIn(Guid.NewGuid(), roles) });
        services.AddScoped<IUserContext>(sp => sp.GetRequiredService<TestUserContext>());
        return services;
    }

    public static ClaimsPrincipal SignedIn(Guid userId, params string[] roles) =>
        new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString()), .. roles.Select(r => new Claim(ClaimTypes.Role, r))],
            authenticationType: "Test"));

    public static ClaimsPrincipal Anonymous() => new(new ClaimsIdentity());
}
