using Crm.Mediator;

namespace Crm.Testing.Outbox;

/// <summary>A fixed correlation for outbox tests (the mediator's own context is internal).</summary>
public sealed class TestMessageContext : IMessageContext
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    public Guid? CausationId { get; init; }
}
