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
        var services = MediatorServices.Create();
        services.AddSingleton<Trace>();
        services.AddScoped<IRequestBehaviour, OuterBehaviour>();
        services.AddScoped<IRequestBehaviour, InnerBehaviour>();
        services.AddRequestHandler<TracedQueryHandler>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new TracedQuery(), TestContext.Current.CancellationToken);

        provider.GetRequiredService<Trace>().ShouldBe(["outer before", "inner before", "handler", "inner after", "outer after"]);
    }

    [Fact]
    public async Task Pipeline_RunsInOrder_CorrelationLoggingValidationAuthorisation()
    {
        await using var provider = MediatorServices.Create().BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        scope.ServiceProvider.GetServices<IRequestBehaviour>().Select(b => b.GetType().Name).ShouldBe(
            ["CorrelationBehaviour", "LoggingBehaviour", "ValidationBehaviour", "AuthorisationBehaviour"]);
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
