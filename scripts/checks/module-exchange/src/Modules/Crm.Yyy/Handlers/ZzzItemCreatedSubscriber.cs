using Crm.Mediator;
using Crm.Mediator.Contracts.Zzz;
using Crm.Yyy.Persistence.Messaging;
using Crm.Yyy.Services;

namespace Crm.Yyy.Handlers;

/// <summary>Receives Zzz's event once (Yyy's inbox), then delegates to Yyy's service.</summary>
internal sealed class ZzzItemCreatedSubscriber(IInbox inbox, IZzzMirrorService mirror) : IEventSubscriber<ZzzItemCreated>
{
    public async Task Handle(ZzzItemCreated integrationEvent, MessageEnvelope envelope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        if (!await inbox.TryReceiveAsync(envelope, nameof(ZzzItemCreatedSubscriber), cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        await mirror.RecordAsync(integrationEvent.ItemId, integrationEvent.Name, cancellationToken).ConfigureAwait(false);
    }
}
