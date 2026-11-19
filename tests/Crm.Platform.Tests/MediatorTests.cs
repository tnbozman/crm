using Crm.Platform.Messaging;
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
}

public sealed record EchoQuery(string Text) : IQuery<EchoReply>;

public sealed record EchoReply(string Text);

internal sealed class EchoQueryHandler : IRequestHandler<EchoQuery, EchoReply>
{
    public Task<EchoReply> Handle(EchoQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(new EchoReply(request.Text + " back"));
}
