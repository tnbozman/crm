using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace Crm.Architecture.Tests;

[Trait("Category", "Architecture")]
public sealed class SolutionLayoutTests
{
    [Fact]
    public void SolutionFile_Loaded_ContainsSkeletonProjects()
    {
        var solution = XDocument.Load(RepositoryRoot.File("Crm.slnx"));

        var projectNames = solution
            .Descendants("Project")
            .Select(p => Path.GetFileNameWithoutExtension((string)p.Attribute("Path")!))
            .ToList();

        string[] required = ["Crm.Host", "Crm.SharedKernel", "Crm.Mediator", "Crm.Testing", "Crm.Migrator"];
        required.ShouldBeSubsetOf(projectNames);
    }
}
