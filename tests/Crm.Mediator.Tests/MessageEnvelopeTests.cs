using System.Text.Json;
using Crm.Mediator;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Xunit;

namespace Crm.Mediator.Tests;

public sealed class MessageEnvelopeTests
{
    [Fact]
    public void MessageEnvelope_ForEvent_CarriesIdsTypeAndPayload()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 19, 9, 30, 0, TimeSpan.Zero));
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sampleId = Guid.NewGuid();

        var envelope = MessageEnvelope.For(new SampleRenamed(sampleId, "New name"), correlationId, causationId, userId, clock);

        envelope.MessageId.ShouldNotBe(Guid.Empty);
        envelope.Type.ShouldBe("Crm.Mediator.Tests.SampleRenamed");
        envelope.OccurredAt.ShouldBe(clock.GetUtcNow());
        envelope.CorrelationId.ShouldBe(correlationId);
        envelope.CausationId.ShouldBe(causationId);
        envelope.OriginatingUserId.ShouldBe(userId);
        JsonSerializer.Deserialize<SampleRenamed>(envelope.Payload, JsonSerializerOptions.Web).ShouldBe(new SampleRenamed(sampleId, "New name"));
    }
}

public sealed record SampleRenamed(Guid SampleId, string Name) : IIntegrationEvent;
