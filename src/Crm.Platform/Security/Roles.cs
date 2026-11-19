namespace Crm.Platform.Security;

/// <summary>Application roles. Values match the Entra app role values (claim "roles"). Refined in R1.</summary>
public static class Roles
{
    public const string Admin = nameof(Admin);
    public const string Sales = nameof(Sales);
    public const string Delivery = nameof(Delivery);
    public const string Finance = nameof(Finance);
    public const string Auditor = nameof(Auditor);
    public const string ReadOnly = nameof(ReadOnly);

    public static IReadOnlyList<string> All { get; } = [Admin, Sales, Delivery, Finance, Auditor, ReadOnly];
}
