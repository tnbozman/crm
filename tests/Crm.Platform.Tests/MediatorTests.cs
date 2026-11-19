using System.Security.Claims;
using Crm.Platform.Messaging;
using Crm.Platform.Security;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Platform.Tests;

public sealed class MediatorTests
{
    [Fact]
    public async Task Send_Query_InvokesTheSingleHandler_AndReturnsItsResponse()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddRequestHandler<EchoQueryHandler>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var reply = await scope.ServiceProvider.GetRequiredService<IMediator>()
            .Send(new EchoQuery("hello"), TestContext.Current.CancellationToken);

        reply.ShouldBe(new EchoReply("hello back"));
    }

    [Fact]
    public async Task Send_WithNoHandler_Throws()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => mediator.Send(new EchoQuery("unheard"), TestContext.Current.CancellationToken));

        exception.Message.ShouldBe("No handler is registered for request Crm.Platform.Tests.EchoQuery.");
    }

    [Fact]
    public async Task Send_RunsInCallersScope_CurrentUserIsTheSignedInUser()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddCurrentUser();
        services.AddRequestHandler<WhoAmIHandler>();
        services.AddScoped<TestUserContext>();
        services.AddScoped<IUserContext>(sp => sp.GetRequiredService<TestUserContext>());
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        var alice = Guid.NewGuid();
        var bob = Guid.NewGuid();
        (await SendAs(provider, alice)).ShouldBe(alice);
        (await SendAs(provider, bob)).ShouldBe(bob);
    }

    private static async Task<Guid> SendAs(IServiceProvider provider, Guid userId)
    {
        await using var scope = provider.CreateAsyncScope();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"));
        scope.ServiceProvider.GetRequiredService<TestUserContext>().Principal = principal;
        return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new WhoAmI(), TestContext.Current.CancellationToken);
    }
}

public sealed record WhoAmI : IQuery<Guid>;

internal sealed class WhoAmIHandler(ICurrentUser user) : IRequestHandler<WhoAmI, Guid>
{
    public Task<Guid> Handle(WhoAmI request, CancellationToken cancellationToken) => Task.FromResult(user.UserId);
}

public sealed record EchoQuery(string Text) : IQuery<EchoReply>;

public sealed record EchoReply(string Text);

internal sealed class EchoQueryHandler : IRequestHandler<EchoQuery, EchoReply>
{
    public Task<EchoReply> Handle(EchoQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(new EchoReply(request.Text + " back"));
}
