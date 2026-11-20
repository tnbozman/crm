using System.Collections.Concurrent;
using Crm.Mediator;

namespace Crm.Testing.Outbox;

/// <summary>
/// Stands in for the mediator's IEventPublisher under a relay. Records each successful publish; <see cref="OnPublish"/>
/// can fail, block or cancel a delivery.
/// </summary>
public sealed class RecordingEventPublisher : IEventPublisher
{
    private int attempts;

    public Func<MessageEnvelope, CancellationToken, Task>? OnPublish { get; init; }

    public ConcurrentQueue<MessageEnvelope> Published { get; } = new();

    public int Attempts => attempts;

    public async Task Publish(MessageEnvelope envelope, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref attempts);
        if (OnPublish is not null)
        {
            await OnPublish(envelope, cancellationToken).ConfigureAwait(false);
        }

        Published.Enqueue(envelope);
    }
}
