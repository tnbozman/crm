using System.Net.Http.Json;
using Crm.Platform.Testing.Builders;
using Shouldly;
using Xunit;

namespace Crm.Host.Tests;

public sealed class FakeAuthTests(TestHostFactory factory) : IClassFixture<TestHostFactory>
{
    [Fact]
    public async Task FakeAuth_UserWithRoles_IsReadByCurrentUser()
    {
        var user = new TestUserBuilder().WithRoles("Sales", "Finance").Build();
        using var client = factory.CreateClientFor(user);

        var me = await client.GetFromJsonAsync<CurrentUserBody>(new Uri(TestOnlyController.MePath, UriKind.Relative), TestContext.Current.CancellationToken);

        me.ShouldNotBeNull();
        me.Id.ShouldBe(user.Id);
        me.Roles.ShouldBe(["Finance", "Sales"]);
    }

    private sealed record CurrentUserBody(string Id, string[] Roles);
}
