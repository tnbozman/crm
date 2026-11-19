# Sprint 13 — Product catalogue and price books (31 May – 11 Jun 2027)

**Release**: R3 Products, research and probity. **Epics**: E15.

## Goal

The business keeps a catalogue of products and services with price books and rate cards that quotes can draw on.

## Demo

1. Create a service and a product; archive one.
2. Define a price book with effective dates and a rate card by role.
3. Look up the price for a SKU at a past and a future date.
4. Show cost and margin hidden from a Sales user.

## Entry criteria

- S12 merged; `make verify` green on `main`.
- R2 released; finance has supplied current price and rate information.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S13-1 Catalogue

*As an Admin I manage products and services.*

Tests first:
- `Create_Product_SkuUnique`
- `Archive_Product_KeepsOnExistingQuotes`
- `TaxTreatment_FromConfigOptions`
- `CustomValues_Validated`

Implement: `Crm.Commerce` (schema `commerce`): SKU, name, type, unit, tax treatment, active flag, JSON values.

Done when: API tested.

### S13-2 Price books and rate cards

*As Finance I maintain prices over time.*

Tests first:
- `PriceBooks_OverlappingEffectiveDates_Rejected`
- `GetPrice_AtDate_PicksCorrectBook`
- `RateCard_ByRole_ReturnsDayRate`
- `VolumeTier_AppliesAtQuantity`
- `Currency_MustMatchBook`

Implement: Price book, price entries, rate card entries with effective ranges.

Done when: price lookup tested with table-driven cases.

### S13-3 Cost and margin

*As Finance I record cost and margin; others cannot see them.*

Tests first:
- `CostFields_MaskedForNonFinanceRoles`
- `CostFields_NotInExportSearchOrAi`
- `CostChange_Audited_WithBeforeAfter`

Implement: Field policy from S12-4 applied to cost fields.

Done when: leakage tests across surfaces.

### S13-4 Catalogue UI and import

*As an Admin I maintain the catalogue quickly.*

Tests first:
- Component tests for catalogue list, price book editor and effective-date timeline
- CSV import through the S7 framework with dry run
- Playwright journey

Implement: `features/commerce/`.

Done when: journey green.

### S13-5 Commerce Contracts

*As the quotes feature I read prices through Contracts.*

Tests first:
- `ICommerceQueries.GetPrice` contract tests
- Architecture tests unchanged

Implement: Contracts and events `ProductChanged`, `PriceBookChanged`.

Done when: consumer test uses Contracts only.

## Compliance evidence

Field-masking evidence for cost data. NIST 3.1.3, 3.1.5.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s13/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s13-products-and-price-books.md.
Check every entry criterion for sprint 13 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S13-1, S13-2, S13-3, S13-5, S13-4.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
