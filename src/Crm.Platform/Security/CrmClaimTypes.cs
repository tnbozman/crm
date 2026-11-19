namespace Crm.Platform.Security;

/// <summary>Claims the CRM adds to the signed-in principal. Teams and business units come from the Access module, not Entra.</summary>
public static class CrmClaimTypes
{
    public const string Team = "crm:team";
    public const string BusinessUnit = "crm:business_unit";
}
