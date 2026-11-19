using Crm.Platform.Testing.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Crm.Platform.Testing;

/// <summary>
/// The real host for API tests. Plain clients get the host's own authentication (deny by default);
/// <see cref="CreateClientFor"/> signs every request in as a synthetic user through <see cref="FakeAuthHandler"/>.
/// </summary>
public class TestWebAppFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    /// <summary>
    /// A client signed in as <paramref name="user"/>, optionally under another environment (for example "Production"),
    /// optionally capturing the host's logs into <paramref name="logs"/>.
    /// </summary>
    public HttpClient CreateClientFor(TestUser user, string environment = "Development", ILoggerProvider? logs = null) =>
        WithWebHostBuilder(builder =>
            {
                builder
                    .UseEnvironment(environment)
                    .ConfigureTestServices(services => services
                        .AddAuthentication(FakeAuthHandler.SchemeName)
                        .AddScheme<FakeAuthOptions, FakeAuthHandler>(FakeAuthHandler.SchemeName, options => options.User = user));
                if (logs is not null)
                {
                    builder.ConfigureLogging(logging => logging.AddProvider(logs));
                }
            })
            .CreateClient();
}
