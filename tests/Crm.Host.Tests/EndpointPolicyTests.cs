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
public sealed class EndpointPolicyTests(WebApplicationFactory<Program> factory, TestHostFactory testHost)
    : IClassFixture<WebApplicationFactory<Program>>, IClassFixture<TestHostFactory>
{
    /// <summary>
    /// Endpoints allowed to carry no named policy of their own (S1-2). Health and the SPA shell are anonymous by
    /// HostPolicies.Anonymous; the /api catch-all relies on the fallback policy so signed-in callers get 404 (ADR-011).
    /// </summary>
    private static readonly string[] AllowList =
    [
        "controller Health: api/v1/health",
        "route {*path:nonfile}",
        "route /api/{**path}",
    ];

    [Fact]
    public void All_Endpoints_HaveAuthorisation_OrAreInAnonymousAllowList() =>
        EndpointsWithoutANamedPolicy(factory).ShouldBeEmpty("name a policy with [Authorize(Policy = ...)], or add the endpoint to AllowList with a reason");

    [Fact]
    public void All_Endpoints_HaveAuthorisation_OrAreInAnonymousAllowList_FlagsEndpointsWithoutAPolicy()
    {
        var unprotected = EndpointsWithoutANamedPolicy(testHost);

        unprotected.ShouldBe(
            [
                "controller TestOnly: api/v1/test-only/forbidden",
                "controller TestOnly: api/v1/test-only/invalid",
                "controller TestOnly: api/v1/test-only/me",
                "controller TestOnly: api/v1/test-only/missing",
                "controller TestOnly: api/v1/test-only/protected",
                "controller TestOnly: api/v1/test-only/throws",
            ],
            "the test host's throwaway endpoints name no policy; admin-only names Policies.Admin");
    }

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

    /// <summary>Endpoints that name neither a policy (other than Anonymous) nor roles, and are not on the allow-list.</summary>
    private static List<string> EndpointsWithoutANamedPolicy(WebApplicationFactory<Program> host) =>
        host.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
            .Where(e => !e.Metadata.GetOrderedMetadata<IAuthorizeData>().Any(a =>
                (!string.IsNullOrEmpty(a.Policy) && a.Policy != HostPolicies.Anonymous) || !string.IsNullOrEmpty(a.Roles)))
            .Select(Describe)
            .Except(AllowList, StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

    private List<RouteEndpoint> Endpoints() =>
        factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>().ToList();

    private static string Describe(RouteEndpoint endpoint) =>
        endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is { } action
            ? $"controller {action.ControllerName}: {endpoint.RoutePattern.RawText}"
            : $"route {endpoint.RoutePattern.RawText}";
}
