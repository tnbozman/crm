namespace Crm.Architecture.Tests;

internal static class RepositoryRoot
{
    public static string Path { get; } = Find();

    public static string File(string relativePath) => System.IO.Path.Combine(Path, relativePath);

    private static string Find()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !System.IO.File.Exists(System.IO.Path.Combine(directory.FullName, "Crm.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Crm.slnx not found above " + AppContext.BaseDirectory);
    }
}
