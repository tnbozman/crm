# Sprint 5 — Directory: organisations and people (8 – 19 Feb 2027)

**Release**: R1 Core CRM. **Epics**: E5.

## Goal

The Directory module holds organisations and people with duplicate detection, history of links, merge, and visibility rules, with usable list and detail screens.

## Demo

1. Create an organisation; creating a near-duplicate shows a warning with candidates.
2. Add a person and link them to the organisation with a job title and start date; end the link and see history retained.
3. Merge two duplicate people with a preview; show the audit entry and the merged event.
4. Log in as a user from another business unit and show the restricted record is not visible.

## Entry criteria

- S4 merged; `make verify` green on `main`.
- Config module validates custom values (S4).
- Business owner has supplied the list of required standard fields and the duplicate rules.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S5-1 Organisation aggregate

*As a user I create organisations with standard and custom fields.*

Tests first:
- `Create_Organisation_NormalisesNameForDuplicateDetection`
- `Create_WithSameAbnOrAcn_ReturnsDuplicateWarningWithCandidates`
- `SetParent_CreatingCycle_Rejected`
- `SoftDelete_ThenRestore_RoundTrips`
- `CustomValues_ValidatedThroughConfigContracts` (fake `IPropertyValidator` plus one real integration test)
- Migrations tests

Implement: `Crm.Directory` (schema `directory`). Fields: name, normalised name, aliases, registration identifiers, website, industry, region, status, owner team ID, business unit ID, parent ID, JSON values, rowversion. Unique index on normalised name only as a warning, not a constraint.

Done when: API CRUD with problem-details validation and concurrency.

### S5-2 Person aggregate

*As a user I record people with multiple emails and phones.*

Tests first:
- `AddEmail_Duplicate_ReturnsWarning`
- `SetPrimaryEmail_OnlyOnePrimary`
- `Create_InvalidEmailFormat_Rejected`
- `PersonNames_AreNormalisedForSearch`

Implement: Person entity with child collections for emails and phones; same visibility fields as organisations.

Done when: API CRUD; tests green.

### S5-3 Organisation–person links with history

*As a user I see who works where now and previously.*

Tests first:
- `Link_PersonToTwoOrganisations_BothCurrent`
- `EndLink_SetsEndDate_KeepsHistory`
- `EndDate_BeforeStartDate_Rejected`
- `CurrentLinks_QueryExcludesEnded`

Implement: `OrganisationPersonLink` with title, role label, start, end.

Done when: link endpoints and queries tested.

### S5-4 Visibility and permissions

*As a user I only see records I am allowed to see.*

Tests first:
- Matrix test: two business units × list, detail, search-by-name, link endpoints → other unit gets empty or 404
- `GlobalQueryFilter_AppliedToBothAggregates`
- `OwnerTeamMember_CanEdit_OthersReadOnly`
- `AllCompanyReadFlag_AllowsReadAcrossTeams`
- Architecture: every Directory `IQueryable` entry point goes through the filtered context

Implement: Global query filters using `ICurrentUser` team and business-unit IDs; rules in one policy class so S12 and S16 extend it.

Done when: negative tests prove no leakage.

### S5-5 Organisation and person screens

*As a user I browse, view and edit records.*

Tests first:
- Component tests for list filters and paging, detail tabs (Overview, People, Timeline placeholder), create and edit forms with duplicate dialog
- Playwright: create, find, edit and link

Implement: Feature folder `features/directory/`; uses `DynamicForm` for custom values.

Done when: Playwright journey green; axe checks pass.

### S5-6 Merge

*As a user I merge duplicates safely.*

Tests first:
- `MergePreview_ListsFieldConflicts_AndLinksToMove`
- `Merge_RepointsLinks_AndSoftDeletesLoser`
- `Merge_EmitsOrganisationMerged_WithLoserAndWinnerIds`
- `Merge_AcrossBusinessUnits_Forbidden`
- `Merge_IsAudited_WithBeforeAfter`
- Other-module consumer test: a handler updates its stored IDs from the event (fake subscriber)

Implement: Merge command, preview endpoint, UI wizard. Other modules adopt the event as they arrive.

Done when: merge tested for both aggregates.

### S5-7 Directory Contracts

*As a module author I read directory data without touching its schema.*

Tests first:
- `GetByIds_ReturnsOnlyVisibleRecords`
- `SearchByName_ReturnsTopNWithVisibility`
- Architecture tests confirm Contracts contain only interfaces, DTOs and events

Implement: `IDirectoryQueries`, DTOs, events `OrganisationCreated/Updated/Merged`, `PersonCreated/Updated/Merged`, `LinkChanged`.

Done when: consumed by a test module through Contracts.

## Compliance evidence

Visibility matrix test output; merge audit sample; data model diagram committed in `docs/`. NIST 3.1.1, 3.1.3, 3.3.1.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s05/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s05-directory-organisations-people.md.
Check every entry criterion for sprint 5 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S5-1, S5-2, S5-3, S5-4, S5-7, S5-5, S5-6.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
