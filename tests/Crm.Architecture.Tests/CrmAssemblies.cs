using System.Reflection;

namespace Crm.Architecture.Tests;

/// <summary>Every production Crm.* assembly copied next to the tests (test projects and Crm.Testing excluded).</summary>
internal static class CrmAssemblies
{
    public static IReadOnlyList<Assembly> All { get; } = Load();

    private static List<Assembly> Load() =>
        Directory.EnumerateFiles(AppContext.BaseDirectory, "Crm.*.dll")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => name is not null
                && !name.EndsWith(".Tests", StringComparison.Ordinal)
                && name != "Crm.Testing")
            .Select(name => Assembly.Load(name!))
            .OrderBy(a => a.GetName().Name, StringComparer.Ordinal)
            .ToList();
}
