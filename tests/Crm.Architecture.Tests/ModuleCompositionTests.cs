using System.Xml.Linq;
using Crm.Host;
using Crm.Migrator;
using Shouldly;
using Xunit;

namespace Crm.Architecture.Tests;

/// <summary>
/// ADR-013: the host is the composition root; it lists and references every module library.
/// The Migrator does the same, so it applies every module's migrations (S1-4).
/// </summary>
[Trait("Category", "Architecture")]
public sealed class ModuleCompositionTests
{
    [Fact]
    public void Host_ModulesList_MatchesModuleProjects() =>
        HostModules.All.Select(m => m.Name).ShouldBe(ModuleNamesFromProjects(), ignoreOrder: true,
            customMessage: "every src/Modules/Crm.<Module> project is listed in Crm.Host HostModules.All, and nothing else is");

    [Fact]
    public void Host_References_EveryModuleProject() =>
        ModuleProjectsNotReferencedBy("src/Crm.Host/Crm.Host.csproj").ShouldBeEmpty("Crm.Host references every module library");

    [Fact]
    public void Migrator_ModulesList_MatchesModuleProjects() =>
        MigratorModules.All.Select(m => m.Name).ShouldBe(ModuleNamesFromProjects(), ignoreOrder: true,
            customMessage: "every src/Modules/Crm.<Module> project is listed in Crm.Migrator MigratorModules.All, and nothing else is");

    [Fact]
    public void Migrator_References_EveryModuleProject() =>
        ModuleProjectsNotReferencedBy("src/Crm.Migrator/Crm.Migrator.csproj").ShouldBeEmpty("Crm.Migrator references every module library");

    private static List<string> ModuleProjectsNotReferencedBy(string projectFile)
    {
        var references = XDocument.Load(RepositoryRoot.File(projectFile))
            .Descendants("ProjectReference")
            .Select(r => Path.GetFileNameWithoutExtension(((string)r.Attribute("Include")!).Replace('\\', '/')))
            .ToHashSet(StringComparer.Ordinal);

        return [.. ShippedModules.Set.ProjectFiles.Select(p => Path.GetFileNameWithoutExtension(p)).Where(module => !references.Contains(module))];
    }

    /// <summary>"src/Modules/Crm.Directory/Crm.Directory.csproj" names the module "Directory".</summary>
    private static List<string> ModuleNamesFromProjects() =>
        [.. ShippedModules.Set.ProjectFiles.Select(p => Path.GetFileNameWithoutExtension(p)!["Crm.".Length..])];
}
