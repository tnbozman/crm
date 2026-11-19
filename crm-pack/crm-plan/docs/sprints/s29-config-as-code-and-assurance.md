# Sprint 29 — Configuration as code and annual assurance (10 – 21 Jan 2028)

**Release**: R6 AI assistant and assurance. **Epics**: E34, E41.

## Goal

Configuration moves through environments as reviewed code, and the annual assurance pack can be produced from the system.

## Demo

1. Export configuration to JSON, change it in Git, dry-run and apply to test.
2. Run the access-review workflow for service identities and roles.
3. Generate the annual assurance pack.
4. Run a retention and disposal job in test.

## Entry criteria

- S28 merged; `make verify` green on `main`.
- Assurance calendar agreed (access reviews, restore drills, pen tests, policy review).

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S29-1 Config export and import

*As an Admin I promote configuration.*

Tests first:
- `Export_ThenImport_RoundTrips`
- `Import_DryRun_ShowsDiff`
- `Import_RequiresApproval`
- `Import_InvalidFile_RejectedWithoutPartialApply`
- `Secrets_NeverExported`

Implement: JSON in Git for Config, pipelines, roles, rules.

Done when: tested.

### S29-2 Annual assurance pack

*As the security officer I gather evidence quickly.*

Tests first:
- `Pack_ContainsAccessReviewBackupDrillPatchStatusAuditSample`
- `Pack_IsReproducible`

Implement: Scripts under `scripts/assurance/` writing to `docs/evidence/annual/`.

Done when: pack generated.

### S29-3 Access review workflow

*As a manager I review who has access.*

Tests first:
- `Review_ListsUsersRolesAndServiceIdentities`
- `ReviewerCannotApproveOwnAccess`
- `Outcome_RecordedAndActionsCreated`

Implement: Review campaigns in Access.

Done when: tested.

### S29-4 Retention and disposal

*As the owner I delete what we must.*

Tests first:
- `Disposal_RemovesPastRetention_AndAudits`
- `LegalHold_PreventsDisposal`
- `Backups_RetentionDocumented`

Implement: Configurable retention jobs with dry run.

Done when: tested.

## Compliance evidence

Assurance pack, access review record. NIST 3.9.x, 3.12.x.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s29/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s29-config-as-code-and-assurance.md.
Check every entry criterion for sprint 29 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S29-1, S29-3, S29-4, S29-2.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
