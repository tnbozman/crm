using Crm.Host;
using Shouldly;
using Xunit;

namespace Crm.Architecture.Tests;

[Trait("Category", "Architecture")]
public sealed class ModuleCompositionTests
{
    [Fact]
    public void Host_ModulesList_MatchesModuleProjects()
    {
        var modulesDirectory = RepositoryRoot.File("src/Modules");
        var moduleFolders = Directory.Exists(modulesDirectory)
            ? Directory.GetDirectories(modulesDirectory).Select(Path.GetFileName).ToList()
            : [];

        HostModules.All.Select(m => m.Name).ShouldBe(moduleFolders, ignoreOrder: true,
            customMessage: "every folder under src/Modules is listed in Crm.Host HostModules.All, and nothing else is");
    }
}
