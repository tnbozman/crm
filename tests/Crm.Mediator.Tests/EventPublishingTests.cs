using Crm.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Mediator.Tests;

public sealed class EventPublishingTests
{
    [Fact]
    public async Task Publish_Event_InvokesEveryHandler()
    {
        var services = MediatorServices.Create();
        services.AddSingleton<Deliveries>();
        services.AddEventSubscriber<AuditOnRename>();
        services.AddEventSubscriber<SearchOnRename>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var renamed = new SampleRenamed(Guid.NewGuid(), "New name");
        var envelope = MessageEnvelope.For(renamed, Guid.NewGuid(), causationId: null, originatingUserId: null, TimeProvider.System);

        await scope.ServiceProvider.GetRequiredService<IEventPublisher>().Publish(envelope, TestContext.Current.CancellationToken);

        provider.GetRequiredService<Deliveries>().ShouldBe(
            [(nameof(AuditOnRename), renamed, envelope.MessageId), (nameof(SearchOnRename), renamed, envelope.MessageId)],
            ignoreOrder: true);
    }

    [Fact]
    public async Task Publish_Event_SubscriberSeesEnvelopeCorrelation_AndMessageAsCausation()
    {
        var services = MediatorServices.Create();
        services.AddSingleton<Trace>();
        services.AddEventSubscriber<CorrelationOnRename>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var envelope = MessageEnvelope.For(new SampleRenamed(Guid.NewGuid(), "x"), Guid.NewGuid(), causationId: null, originatingUserId: null, TimeProvider.System);

        await scope.ServiceProvider.GetRequiredService<IEventPublisher>().Publish(envelope, TestContext.Current.CancellationToken);

        provider.GetRequiredService<Trace>().ShouldBe([$"correlation {envelope.CorrelationId}, causation {envelope.MessageId}"]);
    }
}

internal sealed class CorrelationOnRename(IMessageContext context, Trace trace) : IEventSubscriber<SampleRenamed>
{
    public Task Handle(SampleRenamed integrationEvent, MessageEnvelope envelope, CancellationToken cancellationToken)
    {
        trace.Add($"correlation {context.CorrelationId}, causation {context.CausationId}");
        return Task.CompletedTask;
    }
}

internal sealed class Deliveries : List<(string Subscriber, SampleRenamed Event, Guid MessageId)>;

internal sealed class AuditOnRename(Deliveries deliveries) : IEventSubscriber<SampleRenamed>
{
    public Task Handle(SampleRenamed integrationEvent, MessageEnvelope envelope, CancellationToken cancellationToken)
    {
        deliveries.Add((nameof(AuditOnRename), integrationEvent, envelope.MessageId));
        return Task.CompletedTask;
    }
}

internal sealed class SearchOnRename(Deliveries deliveries) : IEventSubscriber<SampleRenamed>
{
    public Task Handle(SampleRenamed integrationEvent, MessageEnvelope envelope, CancellationToken cancellationToken)
    {
        deliveries.Add((nameof(SearchOnRename), integrationEvent, envelope.MessageId));
        return Task.CompletedTask;
    }
}
