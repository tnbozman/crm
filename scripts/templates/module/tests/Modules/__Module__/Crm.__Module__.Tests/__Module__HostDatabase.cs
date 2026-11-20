using Crm.__Module__.Persistence;
using Crm.Migrator;
using Crm.SharedKernel.Modules;
using Crm.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Crm.__Module__.Tests;

/// <summary>
/// A fresh database with every module's migrations, applied by Crm.Migrator, one per test class. For API tests: the real
/// host composes every module, and their relays and subscribers need their own tables too.
/// </summary>
public sealed class __Module__HostDatabase : IAsyncLifetime
{
    private readonly SqlFixture sql = new();

    public string ConnectionString => sql.ConnectionString;

    internal __Module__DbContext CreateDbContext() =>
        new(__Module__Persistence.Configure(new DbContextOptionsBuilder<__Module__DbContext>(), ConnectionString).Options);

    public async ValueTask InitializeAsync()
    {
        await sql.InitializeAsync();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [$"ConnectionStrings:{ModuleDatabase.ConnectionStringName}"] = ConnectionString })
            .Build();
        await MigrationRunner.RunAsync(MigratorModules.All, configuration, "Test", reset: false, CancellationToken.None);
    }

    public ValueTask DisposeAsync() => sql.DisposeAsync();
}
