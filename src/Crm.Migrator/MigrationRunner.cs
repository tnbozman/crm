using Crm.SharedKernel.Modules;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Migrator;

/// <summary>Applies each module's migrations, through the DbContexts the module registers in IModule.AddServices.</summary>
public static class MigrationRunner
{
    /// <summary>The only environment where <c>reset</c> may drop the database (the devcontainer's).</summary>
    public const string ResetEnvironment = "Development";

    /// <summary>Migrates every module's DbContexts and returns the names of the modules migrated, in order.</summary>
    public static async Task<IReadOnlyList<string>> RunAsync(
        IReadOnlyList<IModule> modules, IConfiguration configuration, string environment, bool reset, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(modules);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(ModuleDatabase.ConnectionStringName)
            ?? throw new InvalidOperationException($"ConnectionStrings:{ModuleDatabase.ConnectionStringName} is not configured.");

        if (reset)
        {
            if (!string.Equals(environment, ResetEnvironment, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Reset drops the database and runs only in {ResetEnvironment}, not in {environment}.");
            }

            await RecreateDatabaseAsync(connectionString, cancellationToken).ConfigureAwait(false);
        }

        var migrated = new List<string>();
        foreach (var module in modules)
        {
            if (await MigrateAsync(module, configuration, cancellationToken).ConfigureAwait(false))
            {
                migrated.Add(module.Name);
            }
        }

        return migrated;
    }

    private static async Task<bool> MigrateAsync(IModule module, IConfiguration configuration, CancellationToken cancellationToken)
    {
        var services = new ServiceCollection();
        module.AddServices(services, configuration);
        var contextTypes = services.Select(d => d.ServiceType).Where(t => t.IsSubclassOf(typeof(DbContext))).Distinct().ToList();

        await using var provider = services.BuildServiceProvider();
        foreach (var contextType in contextTypes)
        {
            await using var scope = provider.CreateAsyncScope();
            var context = (DbContext)scope.ServiceProvider.GetRequiredService(contextType);
            await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }

        return contextTypes.Count > 0;
    }

    private static async Task RecreateDatabaseAsync(string connectionString, CancellationToken cancellationToken)
    {
        var target = new SqlConnectionStringBuilder(connectionString);
        var database = target.InitialCatalog is { Length: > 0 } name
            ? name
            : throw new InvalidOperationException("Reset needs a database name (Initial Catalog) in the connection string.");

        target.InitialCatalog = "master";
        await using (var connection = new SqlConnection(target.ConnectionString))
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await using var command = new SqlCommand(
                """
                DECLARE @quoted nvarchar(258) = QUOTENAME(@database);
                IF DB_ID(@database) IS NOT NULL
                    EXEC (N'ALTER DATABASE ' + @quoted + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ' + @quoted + N';');
                EXEC (N'CREATE DATABASE ' + @quoted + N';');
                """,
                connection);
            command.Parameters.AddWithValue("@database", database);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        // Pooled connections to the dropped database are dead.
        SqlConnection.ClearAllPools();
    }
}
