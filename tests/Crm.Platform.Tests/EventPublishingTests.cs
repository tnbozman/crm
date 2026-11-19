using Crm.Platform.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Crm.Platform.Tests;

public sealed class EventPublishingTests
{
    [Fact]
    public async Task Publish_Event_InvokesEveryHandler()
    {
        var services = new ServiceCollection();
        services.AddMediator();
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
