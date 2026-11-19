namespace Crm.Testing.Auth;

/// <summary>A synthetic signed-in user for API tests. Build with <see cref="Builders.TestUserBuilder"/>.</summary>
public sealed record TestUser(
    Guid Id,
    string Name,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<Guid> TeamIds,
    IReadOnlyList<Guid> BusinessUnitIds);
