using System.Net.Http.Json;
using Crm.SharedKernel.Security;
using Crm.Testing.Builders;
using Shouldly;
using Xunit;

namespace Crm.Host.Tests;

public sealed class FakeAuthTests(TestHostFactory factory) : IClassFixture<TestHostFactory>
{
    [Fact]
    public async Task FakeAuth_UserWithRolesTeamsAndBusinessUnits_IsReadByCurrentUser()
    {
        var team = Guid.NewGuid();
        var unit = Guid.NewGuid();
        var user = new TestUserBuilder().WithRoles(Roles.Sales, Roles.Finance).WithTeams(team).WithBusinessUnits(unit).Build();
        using var client = factory.CreateClientFor(user);

        var me = await client.GetFromJsonAsync<CurrentUserBody>(new Uri(TestOnlyController.MePath, UriKind.Relative), TestContext.Current.CancellationToken);

        me.ShouldNotBeNull();
        me.Id.ShouldBe(user.Id);
        me.Roles.ShouldBe([Roles.Finance, Roles.Sales]);
        me.TeamIds.ShouldBe([team]);
        me.BusinessUnitIds.ShouldBe([unit]);
    }

    private sealed record CurrentUserBody(Guid Id, string[] Roles, Guid[] TeamIds, Guid[] BusinessUnitIds);
}
