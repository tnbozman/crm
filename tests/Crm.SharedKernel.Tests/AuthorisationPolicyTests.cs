using System.Security.Claims;
using Crm.SharedKernel.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.SharedKernel.Tests;

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

    [Fact]
    public async Task RequireRole_Policy_ForbidsOtherRoles()
    {
        using var provider = BuildProvider();
        var adminPolicy = await provider.GetRequiredService<IAuthorizationPolicyProvider>().GetPolicyAsync(Policies.Admin);
        var authorisation = provider.GetRequiredService<IAuthorizationService>();

        adminPolicy.ShouldNotBeNull();
        (await authorisation.AuthorizeAsync(SignedIn(Roles.Sales, Roles.Finance), adminPolicy)).Succeeded.ShouldBeFalse();
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Sales)]
    [InlineData(Roles.Delivery)]
    [InlineData(Roles.Finance)]
    [InlineData(Roles.Auditor)]
    [InlineData(Roles.ReadOnly)]
    public async Task RequireRole_Policy_AllowsThatRoleOnly(string role)
    {
        using var provider = BuildProvider();
        var policy = await provider.GetRequiredService<IAuthorizationPolicyProvider>().GetPolicyAsync(Policies.ForRole(role));
        var authorisation = provider.GetRequiredService<IAuthorizationService>();
        var everyOtherRole = Roles.All.Where(r => r != role).ToArray();

        policy.ShouldNotBeNull();
        (await authorisation.AuthorizeAsync(SignedIn(role), policy)).Succeeded.ShouldBeTrue();
        (await authorisation.AuthorizeAsync(SignedIn(everyOtherRole), policy)).Succeeded.ShouldBeFalse();
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
