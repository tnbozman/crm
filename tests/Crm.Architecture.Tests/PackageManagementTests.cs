using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace Crm.Architecture.Tests;

[Trait("Category", "Architecture")]
public sealed class PackageManagementTests
{
    [Fact]
    public void DirectoryPackagesProps_Loaded_EnablesCentralPackageManagement()
    {
        var path = RepositoryRoot.File("Directory.Packages.props");
        File.Exists(path).ShouldBeTrue("Directory.Packages.props is missing");

        var value = XDocument.Load(path).Descendants("ManagePackageVersionsCentrally").Select(e => e.Value.Trim()).LastOrDefault();

        value.ShouldBe("true");
    }

    [Fact]
    public void ProjectFiles_PackageReferences_DeclareNoVersion()
    {
        var offenders = Directory
            .EnumerateFiles(RepositoryRoot.Path, "*.csproj", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(p => XDocument.Load(p)
                .Descendants("PackageReference")
                .Where(r => r.Attribute("Version") is not null || r.Element("Version") is not null)
                .Select(r => $"{Path.GetRelativePath(RepositoryRoot.Path, p)}: {r.Attribute("Include")?.Value}"))
            .ToList();

        offenders.ShouldBeEmpty("package versions belong in Directory.Packages.props");
    }
}
