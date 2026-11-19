# Sprint 12 — Teams, permissions and R2 hardening (17 – 28 May 2027)

**Release**: R2 Deals and bids. **Epics**: E13.

## Goal

Access control becomes explicit and verified: team administration, record sharing, a documented permission matrix enforced by generated tests, and field-level restrictions.

## Demo

1. Admin creates a team and assigns members; visibility of deals changes accordingly.
2. Share a deal with another team with an expiry date; the share lapses.
3. Show the permission matrix document and the generated test run.
4. Show masked deal value for a role without Finance view.

## Entry criteria

- S11 merged; `make verify` green on `main`.
- Draft permission matrix reviewed by the business owner.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S12-1 Team administration

*As an Admin I manage teams.*

Tests first:
- `CreateTeam_AddRemoveMembers_Audited`
- `UserInMultipleTeams_SeesUnionOfRecords`
- `RemovedMember_LosesAccessImmediately`
- `TeamChange_InvalidatesCachedVisibility`

Implement: Teams UI and API in Access; visibility scope cache with event invalidation.

Done when: tested.

### S12-2 Record sharing

*As a user I share a record with a user or team.*

Tests first:
- `Share_GrantsReadOnly_UntilExpiry`
- `ExpiredShare_NoLongerGrantsAccess`
- `OnlyOwnerOrAdmin_CanShare`
- `Share_IsAudited`
- `SharedRecord_AppearsInSearchOnlyForGrantee`

Implement: Share entries in Access consulted by module query filters through a Contracts interface.

Done when: tested across Directory and Deals.

### S12-3 Permission matrix as tests

*As the security officer I prove the matrix is enforced.*

Tests first:
- Matrix file `docs/permissions-matrix.md` parsed by a test that iterates role × resource × action
- `AddRole_WithoutMatrixEntries_FailsBuild`
- `AddEndpoint_WithoutMatrixEntry_FailsBuild`
- Each cell asserts allowed or forbidden through the API

Implement: Machine-readable matrix (YAML in the doc front matter or a sibling file) plus a test generator.

Done when: matrix covers all current endpoints.

### S12-4 Field-level restrictions

*As an Admin I limit sensitive fields to certain roles.*

Tests first:
- `Value_MaskedInApiResponse_ForRoleWithoutFinanceView`
- `Value_MaskedInExportSearchAndTimeline`
- `MaskedField_NotWritable`

Implement: Field policy attribute in Contracts DTOs and a response filter.

Done when: leakage tests across surfaces.

### S12-5 R2 hardening

*As the security officer I verify R2 before release.*

Tests first:
- Pen-test delta on new endpoints scheduled and findings test-first fixed
- Query review for N+1 on deal lists
- Restore drill repeated with deals data

Implement: Performance fixes, indexes, evidence refresh.

Done when: evidence in `docs/evidence/r2/`.

## Compliance evidence

Permission matrix output; leakage test results; pen-test delta. NIST 3.1.1, 3.1.2, 3.1.5, 3.12.x.

## Release gate

- [ ] Permission matrix tests green
- [ ] No cross-module join detected by architecture tests
- [ ] Pen-test delta clean of highs
- [ ] Restore drill repeated
- [ ] Evidence exported to docs/evidence/r2/

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s12/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s12-teams-permissions-hardening.md.
Check every entry criterion for sprint 12 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S12-1, S12-2, S12-4, S12-3, S12-5.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
