# R2 — Deals and bids (S9–S12, 5 Apr – 28 May 2027)

**Outcome**: opportunities, bids and the organisations involved in each (per-deal roles) are managed in the CRM, with conflict-of-interest flags, notifications and proper team permissions.

Use the **refine prompt** before each sprint. Common rules from `CLAUDE.md` apply to every story.

## S9 — Deals and pipelines (E10), 5 – 16 Apr

- **S9-1 Pipeline and stage definitions** (Config): multiple pipelines (professional services, product, grants), stages with probability, rotting thresholds, required-fields-per-stage. Tests first: cannot delete a stage with deals; stage order changes keep history.
- **S9-2 Deal aggregate** (Deals module): name, owner, pipeline, stage, value (money with currency), expected close date, source, status (open/won/lost), loss reason, custom values; references to organisation and person IDs only. Tests first: stage transitions enforce required fields; won/lost rules; concurrent edit conflict.
- **S9-3 Stage history and velocity**: events and projections of time in stage.
- **S9-4 Deal board and list UI**: Kanban with drag-and-drop (keyboard-accessible alternative), list with filters, deal detail page composed with Directory data via Contracts.
- **S9-5 Deal visibility rules**: owner team and explicit sharing; negative tests.
- **S9-6 Deals Contracts and events**: `DealCreated`, `DealStageChanged`, `DealWon`, `DealLost`; Activities timeline subscribes.

## S10 — Deal roles and conflict flags (E11), 19 – 30 Apr

- **S10-1 Deal participants with roles**: each deal links several organisations and people with a role (customer, prime, subcontractor, teaming partner, competitor, university, supplier, funder). Tests first: same organisation can hold different roles in different deals; role uniqueness rules per deal; history retained.
- **S10-2 Role catalogue** managed in Config, with flags (e.g. `IsCompetitor`, `RequiresProbityCheck`).
- **S10-3 Organisation view of deals** by role (read model through Contracts, no joins).
- **S10-4 Conflict flag service** (stub for Probity in R3): detect an organisation that is customer in one live deal and competitor in another, or an organisation with a declared conflict record; show warnings on the deal.
- **S10-5 UI**: participants panel, add-participant dialog with role, warnings banner.

## S11 — Bid management and notifications (E12, E14), 3 – 14 May

- **S11-1 Bid workspace**: bid/no-bid decision record, bid manager, key dates (RFx release, questions close, submission, award), checklist templates, go/no-go approvers. Tests first: decision requires configured approvers; dates validation.
- **S11-2 Bid documents links** (URLs to SharePoint; no file storage in the CRM): validate allowed domains; no content fetched.
- **S11-3 Notifications**: in-app and email via Microsoft Graph or SMTP relay; per-user preferences; digest. Add `mailpit` to the devcontainer for tests. Tests first: no duplicates on retries, preferences respected, links never include sensitive content.
- **S11-4 Reminders and escalations** from task due dates and bid milestones using the platform scheduler (leased single runner).
- **S11-5 Calendar view of bid milestones** (read-only).

## S12 — Teams, permissions and hardening (E13), 17 – 28 May

- **S12-1 Teams and membership** administration UI; team-based visibility for deals and directory records.
- **S12-2 Record sharing**: share a record with a user or team, with expiry; audit.
- **S12-3 Permission matrix tests**: a generated test suite iterating roles × resources × actions verifies the documented matrix (`docs/permissions-matrix.md`). Tests first: matrix file is the single source; adding a role without matrix entries fails.
- **S12-4 Field-level restrictions** for sensitive properties (value, margin) by role.
- **S12-5 Performance and hardening for R2**: indexes, N+1 checks, pen-test retest on new endpoints, evidence refresh.

## R2 release gate

Permission matrix tests green; no cross-module join detected; pen-test delta clean; backup/restore drill repeated; evidence exported to `docs/evidence/r2/`.
