using System.Security.Claims;
using Crm.Platform.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Platform.Tests;

public sealed class AuthorisationPolicyTests
{
    [Fact]
    public async Task FallbackPolicy_RequiresAuthenticatedUser()
    {
        using var provider = BuildProvider();
        var fallback = await provider.GetRequiredService<IAuthorizationPolicyProvider>().GetFallbackPolicyAsync();
        var authorisation = provider.GetRequiredService<IAuthorizationService>();

        fallback.ShouldNotBeNull("every endpoint is denied to anonymous callers by default");
        (await authorisation.AuthorizeAsync(Anonymous(), fallback)).Succeeded.ShouldBeFalse();
        (await authorisation.AuthorizeAsync(SignedIn(), fallback)).Succeeded.ShouldBeTrue();
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddCrmAuthorization();
        return services.BuildServiceProvider();
    }

    private static ClaimsPrincipal Anonymous() => new(new ClaimsIdentity());

    private static ClaimsPrincipal SignedIn(params string[] roles) =>
        new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()), .. roles.Select(r => new Claim(ClaimTypes.Role, r))],
            authenticationType: "Test"));
}
