using Crm.Mediator;

namespace Fixture.Contracts.Rogue;

public sealed record RogueChanged(Guid RogueId) : IIntegrationEvent;
