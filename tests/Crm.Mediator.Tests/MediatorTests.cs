using Crm.Mediator;
using Crm.SharedKernel.Security;
using Crm.Testing.Auth;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Mediator.Tests;

public sealed class MediatorTests
{
    [Fact]
    public async Task Send_Query_InvokesTheSingleHandler_AndReturnsItsResponse()
    {
        var services = MediatorServices.Create();
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
        var services = MediatorServices.Create();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => mediator.Send(new EchoQuery("unheard"), TestContext.Current.CancellationToken));

        exception.Message.ShouldBe("No handler is registered for request Crm.Mediator.Tests.EchoQuery.");
    }

    [Fact]
    public async Task Send_RunsInCallersScope_CurrentUserIsTheSignedInUser()
    {
        var services = MediatorServices.Create();
        services.AddRequestHandler<WhoAmIHandler>();
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        var alice = Guid.NewGuid();
        var bob = Guid.NewGuid();
        (await SendAs(provider, alice)).ShouldBe(alice);
        (await SendAs(provider, bob)).ShouldBe(bob);
    }

    private static async Task<Guid> SendAs(IServiceProvider provider, Guid userId)
    {
        await using var scope = provider.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<TestUserContext>().Principal = MediatorServices.SignedIn(userId);
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
