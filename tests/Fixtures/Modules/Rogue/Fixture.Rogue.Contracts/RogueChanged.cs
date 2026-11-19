using Crm.Mediator;

namespace Fixture.Rogue.Contracts;

public sealed record RogueChanged(Guid RogueId) : IIntegrationEvent;
