using Crm.Migrator.Tests.Probe;
using Crm.Testing;
using Microsoft.Data.SqlClient;
using Shouldly;
using Xunit;

namespace Crm.Migrator.Tests;

[Trait("Category", TestCategories.Integration)]
public sealed class MigrationRunnerDatabaseTests(SqlFixture database) : IClassFixture<SqlFixture>
{
    [Fact]
    public async Task RunAsync_AppliesEveryModulesMigrations()
    {
        var applied = await MigrationRunner.RunAsync(
            [new ProbeModule()], MigrationRunnerTests.Configuration(database.ConnectionString), "Production", reset: false, TestContext.Current.CancellationToken);

        applied.ShouldBe(["Probe"]);
        (await ScalarAsync(database.ConnectionString, "SELECT OBJECT_ID('probe.Probes')")).ShouldNotBe(DBNull.Value);
    }

    [Fact]
    public async Task RunAsync_ResetInDevelopment_RecreatesTheDatabase_ThenMigrates()
    {
        // A separate database: dropping the class fixture's would break the other test.
        var scratch = new SqlFixture();
        await scratch.InitializeAsync();
        try
        {
            await ScalarAsync(scratch.ConnectionString, "CREATE TABLE dbo.Leftover (Id int NOT NULL); SELECT 1;");

            await MigrationRunner.RunAsync(
                [new ProbeModule()], MigrationRunnerTests.Configuration(scratch.ConnectionString), "Development", reset: true, TestContext.Current.CancellationToken);

            (await ScalarAsync(scratch.ConnectionString, "SELECT OBJECT_ID('dbo.Leftover')")).ShouldBe(DBNull.Value);
            (await ScalarAsync(scratch.ConnectionString, "SELECT OBJECT_ID('probe.Probes')")).ShouldNotBe(DBNull.Value);
        }
        finally
        {
            await scratch.DisposeAsync();
        }
    }

    private static async Task<object> ScalarAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new SqlCommand(sql, connection);
        return await command.ExecuteScalarAsync(TestContext.Current.CancellationToken) ?? DBNull.Value;
    }
}
