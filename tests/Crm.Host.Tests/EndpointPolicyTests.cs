using Crm.Host.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Host.Tests;

/// <summary>ADR-011: endpoints open to anonymous callers only through named policies, and only where allowed.</summary>
[Trait("Category", "Architecture")]
public sealed class EndpointPolicyTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public void HealthController_IsOnlyEndpointUsingAnonymousPolicy()
    {
        var anonymous = Endpoints()
            .Where(e => e.Metadata.GetOrderedMetadata<IAuthorizeData>().Any(a => a.Policy == HostPolicies.Anonymous))
            .Select(Describe)
            .Order(StringComparer.Ordinal)
            .ToList();

        anonymous.ShouldBe(["controller Health: api/v1/health", "route {*path:nonfile}"]);
    }

    [Fact]
    public void Endpoints_DoNotUseAllowAnonymous()
    {
        var allowAnonymous = Endpoints().Where(e => e.Metadata.GetMetadata<IAllowAnonymous>() is not null).Select(Describe).ToList();

        allowAnonymous.ShouldBeEmpty("open an endpoint with a named HostPolicies policy, not [AllowAnonymous] (ADR-011)");
    }

    private List<RouteEndpoint> Endpoints() =>
        factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>().ToList();

    private static string Describe(RouteEndpoint endpoint) =>
        endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is { } action
            ? $"controller {action.ControllerName}: {endpoint.RoutePattern.RawText}"
            : $"route {endpoint.RoutePattern.RawText}";
}
