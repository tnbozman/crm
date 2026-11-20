using Crm.Migrator;
using Microsoft.Extensions.Configuration;

// Applies every module's migrations. Runs with the deployment identity; the host's runtime identity never migrates.
// Connection string: ConnectionStrings__Crm (environment variable) or --ConnectionStrings:Crm=...
// --reset drops and recreates the database first, and is refused outside DOTNET_ENVIRONMENT=Development (make db-reset).
const string ResetFlag = "--reset";

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
var reset = args.Contains(ResetFlag, StringComparer.Ordinal);
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddCommandLine([.. args.Where(a => a != ResetFlag)])
    .Build();

var migrated = await MigrationRunner.RunAsync(MigratorModules.All, configuration, environment, reset, CancellationToken.None);
Console.WriteLine(migrated.Count == 0 ? "migrator: no module migrations to apply" : $"migrator: migrated {string.Join(", ", migrated)}");
return 0;
