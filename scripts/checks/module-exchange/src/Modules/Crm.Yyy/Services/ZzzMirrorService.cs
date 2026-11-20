using Crm.Mediator;
using Crm.Mediator.Contracts.Zzz;
using Crm.Yyy.Dtos;
using Crm.Yyy.Models;
using Crm.Yyy.Persistence;

namespace Crm.Yyy.Services;

internal sealed class ZzzMirrorService(YyyDbContext db, IMediator mediator, TimeProvider clock) : IZzzMirrorService
{
    public async Task RecordAsync(Guid zzzItemId, string name, CancellationToken cancellationToken)
    {
        db.Items.Add(new Item(zzzItemId, $"Seen {name}", clock.GetUtcNow()));
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<ZzzItemResponse> GetZzzItemAsync(Guid zzzItemId, CancellationToken cancellationToken)
    {
        var summary = await mediator.Send(new GetZzzItemSummary(zzzItemId), cancellationToken).ConfigureAwait(false);
        return new ZzzItemResponse(summary.ItemId, summary.Name);
    }
}
