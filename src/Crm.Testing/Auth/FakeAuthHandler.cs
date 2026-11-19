using System.Security.Claims;
using System.Text.Encodings.Web;
using Crm.SharedKernel.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crm.Testing.Auth;

public sealed class FakeAuthOptions : AuthenticationSchemeOptions
{
    /// <summary>The user every request is signed in as.</summary>
    public TestUser? User { get; set; }
}

/// <summary>Test-only authentication scheme: signs every request in as <see cref="FakeAuthOptions.User"/>.</summary>
public sealed class FakeAuthHandler(IOptionsMonitor<FakeAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<FakeAuthOptions>(options, logger, encoder)
{
    public const string SchemeName = "FakeAuth";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Options.User is not { } user)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            .. user.Roles.Select(role => new Claim(ClaimTypes.Role, role)),
            .. user.TeamIds.Select(id => new Claim(CrmClaimTypes.Team, id.ToString())),
            .. user.BusinessUnitIds.Select(id => new Claim(CrmClaimTypes.BusinessUnit, id.ToString())),
        ];
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
