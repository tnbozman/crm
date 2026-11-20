using Crm.__Module__.Dtos;
using Crm.__Module__.Mapping;
using Crm.__Module__.Models;
using Crm.__Module__.Persistence;
using Crm.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;

namespace Crm.__Module__.Services;

internal sealed class ItemService(__Module__DbContext db, TimeProvider clock) : IItemService
{
    public async Task<ItemResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await db.Items.AsNoTracking().SingleOrDefaultAsync(i => i.Id == id, cancellationToken).ConfigureAwait(false);
        return item?.ToResponse() ?? throw new NotFoundException($"Item {id} was not found.");
    }

    public async Task<ItemResponse> CreateAsync(CreateItemRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var item = new Item(Guid.CreateVersion7(), request.Name, clock.GetUtcNow());
        db.Items.Add(item);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return item.ToResponse();
    }
}
