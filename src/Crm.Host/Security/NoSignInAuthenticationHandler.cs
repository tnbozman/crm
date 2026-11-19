using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Crm.Host.Security;

/// <summary>
/// Placeholder scheme until Entra ID sign-in lands (S1-6): nobody is ever authenticated,
/// and a challenge answers 401. It holds no credentials and accepts no input.
/// </summary>
internal sealed class NoSignInAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "NoSignIn";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
        Task.FromResult(AuthenticateResult.NoResult());
}
