using Bogus;
using Crm.Platform.Testing.Auth;

namespace Crm.Platform.Testing.Builders;

/// <summary>Builds synthetic users (Bogus): never real people, never classified or CUI data.</summary>
public sealed class TestUserBuilder
{
    private readonly Faker faker = new("en_AU");
    private readonly List<string> roles = [];
    private readonly List<Guid> teamIds = [];
    private readonly List<Guid> businessUnitIds = [];

    public TestUserBuilder WithRoles(params string[] names)
    {
        roles.AddRange(names);
        return this;
    }

    public TestUserBuilder WithTeams(params Guid[] ids)
    {
        teamIds.AddRange(ids);
        return this;
    }

    public TestUserBuilder WithBusinessUnits(params Guid[] ids)
    {
        businessUnitIds.AddRange(ids);
        return this;
    }

    public TestUser Build() =>
        new(faker.Random.Guid(), faker.Name.FullName(), faker.Internet.ExampleEmail(), [.. roles], [.. teamIds], [.. businessUnitIds]);
}
