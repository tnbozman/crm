using Crm.SharedKernel.Modules;
using Crm.Testing;
using Microsoft.AspNetCore.Hosting;

namespace Crm.__Module__.Tests;

/// <summary>The real host, with the __Module__ module's connection string pointing at a test database.</summary>
public sealed class __Module__ApiFactory(string connectionString) : TestWebAppFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting($"ConnectionStrings:{ModuleDatabase.ConnectionStringName}", connectionString);
}
