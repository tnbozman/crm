using Crm.__Module__.Dtos;

namespace Crm.__Module__.Services;

internal interface IItemService
{
    /// <summary>The item, or <see cref="Crm.SharedKernel.Errors.NotFoundException"/>.</summary>
    Task<ItemResponse> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<ItemResponse> CreateAsync(CreateItemRequest request, CancellationToken cancellationToken);
}
