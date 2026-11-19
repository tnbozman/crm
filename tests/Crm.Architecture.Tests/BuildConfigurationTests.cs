using System.Xml.Linq;
using Shouldly;
using Xunit;

namespace Crm.Architecture.Tests;

[Trait("Category", "Architecture")]
public sealed class BuildConfigurationTests
{
    [Theory]
    [InlineData("Nullable", "enable")]
    [InlineData("TreatWarningsAsErrors", "true")]
    [InlineData("AnalysisMode", "Recommended")]
    [InlineData("EnforceCodeStyleInBuild", "true")]
    [InlineData("Deterministic", "true")]
    public void DirectoryBuildProps_QualityGate_IsSet(string property, string expected)
    {
        var path = RepositoryRoot.File("Directory.Build.props");
        File.Exists(path).ShouldBeTrue("Directory.Build.props is missing");

        var value = XDocument.Load(path).Descendants(property).Select(e => e.Value.Trim()).LastOrDefault();

        value.ShouldBe(expected, $"{property} in Directory.Build.props");
    }
}
