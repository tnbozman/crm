using Crm.__Module__.Services;
using Crm.Mediator;
using Crm.Mediator.Contracts.__Module__;

namespace Crm.__Module__.Handlers;

/// <summary>Answers other modules' <see cref="Get__Module__ItemSummary"/>. Thin: delegates to the service (ADR-009).</summary>
internal sealed class Get__Module__ItemSummaryHandler(IItemService items) : IRequestHandler<Get__Module__ItemSummary, __Module__ItemSummary>
{
    public async Task<__Module__ItemSummary> Handle(Get__Module__ItemSummary request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var item = await items.GetAsync(request.ItemId, cancellationToken).ConfigureAwait(false);
        return new __Module__ItemSummary(item.Id, item.Name);
    }
}
