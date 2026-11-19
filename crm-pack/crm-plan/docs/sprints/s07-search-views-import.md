# Sprint 7 — Search, saved views and CSV import/export (8 – 19 Mar 2027)

**Release**: R1 Core CRM. **Epics**: E9.

## Goal

Users find anything quickly, save their filters, and the business loads its existing data through a safe import with dry-run and undo.

## Demo

1. Type three letters in global search and open an organisation with the keyboard.
2. Save a filtered organisation view and share it with a team.
3. Import a CSV with duplicates and errors: dry run shows row-level issues, commit, then undo.
4. Export a list: handling-marked columns are excluded and a formula-injection cell is neutralised.

## Entry criteria

- S6 merged; `make verify` green on `main`.
- Sample real (de-identified) export from the current system available for import testing.
- Decision pending on search approach (ADR-006) is made at the start of S7-2 with measured data.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S7-1 Search projection

*As a user I search across record types.*

Tests first:
- `Index_UpdatesWithin_BoundedTime_AfterDirectoryEvent`
- `Index_Rebuild_FromContractsQueries_MatchesIncremental`
- `Search_RespectsVisibilityScope` (two business units)
- `Search_HandlingMarkedValues_NotIndexed`
- `Search_Ranks_ExactNameBeforePrefixBeforeAlias`

Implement: `Crm.Reporting` module (schema `reporting`) with `search_documents`; normalisation (case, diacritics, punctuation) and token columns; visibility scope columns copied from events.

Done when: projection tested for organisations and people.

### S7-2 Search approach decision and global search UI

*As a user I get results as I type.*

Tests first:
- Benchmark test (category `Integration`, generous thresholds) over 100k synthetic documents: p95 under the limit for `LIKE` prefix search
- Record the comparison with SQL full-text in ADR-006
- Vitest: debounce, keyboard navigation, grouped results, no-result state
- Playwright: `/` shortcut opens search

Implement: Choose `LIKE` over normalised tokens or add a full-text index per the benchmark; implement the endpoint and the UI.

Done when: ADR-006 written with numbers.

### S7-3 Saved views

*As a user I save and share filtered lists.*

Tests first:
- `SavedView_StoresFilterJsonWithSchemaVersion`
- `OldVersionView_IsMigratedOnRead`
- `SharedView_VisibleToTeamOnly`
- `SavedView_WithInvalidFilter_Rejected`

Implement: View entity in Config (user preferences) or Reporting; versioned JSON schema shared with the frontend through the generated client.

Done when: UI selector for views tested.

### S7-4 CSV import

*As an Admin I import data safely.*

Tests first:
- `Upload_AboveSizeLimit_Rejected`
- `DryRun_ReportsRowErrors_WithoutWriting`
- `Commit_CreatesRecords_AndEmitsEvents`
- `Duplicates_AreReportedAndSkippedOrMergedPerOption`
- `PartialFailure_ReportsEachRow_AndIsAtomicPerBatch`
- `Undo_WithinWindow_RemovesImportedRecords`
- `Import_RequiresAdminRole_AndIsAudited`

Implement: Import framework (generic): column mapping, validation through Contracts of the target module, batch commit, import run record with undo. Directory is the first target.

Done when: round-trip test with the sample export.

### S7-5 CSV export

*As a user I export lists I may see.*

Tests first:
- `Export_OnlyVisibleRows`
- `Export_ExcludesHandlingMarkedColumns`
- `Export_CellsStartingWithEqualsPlusMinusAt_AreNeutralised`
- `Export_IsAudited_WithRowCountAndFilter`
- `Export_RowLimit_Enforced`

Implement: Streaming export endpoint with permission check and audit.

Done when: tests green; role allowed to export is configurable.

### S7-6 Import and search screens

*As an Admin I walk through the import wizard.*

Tests first:
- Component tests for mapping and dry-run result tables
- Playwright: import with errors, fix, commit, undo

Implement: `features/imports/`.

Done when: Playwright journey green.

## Compliance evidence

Export audit samples and the formula-injection test output. NIST 3.1.3 (information flow), 3.8.x media export controls, 3.14.x.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s07/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s07-search-views-import.md.
Check every entry criterion for sprint 7 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S7-1, S7-2, S7-3, S7-5, S7-4, S7-6.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
