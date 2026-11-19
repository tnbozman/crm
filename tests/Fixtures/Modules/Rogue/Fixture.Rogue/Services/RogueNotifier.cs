using Crm.Mediator;
using Fixture.Rogue.Contracts;

namespace Fixture.Rogue.Services;

/// <summary>Violation: a service publishing directly instead of writing to its outbox (ADR-009).</summary>
internal sealed class RogueNotifier(IEventPublisher publisher, TimeProvider clock)
{
    public Task Changed(Guid rogueId) =>
        publisher.Publish(MessageEnvelope.For(new RogueChanged(rogueId), Guid.NewGuid(), null, null, clock));
}
