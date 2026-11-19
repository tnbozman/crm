using Crm.Testing;
using Crm.Testing.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Host.Tests;

/// <summary>The real host plus the test-only controller (<see cref="TestOnlyController"/>).</summary>
public sealed class TestHostFactory : TestWebAppFactory<Program>
{
    public const string ProtectedPath = "/api/v1/test-only/protected";
    public const string ThrowingPath = "/api/v1/test-only/throws";
    public const string ExceptionDetail = "sensitive-internal-detail";

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
            services.AddControllers().AddApplicationPart(typeof(TestOnlyController).Assembly));

    /// <summary>A client signed in as a synthetic user with no roles.</summary>
    public HttpClient CreateSignedInClient(string environment = "Development") =>
        CreateClientFor(new TestUserBuilder().Build(), environment);
}
