# Sprint 10 — Deal roles and conflict flags (19 – 30 Apr 2027)

**Release**: R2 Deals and bids. **Epics**: E11.

## Goal

Each deal records which organisations and people take part and in what role. Roles are per deal, not global. Conflicts are flagged early.

## Demo

1. Add the same organisation as customer on one deal and competitor on another.
2. Open the organisation page and see its deals grouped by role.
3. Add an organisation that triggers a conflict warning on the deal.
4. Admin adds a role to the catalogue with a probity flag.

## Entry criteria

- S9 merged; `make verify` green on `main`.
- Role catalogue draft agreed with the business (customer, prime, subcontractor, teaming partner, competitor, university, supplier, funder).

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S10-1 Role catalogue

*As an Admin I manage deal roles.*

Tests first:
- `Role_HasFlags_IsCompetitorAndRequiresProbity`
- `ArchiveRole_InUse_KeepsExistingAssignments`
- `RoleKeys_Unique`

Implement: Role catalogue in Config with Contracts `IDealRoles`.

Done when: admin API tested.

### S10-2 Deal participants

*As a user I attach organisations and people to a deal with roles.*

Tests first:
- `AddParticipant_SameOrg_DifferentRolesOnDifferentDeals_Allowed`
- `AddParticipant_DuplicateRoleOnSameDeal_Rejected`
- `AddPerson_MustHaveCurrentOrNoOrgLink_Warn`
- `RemoveParticipant_KeepsHistory`
- `Participant_ReferencesValidatedThroughDirectoryContracts`
- `OrganisationMerged_Event_RepointsParticipants` (inbox idempotent)

Implement: Participant entity in Deals; handlers for Directory merge events.

Done when: API tested.

### S10-3 Organisation deal view

*As a user I see an organisation's involvement across deals.*

Tests first:
- `QueryByOrganisationAndRole_ReturnsOnlyVisibleDeals`
- `Composition_Endpoint_CombinesDirectoryAndDeals_WithoutCrossSchemaJoin`

Implement: Contracts query plus a Host composition endpoint and a UI tab.

Done when: tab renders in Playwright.

### S10-4 Conflict flags

*As a user I am warned before conflicts become problems.*

Tests first:
- `SameOrg_CustomerAndCompetitorInOverlappingLiveDeals_RaisesAdvisoryFlag`
- `ProbityFlaggedRole_RaisesFlag_UntilReviewed` (stub provider interface; real provider in S15)
- `Flags_ReevaluatedWhenParticipantsChange`
- `Flags_VisibleOnlyToUsersWhoCanSeeBothDeals`

Implement: `IConflictChecks` abstraction in Deals Contracts with an in-module rule set; replaceable by Probity.

Done when: flags appear on deal API.

### S10-5 Participants UI

*As a user I manage participants and see warnings.*

Tests first:
- Component tests for add dialog, role chips, warnings banner
- Playwright journey

Implement: Participants panel on deal detail.

Done when: journey green.

## Compliance evidence

Role-per-deal test evidence; conflict rule documentation. Supports probity and conflict-of-interest governance.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s10/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s10-deal-roles-and-conflicts.md.
Check every entry criterion for sprint 10 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S10-1, S10-2, S10-4, S10-3, S10-5.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
