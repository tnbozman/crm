using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Host.Tests;

/// <summary>The real host plus throwaway endpoints that exist only in tests.</summary>
public sealed class TestHostFactory : WebApplicationFactory<Program>
{
    public const string ProtectedPath = "/api/v1/test-only/protected";

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services => services.AddSingleton<IApiEndpoints, TestOnlyEndpoints>());

    private sealed class TestOnlyEndpoints : IApiEndpoints
    {
        public void Map(IEndpointRouteBuilder api) => api.MapGet("/test-only/protected", () => Results.Ok("secret"));
    }
}
