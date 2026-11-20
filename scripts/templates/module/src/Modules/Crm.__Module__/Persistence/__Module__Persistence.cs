using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.__Module__.Persistence;

internal static class __Module__Persistence
{
    /// <summary>The database every module shares (one schema each). Managed identity in Azure: no password.</summary>
    public const string ConnectionStringName = "Crm";

    /// <summary>Registers the DbContext. The connection string is read when a context is first created, not at startup.</summary>
    public static IServiceCollection Add__Module__DbContext(this IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<__Module__DbContext>(options => Configure(
            options,
            configuration.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException($"ConnectionStrings:{ConnectionStringName} is not configured.")));

    /// <summary>SQL Server, with the migrations history table in the module's schema.</summary>
    public static TBuilder Configure<TBuilder>(TBuilder options, string connectionString)
        where TBuilder : DbContextOptionsBuilder
    {
        options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, __Module__DbContext.Schema));
        return options;
    }
}
