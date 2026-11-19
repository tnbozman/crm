using System.Text.RegularExpressions;
using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace Crm.Architecture.Tests;

[Trait("Category", "Architecture")]
public sealed partial class PersistenceBoundaryTests
{
    [Fact]
    public void Projects_ReferencingEntityFrameworkCore_AreModulesOrMigrator()
    {
        var src = RepositoryRoot.File("src");
        var offenders = Directory.EnumerateFiles(src, "*.csproj", SearchOption.AllDirectories)
            .Where(p => XDocument.Load(p).Descendants("PackageReference")
                .Any(r => r.Attribute("Include")?.Value.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true))
            .Select(p => Path.GetRelativePath(RepositoryRoot.Path, p).Replace('\\', '/'))
            .Where(p => !p.StartsWith("src/Modules/", StringComparison.Ordinal) && p != "src/Crm.Migrator/Crm.Migrator.csproj")
            .ToList();

        offenders.ShouldBeEmpty("only module projects and Crm.Migrator use EF Core");
    }

    [Fact]
    public void Host_DoesNotCall_Migrate()
    {
        var host = RepositoryRoot.File("src/Crm.Host");
        var offenders = Directory.EnumerateFiles(host, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(p => MigrateCall().IsMatch(File.ReadAllText(p)))
            .Select(p => Path.GetRelativePath(RepositoryRoot.Path, p))
            .ToList();

        offenders.ShouldBeEmpty("the runtime identity never runs migrations; Crm.Migrator does");
    }

    [GeneratedRegex(@"\.Migrate(Async)?\s*\(")]
    private static partial Regex MigrateCall();
}
