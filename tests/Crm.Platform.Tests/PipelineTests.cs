using Crm.Platform.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Platform.Tests;

public sealed class PipelineTests
{
    [Fact]
    public async Task Send_WithBehaviours_RunsThemInRegistrationOrderAroundTheHandler()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddSingleton<Trace>();
        services.AddScoped<IRequestBehaviour, OuterBehaviour>();
        services.AddScoped<IRequestBehaviour, InnerBehaviour>();
        services.AddRequestHandler<TracedQueryHandler>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new TracedQuery(), TestContext.Current.CancellationToken);

        provider.GetRequiredService<Trace>().ShouldBe(["outer before", "inner before", "handler", "inner after", "outer after"]);
    }
}

internal sealed class Trace : List<string>;

public sealed record TracedQuery : IQuery<int>;

internal sealed class TracedQueryHandler(Trace trace) : IRequestHandler<TracedQuery, int>
{
    public Task<int> Handle(TracedQuery request, CancellationToken cancellationToken)
    {
        trace.Add("handler");
        return Task.FromResult(1);
    }
}

internal abstract class TracingBehaviour(Trace trace, string name) : IRequestBehaviour
{
    public async Task<TResponse> Handle<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> proceed, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        trace.Add($"{name} before");
        var response = await proceed();
        trace.Add($"{name} after");
        return response;
    }
}

internal sealed class OuterBehaviour(Trace trace) : TracingBehaviour(trace, "outer");

internal sealed class InnerBehaviour(Trace trace) : TracingBehaviour(trace, "inner");
