namespace Crm.Platform.Messaging;

/// <summary>Correlation for the current scope: shared by everything one user action causes, carried into envelopes.</summary>
public interface IMessageContext
{
    Guid CorrelationId { get; }

    /// <summary>The message being handled, when this scope is handling an integration event.</summary>
    Guid? CausationId { get; }
}

internal sealed class MessageContext : IMessageContext
{
    public Guid CorrelationId { get; set; }

    public Guid? CausationId { get; set; }
}
