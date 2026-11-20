namespace Crm.SharedKernel.Modules;

/// <summary>The one database every module shares, each in its own schema (ADR-013).</summary>
public static class ModuleDatabase
{
    /// <summary>ConnectionStrings:Crm. Managed identity in Azure, so it holds no password there.</summary>
    public const string ConnectionStringName = "Crm";
}
