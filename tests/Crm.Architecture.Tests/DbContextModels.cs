using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Crm.Architecture.Tests;

/// <summary>Builds a module DbContext's model without a database, so tests can inspect schemas and mappings.</summary>
internal static class DbContextModels
{
    public static IModel Load(Type dbContextType)
    {
        var builder = (DbContextOptionsBuilder)Activator.CreateInstance(typeof(DbContextOptionsBuilder<>).MakeGenericType(dbContextType))!;
        builder.UseSqlServer("Server=model-inspection-only;Database=none"); // never opened

        using var context = (DbContext)Activator.CreateInstance(dbContextType, builder.Options)!;
        return context.GetService<IDesignTimeModel>().Model;
    }
}
