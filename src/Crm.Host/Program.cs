using Crm.Host;
using Crm.Host.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(NoSignInAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, NoSignInAuthenticationHandler>(NoSignInAuthenticationHandler.SchemeName, configureOptions: null);

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
    .AddPolicy(HostPolicies.Anonymous, policy => policy.RequireAssertion(_ => true));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api/v1");
api.MapGet("/health", () => new HealthResponse("ok")).RequireAuthorization(HostPolicies.Anonymous);

foreach (var endpoints in app.Services.GetServices<IApiEndpoints>())
{
    endpoints.Map(api);
}

app.Run();

internal sealed record HealthResponse(string Status);
