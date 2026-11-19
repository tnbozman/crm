using Crm.Platform.Errors;
using Crm.Platform.Messaging;
using Crm.Platform.Security;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Platform.Tests;

public sealed class RequestAuthorisationTests
{
    [Fact]
    public async Task Send_RequestRequiringFinance_AsSales_ThrowsForbidden_WithoutCallingTheHandler()
    {
        var services = MediatorServices.Create(Roles.Sales);
        services.AddSingleton<Trace>();
        services.AddRequestHandler<GetMarginsHandler>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        await Should.ThrowAsync<ForbiddenException>(() => scope.ServiceProvider.GetRequiredService<IMediator>()
            .Send(new GetMargins(), TestContext.Current.CancellationToken));

        provider.GetRequiredService<Trace>().ShouldBeEmpty();
    }

    [Fact]
    public async Task Send_RequestRequiringFinance_AsFinance_ReachesTheHandler()
    {
        var services = MediatorServices.Create(Roles.Finance);
        services.AddSingleton<Trace>();
        services.AddRequestHandler<GetMarginsHandler>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var margin = await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new GetMargins(), TestContext.Current.CancellationToken);

        margin.ShouldBe(0.25m);
    }

    [Fact]
    public async Task Send_RequestWithoutPolicy_AsAnonymous_ThrowsForbidden()
    {
        var services = MediatorServices.Create();
        services.AddRequestHandler<EchoQueryHandler>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<TestUserContext>().Principal = MediatorServices.Anonymous();

        await Should.ThrowAsync<ForbiddenException>(() => scope.ServiceProvider.GetRequiredService<IMediator>()
            .Send(new EchoQuery("hi"), TestContext.Current.CancellationToken));
    }
}

[RequiresPolicy(Policies.Finance)]
public sealed record GetMargins : IQuery<decimal>;

internal sealed class GetMarginsHandler(Trace trace) : IRequestHandler<GetMargins, decimal>
{
    public Task<decimal> Handle(GetMargins request, CancellationToken cancellationToken)
    {
        trace.Add("handler");
        return Task.FromResult(0.25m);
    }
}
