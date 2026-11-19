using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crm.Host.Tests;

/// <summary>The real host plus throwaway endpoints that exist only in tests.</summary>
public sealed class TestHostFactory : WebApplicationFactory<Program>
{
    public const string ProtectedPath = "/api/v1/test-only/protected";
    public const string ThrowingPath = "/api/v1/test-only/throws";
    public const string ExceptionDetail = "sensitive-internal-detail";

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IApiEndpoints, TestOnlyEndpoints>();
            services.AddControllers().AddApplicationPart(typeof(TestOnlyController).Assembly);
        });

    /// <summary>A client whose requests are authenticated as a test user (replaced by FakeAuthHandler in S0-9).</summary>
    public HttpClient CreateSignedInClient(string environment = "Development") =>
        WithWebHostBuilder(builder => builder
                .UseEnvironment(environment)
                .ConfigureTestServices(services => services
                    .AddAuthentication(SignedInHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, SignedInHandler>(SignedInHandler.SchemeName, configureOptions: null)))
            .CreateClient();

    private sealed class SignedInHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "TestSignedIn";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "test-user")], SchemeName);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
        }
    }

    private sealed class TestOnlyEndpoints : IApiEndpoints
    {
        public void Map(IEndpointRouteBuilder api)
        {
            api.MapGet("/test-only/protected", () => Results.Ok("secret"));
            api.MapGet("/test-only/throws", IResult () => throw new InvalidOperationException(ExceptionDetail));
        }
    }
}
