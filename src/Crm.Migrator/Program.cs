using Microsoft.Extensions.Configuration;

namespace Crm.Migrator;

/// <summary>
/// Applies every module's migrations. Runs with the deployment identity; the host's runtime identity never migrates.
/// Connection string: ConnectionStrings__Crm (environment variable) or --ConnectionStrings:Crm=...
/// --reset drops and recreates the database first, and is refused outside DOTNET_ENVIRONMENT=Development (make db-reset).
/// An explicit internal class, not top-level statements, so test projects that reference both the Migrator and the host
/// see only the host's Program.
/// </summary>
internal static class Program
{
    private const string ResetFlag = "--reset";

    public static async Task<int> Main(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        var reset = args.Contains(ResetFlag, StringComparer.Ordinal);
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddCommandLine([.. args.Where(a => a != ResetFlag)])
            .Build();

        var migrated = await MigrationRunner.RunAsync(MigratorModules.All, configuration, environment, reset, CancellationToken.None)
            .ConfigureAwait(false);
        Console.WriteLine(migrated.Count == 0 ? "migrator: no module migrations to apply" : $"migrator: migrated {string.Join(", ", migrated)}");
        return 0;
    }
}
