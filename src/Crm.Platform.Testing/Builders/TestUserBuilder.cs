using Bogus;
using Crm.Platform.Testing.Auth;

namespace Crm.Platform.Testing.Builders;

/// <summary>Builds synthetic users (Bogus): never real people, never classified or CUI data.</summary>
public sealed class TestUserBuilder
{
    private readonly Faker faker = new("en_AU");
    private readonly List<string> roles = [];

    public TestUserBuilder WithRoles(params string[] names)
    {
        roles.AddRange(names);
        return this;
    }

    public TestUser Build() =>
        new($"test-user-{faker.Random.Guid():N}", faker.Name.FullName(), faker.Internet.ExampleEmail(), [.. roles]);
}
