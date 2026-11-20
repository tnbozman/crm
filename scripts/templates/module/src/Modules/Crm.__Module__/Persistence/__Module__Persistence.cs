using Crm.SharedKernel.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.__Module__.Persistence;

internal static class __Module__Persistence
{
    /// <summary>Registers the DbContext. The connection string is read when a context is first created, not at startup.</summary>
    public static IServiceCollection Add__Module__DbContext(this IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<__Module__DbContext>(options => Configure(
            options,
            configuration.GetConnectionString(ModuleDatabase.ConnectionStringName)
                ?? throw new InvalidOperationException($"ConnectionStrings:{ModuleDatabase.ConnectionStringName} is not configured.")));

    /// <summary>SQL Server, with the migrations history table in the module's schema.</summary>
    public static TBuilder Configure<TBuilder>(TBuilder options, string connectionString)
        where TBuilder : DbContextOptionsBuilder
    {
        options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, __Module__DbContext.Schema));
        return options;
    }
}
