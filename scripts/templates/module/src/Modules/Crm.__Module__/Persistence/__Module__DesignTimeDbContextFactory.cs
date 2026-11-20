using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.__Module__.Persistence;

/// <summary>Lets make migration-add build the model without the host or a database (dotnet-ef never connects for "migrations add").</summary>
internal sealed class __Module__DesignTimeDbContextFactory : IDesignTimeDbContextFactory<__Module__DbContext>
{
    public __Module__DbContext CreateDbContext(string[] args) =>
        new(__Module__Persistence.Configure(new DbContextOptionsBuilder<__Module__DbContext>(), "Server=design-time-only;Database=none").Options);
}
