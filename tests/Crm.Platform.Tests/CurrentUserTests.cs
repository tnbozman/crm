using System.Security.Claims;
using Crm.Platform.Security;
using Shouldly;
using Xunit;

namespace Crm.Platform.Tests;

public sealed class CurrentUserTests
{
    private static readonly Guid UserId = Guid.Parse("6f1b7c1e-2a4d-4c55-9a51-0d3e8f2b7a10");
    private static readonly Guid TeamA = Guid.Parse("a0000000-0000-0000-0000-00000000000a");
    private static readonly Guid TeamB = Guid.Parse("b0000000-0000-0000-0000-00000000000b");
    private static readonly Guid UnitX = Guid.Parse("c0000000-0000-0000-0000-00000000000c");

    [Fact]
    public void CurrentUser_FromClaims_ExposesIdRolesTeamsAndBusinessUnits()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, UserId.ToString()),
                new Claim(ClaimTypes.Role, Roles.Sales),
                new Claim(ClaimTypes.Role, Roles.Finance),
                new Claim(CrmClaimTypes.Team, TeamA.ToString()),
                new Claim(CrmClaimTypes.Team, TeamB.ToString()),
                new Claim(CrmClaimTypes.BusinessUnit, UnitX.ToString()),
            ],
            authenticationType: "Test"));

        var user = CurrentUser.FromPrincipal(principal);

        user.IsAuthenticated.ShouldBeTrue();
        user.UserId.ShouldBe(UserId);
        user.Roles.ShouldBe([Roles.Sales, Roles.Finance], ignoreOrder: true);
        user.TeamIds.ShouldBe([TeamA, TeamB], ignoreOrder: true);
        user.BusinessUnitIds.ShouldBe([UnitX]);
        user.IsInRole(Roles.Finance).ShouldBeTrue();
        user.IsInRole(Roles.Admin).ShouldBeFalse();
    }

    [Fact]
    public void CurrentUser_FromUnauthenticatedPrincipal_IsAnonymous()
    {
        var user = CurrentUser.FromPrincipal(new ClaimsPrincipal(new ClaimsIdentity()));

        user.IsAuthenticated.ShouldBeFalse();
        user.UserId.ShouldBe(Guid.Empty);
        user.Roles.ShouldBeEmpty();
        user.TeamIds.ShouldBeEmpty();
        user.BusinessUnitIds.ShouldBeEmpty();
    }
}
