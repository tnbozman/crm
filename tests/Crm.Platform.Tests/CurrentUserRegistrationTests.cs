using System.Security.Claims;
using Crm.Platform.Security;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Platform.Tests;

public sealed class CurrentUserRegistrationTests
{
    [Fact]
    public void AddCurrentUser_InScope_ReadsTheUserContextsPrincipal()
    {
        var userId = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Role, Roles.Auditor)], authenticationType: "Test"));
        var services = new ServiceCollection();
        services.AddCurrentUser();
        services.AddScoped<IUserContext>(_ => new FixedUserContext(principal));

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var user = scope.ServiceProvider.GetRequiredService<ICurrentUser>();

        user.UserId.ShouldBe(userId);
        user.IsInRole(Roles.Auditor).ShouldBeTrue();
    }

    private sealed class FixedUserContext(ClaimsPrincipal principal) : IUserContext
    {
        public ClaimsPrincipal Principal => principal;
    }
}
