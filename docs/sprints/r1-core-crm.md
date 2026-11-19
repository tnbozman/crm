# R1 — Core CRM (S4–S8, 25 Jan – 2 Apr 2027)

**Outcome**: the business runs its organisations, people, notes and tasks in the CRM and stops using spreadsheets. Go-live 31 March 2027 (S8 ends 2 April; keep the last two days as buffer).

Each sprint below is a *starting* outline. Before each sprint run the **refine prompt** from `README.md` so the section gets the S0–S3 level of detail using the real code.

Common to all stories: tests first, module boundary rules, audit events, permission positive and negative tests, OpenAPI and client regenerated, evidence notes.

## S4 — Config module and app shell (25 Jan – 5 Feb) — E6

**Goal**: administrators define custom properties, validations and option lists without code. A reusable form and table system in the SPA reads definitions.

Stories:
- **S4-1 Property definitions.** Types: text, long text, number, money, date, boolean, single-select, multi-select, user, link. Per-object-type, groups, required flag, default, help text, order, archived flag. Tests first: invalid definitions rejected; archived properties keep existing values; names unique per object type; migrations and audit.
- **S4-2 Validation service in Contracts.** `IPropertyValidator.Validate(objectType, values)` returns field errors. Tests first for every type and the required rules; other modules will call it.
- **S4-3 Option lists and handling markers.** Controlled vocabularies (industry, region, relationship type); a *handling marker* property flag that stops values reaching the AI assistant or exports.
- **S4-4 Dynamic form and table components** (frontend). Tests first: given definitions, the form renders the right inputs, shows errors from the API and submits the JSON values; table shows chosen columns, sorts and pages.
- **S4-5 App shell polish.** Navigation per module, breadcrumbs, empty states, error boundary, accessible dialogs, keyboard shortcuts for search (stub).

Done when: admin creates a custom field; a sample object form shows it; invalid input rejected by API and UI.

## S5 — Directory: organisations and people (8 – 19 Feb) — E5

Stories:
- **S5-1 Organisation aggregate.** Name, normalised name, aliases, registration identifiers (ABN or ACN, optional), website, industry, region, status, owner team, parent organisation, custom values. Tests first: duplicate detection on normalised name and identifier returns a warning with candidates; parent cycle prevented; soft delete with restore.
- **S5-2 Person aggregate.** Names, emails (multiple, one primary), phones, job title, current employer links, status, custom values. Tests first: email format and uniqueness warnings; merge preview.
- **S5-3 Organisation–person links** with role title, start and end dates (history kept). Tests first: a person can have several current links; ending a link keeps history.
- **S5-4 CRUD API and permissions.** Visibility by team and business unit through global query filters (stub rules: owner team, all-company read). Negative tests: user from another business unit cannot read restricted records.
- **S5-5 Screens.** List with filters, detail with tabs (Overview, People, Timeline placeholder), create and edit forms, duplicate-warning dialog.
- **S5-6 Merge organisations and people.** Merge command with a preview, re-pointing links, `OrganisationMerged` event for other modules, audit entry.
- **S5-7 Directory Contracts.** `IDirectoryQueries` (get by ID, batch get, search by name), events `OrganisationCreated/Updated/Merged`, `PersonCreated/Updated/Merged`.

## S6 — Relationships, notes, tasks and timeline (22 Feb – 5 Mar) — E7, E8

Stories:
- **S6-1 Organisation–organisation relationships** (parent, subsidiary, partner, customer, supplier, university partner; directional with label). Tests first: relationships are directional; duplicates rejected.
- **S6-2 Notes** (Activities module): rich text limited to a safe subset, attached to any record by (type, ID); mentions of users; edit history via audit. Sanitisation tests (XSS payloads).
- **S6-3 Tasks**: owner, due date, status, priority, link to any record, recurrence (simple), overdue calculation. Tests first for state transitions and permissions (only owner or admin can reassign).
- **S6-4 Timeline projection.** Activities consumes Directory events and its own to build a per-record timeline; no cross-schema joins. Tests first: events arrive out of order and duplicated and the timeline is still correct and ordered.
- **S6-5 Screens**: notes panel, task list ("My tasks"), timeline component, record header summary.
- **S6-6 Email and meeting logging (manual)**: log an interaction with participants matched to people by email. M365 automation comes in S19.

## S7 — Search, saved views and import (8 – 19 Mar) — E9

Stories:
- **S7-1 Reporting.search projection.** Reporting module consumes events and maintains a normalised search table (type, ID, display text, tokens, visibility scope). Tests first: eventual consistency within a bounded time, rebuild command from source via Contracts, visibility filter respected (negative tests).
- **S7-2 Global search UI** with type-ahead, keyboard navigation and result grouping. Decide full-text versus `LIKE` (ADR-006) with a measured test on 100k synthetic rows.
- **S7-3 Saved views and filters.** Per user and shared; stored as JSON with a versioned schema. Tests first for migration of older view versions.
- **S7-4 CSV import** for organisations and people: upload, map columns, dry run with row-level errors, commit, undo within a window. Tests first: duplicate handling, partial failure reporting, huge file limits, malicious CSV (formula injection stripping on export, not import).
- **S7-5 CSV export** with permission checks, audit entry, handling-marker exclusion and formula-injection protection.

## S8 — Hardening and go-live (22 Mar – 2 Apr) — E38, E39, E40

Stories:
- **S8-1 Backup and restore exercise** on production-like data; record RPO and RTO; immutability confirmed (E38).
- **S8-2 Performance baseline** at 100k records: p95 under agreed limits for list, search and detail; indexes reviewed from query plans (tests with a seeded benchmark database, thresholds generous enough to avoid flaky CI).
- **S8-3 Penetration test** (external or internal specialist) with scope, retest and remediation of high findings (E40).
- **S8-4 Accessibility pass** (axe in e2e, keyboard-only journey).
- **S8-5 Data migration rehearsal** from the current spreadsheets/HubSpot export: two dry runs, reconciliation report, sign-off.
- **S8-6 Operations runbook**: onboarding and offboarding, role assignment, incident response, restore, key rotation, patch window.
- **S8-7 Training and go-live** checklist; rollback plan; hypercare roster.
- **S8-8 Supply chain and compliance re-check** (E39): SBOM, dependency review, matrix update, system security plan updated.

## R1 go-live gate

Pen-test highs closed; restore exercise recorded; data migration reconciled; all users sign in with phishing-resistant MFA; owner sign-off; evidence pack exported to `docs/evidence/r1/`.

## Claude kickoff prompt (any R1 sprint)

```
Read CLAUDE.md, docs/architecture.md and the section for sprint <N> in docs/sprints/r1-core-crm.md.
First run the refine procedure: rewrite the section with stories, tests-first lists, implementation notes and Done-when criteria,
based on the current code. Show me the rewritten section for approval before implementing anything.
```
