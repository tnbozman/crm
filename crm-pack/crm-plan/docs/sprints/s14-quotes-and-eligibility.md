# Sprint 14 — Quotes and market eligibility (14 – 25 Jun 2027)

**Release**: R3 Products, research and probity. **Epics**: E16, E17.

## Goal

Sales produce versioned quotes from deals using price books, with approval thresholds and eligibility warnings.

## Demo

1. Build a quote from a deal with services and a product.
2. Add a discount above the threshold: approval required.
3. Send the quote: it becomes immutable; a change creates version 2.
4. Eligibility warning appears for an ineligible market or programme.

## Entry criteria

- S13 merged; `make verify` green on `main`.
- ADR-007 (PDF generation) decided at the start of the sprint after checking library licence terms.
- Approval thresholds agreed with Finance.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S14-1 Quote aggregate

*As a salesperson I price a deal.*

Tests first:
- `Totals_AreDeterministic_RoundedPerRule`
- `Discount_AboveThreshold_RequiresApproval`
- `Send_MakesVersionImmutable`
- `EditAfterSend_CreatesNewVersion`
- `Expired_QuoteCannotBeAccepted`
- `PricesCapturedAtQuoteTime_NotRecomputedLater`
- `Quote_ReferencesDealByIdOnly`

Implement: Quote, lines, versions, status machine in Commerce; price capture from Commerce price lookup.

Done when: API tested with table-driven total cases.

### S14-2 Quote approval

*As an approver I approve discounts.*

Tests first:
- `Approver_CanApprove_OthersCannot`
- `SelfApproval_Rejected`
- `ApprovalInvalidatedByQuoteChange`
- `Approval_Audited`

Implement: In-module state machine (rules engine arrives in S17).

Done when: tested.

### S14-3 Quote document output

*As a salesperson I send a document.*

Tests first:
- `Document_ContainsLinesTotalsAndTerms` (extracted text, not pixels)
- `Document_UsesBrandingTemplate`
- `Document_GenerationFailure_ReturnsProblemDetails`
- `Document_IsNotStoredWithMaskedFields`

Implement: Generate with the ADR-007 choice; store via link or stream, not in the database unless decided.

Done when: ADR-007 and tests complete.

### S14-4 Market eligibility

*As a user I am warned about eligibility issues.*

Tests first:
- `EligibilityRule_MatchesOrgProgrammeAndProduct_ReturnsAdvisoryOrBlocking`
- `AdvisoryWarning_AllowsSend`
- `BlockingWarning_PreventsSend_UntilResolved`
- `Rules_ManagedInConfig_ByAdmin`

Implement: Controlled eligibility properties and rule set in Config; evaluation in Commerce.

Done when: warnings appear on quote and deal APIs.

### S14-5 Quote UI

*As a salesperson I build and compare quotes.*

Tests first:
- Component tests for builder, totals preview, version compare, approval banner
- Playwright: build, request approval, approve, send

Implement: `features/commerce/quotes`.

Done when: journey green.

## Compliance evidence

Approval audit samples; masked-field checks on documents. NIST 3.1.3, 3.3.x.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s14/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s14-quotes-and-eligibility.md.
Check every entry criterion for sprint 14 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S14-1, S14-4, S14-2, S14-3, S14-5.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
