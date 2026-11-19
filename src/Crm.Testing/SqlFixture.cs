using Microsoft.Data.SqlClient;
using Respawn;
using Respawn.Graph;
using Xunit;

namespace Crm.Testing;

/// <summary>
/// A uniquely named database on the test SQL Server, created on initialise and dropped on dispose.
/// Use one per test class (<c>IClassFixture&lt;SqlFixture&gt;</c>) so classes can run in parallel.
/// The server comes from <c>CRM_SQL_SERVER</c>, credentials from <c>CRM_SQL_USER</c> (default <c>sa</c>)
/// and <c>CRM_SQL_PASSWORD</c>: set by the devcontainer and CI, never written in code.
/// </summary>
public sealed class SqlFixture : IAsyncLifetime
{
    public string DatabaseName { get; } = $"crm_test_{Guid.NewGuid():N}";

    public string ConnectionString => Build(DatabaseName);

    public static string MasterConnectionString() => Build("master");

    public async ValueTask InitializeAsync() =>
        await ExecuteOnMasterAsync($"CREATE DATABASE [{DatabaseName}];").ConfigureAwait(false);

    public async ValueTask DisposeAsync()
    {
        await using (var connection = new SqlConnection(ConnectionString))
        {
            SqlConnection.ClearPool(connection);
        }

        await ExecuteOnMasterAsync(
            $"IF DB_ID(N'{DatabaseName}') IS NOT NULL BEGIN ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{DatabaseName}]; END;")
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes every row in the fixture database (Respawn), keeping the schema and the migrations history.
    /// Call it at the start of each test that needs empty tables.
    /// </summary>
    public async Task ResetAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        // Created per reset so tables added after the fixture started (migrations, test tables) are included.
        var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = [new Table("__EFMigrationsHistory")],
        }).ConfigureAwait(false);
        await respawner.ResetAsync(connection).ConfigureAwait(false);
    }

    private static async Task ExecuteOnMasterAsync(string sql)
    {
        await using var connection = new SqlConnection(MasterConnectionString());
        await connection.OpenAsync().ConfigureAwait(false);
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    private static string Build(string database) =>
        new SqlConnectionStringBuilder
        {
            DataSource = Required("CRM_SQL_SERVER"),
            InitialCatalog = database,
            UserID = Environment.GetEnvironmentVariable("CRM_SQL_USER") is { Length: > 0 } user ? user : "sa",
            Password = Required("CRM_SQL_PASSWORD"),
            // The devcontainer and CI SQL Server use a self-signed certificate.
            TrustServerCertificate = true,
        }.ConnectionString;

    private static string Required(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"{name} is not set. The devcontainer and CI set it; see docs/testing.md.");
}
