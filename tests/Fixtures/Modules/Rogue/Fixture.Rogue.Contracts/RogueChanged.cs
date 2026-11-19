using Crm.Platform.Messaging;

namespace Fixture.Rogue.Contracts;

public sealed record RogueChanged(Guid RogueId) : IIntegrationEvent;
