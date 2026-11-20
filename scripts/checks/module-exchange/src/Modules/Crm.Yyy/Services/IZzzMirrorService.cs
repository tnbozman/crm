using Crm.Yyy.Dtos;

namespace Crm.Yyy.Services;

/// <summary>Yyy's side of the S1-5 module exchange (scripts/checks/module-exchange).</summary>
internal interface IZzzMirrorService
{
    /// <summary>Saves a Yyy item with the Zzz item's ID; commits any inbox record added in this scope with it.</summary>
    Task RecordAsync(Guid zzzItemId, string name, CancellationToken cancellationToken);

    /// <summary>Asks Zzz, through the mediator, for its item's summary.</summary>
    Task<ZzzItemResponse> GetZzzItemAsync(Guid zzzItemId, CancellationToken cancellationToken);
}
