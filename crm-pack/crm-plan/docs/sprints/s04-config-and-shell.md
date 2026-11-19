# Sprint 4 — Config module and application shell (25 Jan – 5 Feb 2027)

**Release**: R1 Core CRM. **Epics**: E6.

## Goal

Administrators define custom properties, validations and option lists without code. The SPA has a reusable dynamic form and table system driven by those definitions, inside a proper application shell.

## Demo

1. As Admin, create a custom property `Security clearance level` (single-select) on Organisation.
2. Open the sample object form: the new field appears, validates and saves into the JSON values column.
3. Archive the property: existing values remain readable, new forms no longer show it.
4. Show the app shell navigation, breadcrumbs, empty state and error boundary.

## Entry criteria

- S3 merged; `make verify` green on `main`.
- R0 compliance gate (S3) passed or explicitly waived for development-only data.
- Role names and the permission matrix draft from S1 are agreed.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S4-1 Property definitions

*As an Admin I define custom properties per object type.*

Tests first:
- `Create_TextProperty_StoresDefinitionWithGroupAndOrder`
- `Create_DuplicateNameForSameObjectType_Rejected`
- `Create_SameNameOnDifferentObjectTypes_Allowed`
- `Archive_Property_KeepsExistingValuesReadable`
- `NonAdmin_CannotCreateProperty` (403) and audit entry emitted
- Migrations apply to empty database; no pending model changes

Implement: `Crm.Config` module (schema `config`) scaffolded with `make module-new name=Config`. Types: text, long text, number, money, date, boolean, single-select, multi-select, user, link. Fields: object type, key, label, group, required, default, help text, order, archived. Keys immutable after creation.

Done when: definitions CRUD via API with ETag concurrency; audit events; tests green.

### S4-2 Validation service in Contracts

*As a module author I validate custom values through one shared service.*

Tests first:
- One test per property type for valid, invalid and boundary values (date range, money precision, option not in list)
- `Validate_MissingRequired_ReturnsFieldError`
- `Validate_UnknownKey_ReturnsError`
- `Validate_ArchivedProperty_RejectedOnWrite_AcceptedOnExistingValue`
- Architecture: other modules reference only `Crm.Config.Contracts`

Implement: `IPropertyValidator.Validate(objectType, values) -> ValidationResult` and `IPropertyDefinitions.GetForObjectType`. Cache definitions per request; invalidate on `PropertyDefinitionChanged` event.

Done when: validator used by a sample consumer test through Contracts only.

### S4-3 Option lists and handling markers

*As an Admin I manage controlled vocabularies and flag sensitive properties.*

Tests first:
- `OptionList_CannotRemoveOptionInUse_OnlyArchive`
- `Option_Reorder_PersistsOrder`
- `HandlingMarkedProperty_IsFlaggedInContractsDto`
- `HandlingMarkedProperty_ExcludedFrom_ExportAndAiProjection` (test double of the projection used later in S7 and S26)

Implement: Option list entity shared across properties; boolean `HandlingMarked` on property definition surfaced in Contracts so export and AI code can exclude it.

Done when: admin UI not required yet; API and tests complete.

### S4-4 Dynamic form and table components

*As a user I see forms and tables built from property definitions.*

Tests first:
- Vitest: form renders correct input per type from a definition fixture
- `shows API field errors beside the right input`
- `submits custom values as JSON keyed by property key`
- `table shows chosen columns, sorts and pages`
- axe accessibility check passes for form and table
- MSW contract test uses the generated API types

Implement: `src/web/src/features/config/` plus shared `ui/DynamicForm.vue` and `ui/DynamicTable.vue` built on the OpenVue wrappers.

Done when: components documented with usage examples; tests green.

### S4-5 Property administration screens

*As an Admin I manage properties in the UI.*

Tests first:
- Component tests for create, edit, archive and reorder flows
- Playwright: admin creates a property and sees it on the sample form; ReadOnly user cannot see the admin page

Implement: Config admin pages under `/admin/config`, route guard requires Admin role.

Done when: Playwright journey green.

### S4-6 Application shell polish

*As a user I navigate consistently with clear states.*

Tests first:
- `Nav_ShowsModulesAllowedByRole`
- `ErrorBoundary_ShowsProblemDetailsTitleAndCorrelationId`
- `EmptyState_ShowsCallToAction`
- Keyboard test: skip link, focus trap in dialogs, shortcut `/` focuses search placeholder

Implement: Layout, breadcrumbs, toast service, global error handling, session-expiry dialog from S1.

Done when: keyboard-only walk-through recorded in the sprint log.

## Compliance evidence

Property-definition change audit entries; handling-marker design note linked from the matrix (NIST 3.1.3 control of CUI flow, supports AI boundary).

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s04/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s04-config-and-shell.md.
Check every entry criterion for sprint 4 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Start with S4-1, then S4-2, S4-3, S4-4, S4-5, S4-6.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
