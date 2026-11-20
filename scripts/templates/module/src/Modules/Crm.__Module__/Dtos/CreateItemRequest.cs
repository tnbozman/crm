using System.ComponentModel.DataAnnotations;
using Crm.__Module__.Validation;

namespace Crm.__Module__.Dtos;

internal sealed record CreateItemRequest([Required, StringLength(ItemRules.NameMaxLength)] string Name);
