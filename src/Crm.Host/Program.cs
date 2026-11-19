using Crm.Host;
using Crm.Host.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services
    .AddAuthentication(NoSignInAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, NoSignInAuthenticationHandler>(NoSignInAuthenticationHandler.SchemeName, configureOptions: null);

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
    .AddPolicy(HostPolicies.Anonymous, policy => policy.RequireAssertion(_ => true));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// The built SPA (wwwroot) is public: static files are served before authentication.
// UseStaticFiles reads wwwroot from disk, which works for files the frontend build creates (docs/environment.md §4).
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var api = app.MapGroup("/api/v1");

foreach (var endpoints in app.Services.GetServices<IApiEndpoints>())
{
    endpoints.Map(api);
}

// Unknown /api routes never fall through to the SPA: 404 problem details for signed-in callers,
// 401 for anonymous ones through the fallback policy (ADR-011).
app.MapFallback("/api/{**path}", () => Results.Problem(statusCode: StatusCodes.Status404NotFound));

// History-mode client routes get the SPA shell. The shell holds no data, so it is open (ADR-011).
app.MapFallbackToFile("index.html").RequireAuthorization(HostPolicies.Anonymous);

app.Run();
