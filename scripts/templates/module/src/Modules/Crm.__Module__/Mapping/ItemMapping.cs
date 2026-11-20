using Crm.__Module__.Dtos;
using Crm.__Module__.Models;

namespace Crm.__Module__.Mapping;

internal static class ItemMapping
{
    public static ItemResponse ToResponse(this Item item) => new(item.Id, item.Name, item.CreatedAt);
}
