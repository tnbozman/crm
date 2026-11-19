# Sprint 8 — Hardening and go-live (22 Mar – 2 Apr 2027)

**Release**: R1 Core CRM. **Epics**: E38, E39, E40.

## Goal

The CRM is safe, recoverable, performant and supported enough for the business to move onto it. Core CRM live by 31 March 2027, with two days of buffer.

## Demo

1. Restore production-like data from backup into a scratch environment and run smoke tests; show RPO and RTO.
2. Show the load-test results at 100k records and the penetration-test retest summary.
3. Walk through the migration reconciliation report from the rehearsal.
4. Run the onboarding/offboarding runbook for a new user end to end.
5. Perform the go-live checklist.

## Entry criteria

- S7 merged; `make verify` green on `main`.
- Penetration tester engaged and scheduled for the last two weeks of S7 and S8.
- Spreadsheet or HubSpot export for the migration rehearsal available.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S8-1 Backup and restore exercise

*As an operator I know we can recover.*

Tests first:
- `scripts/checks/restore-drill.sh` restores the latest backup, runs smoke tests and records duration
- Immutable copy verified as unmodifiable by backup administrators in a documented test
- Projection rebuild after restore completes within the target

Implement: Update runbook; record RPO and RTO; schedule the drill quarterly.

Done when: drill report saved in `docs/evidence/r1/`.

### S8-2 Performance baseline at 100k records

*As a user I get fast lists and search.*

Tests first:
- Seeded benchmark database (Bogus) with 100k organisations/people and 200k notes
- p95 thresholds for list, detail, search, timeline asserted in `Category=Performance` tests run nightly
- Query plan review recorded; missing indexes added with migrations

Implement: Indexes, paging fixes, projection tuning.

Done when: numbers recorded and thresholds committed.

### S8-3 Penetration test and remediation

*As the security officer I have independent assurance.*

Tests first:
- Scope document and rules of engagement approved
- Every high finding gets a regression test before the fix
- Retest confirms closure

Implement: Coordinate tester access to the test environment; track findings as issues linked to tests.

Done when: no open high findings; mediums have dated plans in the POA&M.

### S8-4 Accessibility and usability pass

*As a user with assistive technology I can use the CRM.*

Tests first:
- axe run across all routes in Playwright with zero serious violations
- Keyboard-only journey script for the five main tasks

Implement: Fix findings; document known gaps.

Done when: report saved.

### S8-5 Data migration rehearsal

*As the business owner I trust the migrated data.*

Tests first:
- Import of the sample export reconciles counts and key fields (`ReconciliationReport` test)
- Second rehearsal on fresh database yields identical results (idempotent)
- Rollback of a failed import leaves no partial data

Implement: Use the S7 import framework plus a mapping file in `docs/migration/`; reconciliation script and sign-off template.

Done when: two rehearsals recorded, owner sign-off.

### S8-6 Operations runbook

*As an operator I follow written procedures.*

Tests first:
- Runbook review checklist executed by someone who did not write it
- Onboarding and offboarding procedure executed once in test

Implement: `docs/runbook.md`: user lifecycle, role assignment, incident response, restore, key rotation, patch window, break-glass use, support contacts.

Done when: runbook reviewed and stored.

### S8-7 Supply chain and compliance re-check

*As the security officer I confirm the baseline still holds.*

Tests first:
- Dependency and secret scans clean of criticals
- SBOM generated for the release
- Compliance matrix rows updated with evidence links
- System security plan and POA&M updated

Implement: Run S3 gate script again against production resources.

Done when: gate script green on production.

### S8-8 Training, go-live and hypercare

*As a user I am ready on day one.*

Tests first:
- Go-live checklist with owners and rollback plan
- Smoke tests run against production after deploy
- Hypercare roster for two weeks

Implement: Training sessions, quick-reference guide, announcement, cut-over window, spreadsheet retirement date.

Done when: go-live approved by business owner and security officer.

## Compliance evidence

Restore drill, pen-test report summary, load-test numbers, migration reconciliation, gate script output.

## Release gate

- [ ] Pen-test high findings closed
- [ ] Restore drill recorded
- [ ] Migration reconciled twice
- [ ] All users on phishing-resistant MFA
- [ ] Business owner sign-off
- [ ] Evidence exported to docs/evidence/r1/

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s08/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s08-hardening-and-go-live.md.
Check every entry criterion for sprint 8 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S8-1, S8-2, S8-5, S8-3 (in parallel with external tester), S8-4, S8-6, S8-7, S8-8.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
