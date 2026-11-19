# R3 — Products, quotes, programmes and probity (S13–S16, 31 May – 23 Jul 2027)

**Outcome**: the business can quote products and services, track grants and research programmes with university partners, and run a probity register with business-unit separation.

Use the **refine prompt** before each sprint. Common rules from `CLAUDE.md` apply.

## S13 — Product catalogue and price books (E15), 31 May – 11 Jun

- **S13-1 Product and service catalogue** (Commerce module): SKU, name, type (service, product, licence, support), unit, tax treatment, active flag, custom values. Tests first: SKU uniqueness; archived products remain on old quotes.
- **S13-2 Price books** with effective dates, currency, rate cards by role for services (day rates), volume tiers. Tests first: overlapping effective dates rejected; price lookup picks the correct book at a date.
- **S13-3 Cost and margin fields** with field-level restrictions (Finance and Admin only).
- **S13-4 Catalogue UI** and import from CSV (reuse S7 import framework).
- **S13-5 Commerce Contracts**: `ICommerceQueries.GetPrice(sku, date, priceBookId)`.

## S14 — Quotes and market eligibility (E16, E17), 14 – 25 Jun

- **S14-1 Quote aggregate** linked to a deal: line items, discounts with approval thresholds, taxes, validity, versions, status (draft, approved, sent, accepted, expired). Tests first: totals are deterministic and rounded per rule; approval required above threshold; immutability after sent (new version instead).
- **S14-2 Quote approval workflow** using the Automation hooks stubbed as in-module state machine (full rules engine arrives in S17).
- **S14-3 Quote document output**: decision on PDF generation library and licence (ADR-007) before implementation; template with company branding; tests compare extracted text and totals, not pixels.
- **S14-4 Market eligibility**: record which markets and programmes an organisation or product is eligible for (e.g. export controls flags, approved-vendor lists, security accreditation levels) as controlled properties; warnings on deals and quotes. Tests first: ineligible combination raises a blocking or advisory warning as configured.
- **S14-5 UI**: quote builder, version compare, approval banner.

## S15 — Programmes, grants and probity register (E18, E19), 28 Jun – 9 Jul

- **S15-1 Programme and grant tracking** (Programmes module): programme, funding body, round, milestones, reporting dates, budget, matched funding, partners (organisation IDs), linked deals. Tests first: milestone date rules; budget totals; partner role uniqueness.
- **S15-2 University partner management**: partner agreements, key contacts, IP terms summary fields (links to documents, not content).
- **S15-3 Probity module — declarations**: users declare interests (organisations, relationships) with dates and status; reviewers approve. Tests first: declarations visible only to the declarer, reviewers and auditors.
- **S15-4 Probity checks API** in Contracts: `IProbityChecks.Evaluate(subject, context)` returns clear/advisory/blocking with reasons.
- **S15-5 UI**: programme dashboard, milestone calendar, declaration forms.

## S16 — Probity gate, business-unit separation and hardening (E19, E20), 12 – 23 Jul

- **S16-1 Probity gate on deals and quotes**: stage transitions and quote sending call the probity API; blocking results stop the action unless an override with reason and approver is recorded. Tests first for every block, override and audit.
- **S16-2 Business-unit separation**: records tagged with business unit; users restricted to their unit; cross-unit sharing needs explicit approval; global filters in every module. Tests first: matrix of two units × all list and detail endpoints, search results, exports, and AI tools.
- **S16-3 Reporting visibility**: projections carry visibility scope; negative tests for leakage via search and exports.
- **S16-4 R3 hardening**: performance, pen-test delta, evidence refresh, data reconciliation for imported programmes.

## R3 release gate

Business-unit isolation tests green across all modules; probity overrides audited; pen-test delta clean; `docs/evidence/r3/` exported.
