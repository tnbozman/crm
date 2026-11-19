using System.Xml.Linq;
using Crm.Host;
using Shouldly;
using Xunit;

namespace Crm.Architecture.Tests;

/// <summary>ADR-013: the host is the composition root; it lists and references every module library.</summary>
[Trait("Category", "Architecture")]
public sealed class ModuleCompositionTests
{
    [Fact]
    public void Host_ModulesList_MatchesModuleProjects() =>
        HostModules.All.Select(m => m.Name).ShouldBe(ModuleNamesFromProjects(), ignoreOrder: true,
            customMessage: "every src/Modules/Crm.<Module> project is listed in Crm.Host HostModules.All, and nothing else is");

    [Fact]
    public void Host_References_EveryModuleProject()
    {
        var hostReferences = XDocument.Load(RepositoryRoot.File("src/Crm.Host/Crm.Host.csproj"))
            .Descendants("ProjectReference")
            .Select(r => Path.GetFileNameWithoutExtension(((string)r.Attribute("Include")!).Replace('\\', '/')))
            .ToHashSet(StringComparer.Ordinal);

        ShippedModules.Set.ProjectFiles.Select(Path.GetFileNameWithoutExtension)
            .Where(module => !hostReferences.Contains(module!))
            .ShouldBeEmpty("Crm.Host references every module library");
    }

    /// <summary>"src/Modules/Crm.Directory/Crm.Directory.csproj" names the module "Directory".</summary>
    private static List<string> ModuleNamesFromProjects() =>
        [.. ShippedModules.Set.ProjectFiles.Select(p => Path.GetFileNameWithoutExtension(p)!["Crm.".Length..])];
}
