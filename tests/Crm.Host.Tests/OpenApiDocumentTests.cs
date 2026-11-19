using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Host.Tests;

public sealed class OpenApiDocumentTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task OpenApiDocument_Generated_DescribesHealthEndpoint()
    {
        var provider = factory.Services.GetRequiredKeyedService<IOpenApiDocumentProvider>("v1");

        var document = await provider.GetOpenApiDocumentAsync(TestContext.Current.CancellationToken);

        document.Paths.ShouldNotBeNull().Keys.ShouldContain("/api/v1/health");
    }
}
