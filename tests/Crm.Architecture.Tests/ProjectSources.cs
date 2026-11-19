using System.Reflection;

namespace Crm.Architecture.Tests;

/// <summary>Finds the project folder an assembly was built from (src/ or tests/Fixtures/), for source-level rules.</summary>
internal static class ProjectSources
{
    private static readonly string[] Roots = ["src", "tests/Fixtures"];

    public static string DirectoryOf(Assembly assembly)
    {
        var projectFile = assembly.GetName().Name + ".csproj";
        return Roots
            .Select(RepositoryRoot.File)
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, projectFile, SearchOption.AllDirectories))
            .Select(Path.GetDirectoryName)
            .Single()!;
    }

    public static string Relative(string path) => Path.GetRelativePath(RepositoryRoot.Path, path).Replace('\\', '/');
}
