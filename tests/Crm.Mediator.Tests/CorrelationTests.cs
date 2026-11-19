using Crm.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Mediator.Tests;

public sealed class CorrelationTests
{
    [Fact]
    public async Task Send_WithoutCorrelation_AssignsOneSharedByTheScope()
    {
        var services = MediatorServices.Create();
        services.AddRequestHandler<CorrelationQueryHandler>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var first = await mediator.Send(new CorrelationQuery(), TestContext.Current.CancellationToken);
        var second = await mediator.Send(new CorrelationQuery(), TestContext.Current.CancellationToken);

        first.ShouldNotBe(Guid.Empty);
        second.ShouldBe(first);
    }
}

public sealed record CorrelationQuery : IQuery<Guid>;

internal sealed class CorrelationQueryHandler(IMessageContext context) : IRequestHandler<CorrelationQuery, Guid>
{
    public Task<Guid> Handle(CorrelationQuery request, CancellationToken cancellationToken) => Task.FromResult(context.CorrelationId);
}
