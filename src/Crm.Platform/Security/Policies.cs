namespace Crm.Platform.Security;

/// <summary>Named authorisation policies for <c>[Authorize(Policy = ...)]</c>. Registered by AddCrmAuthorization.</summary>
public static class Policies
{
    /// <summary>One policy per application role: the caller must hold that role.</summary>
    public const string Admin = "Role:" + Roles.Admin;
    public const string Sales = "Role:" + Roles.Sales;
    public const string Delivery = "Role:" + Roles.Delivery;
    public const string Finance = "Role:" + Roles.Finance;
    public const string Auditor = "Role:" + Roles.Auditor;
    public const string ReadOnly = "Role:" + Roles.ReadOnly;

    public static string ForRole(string role) => "Role:" + role;
}
