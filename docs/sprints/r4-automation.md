# R4 — Automation and integration (S17–S20, 26 Jul – 17 Sep 2027)

**Outcome**: routine work is automated by configurable rules, other internal systems can be notified or can call the CRM safely, Microsoft 365 data enriches the timeline, and won deals hand off to finance and delivery.

Use the **refine prompt** before each sprint. Common rules from `CLAUDE.md` apply.

## S17 — Rules engine (E21), 26 Jul – 6 Aug

- **S17-1 Rule model** (Automation module): trigger (integration event type + filter), conditions (field comparisons, role, value thresholds), actions (create task, send notification, set field, request approval, call webhook). Stored as versioned JSON; no scripting language.
- **S17-2 Rule evaluation engine**: deterministic, idempotent per (rule, event) via inbox; loop protection (max chain depth, action cause tracking). Tests first: duplicates, ordering, loops, failing action retried and dead-lettered.
- **S17-3 Dry-run and simulation**: test a rule against a historical event without side effects.
- **S17-4 Admin UI**: rule list, editor with validation, run history, enable/disable with audit.
- **S17-5 Permissions**: only Admin edits rules; actions run as a *service principal for automation* with limited rights and always audited with the rule ID.

## S18 — Webhooks and service identities (E22), 9 – 20 Aug

- **S18-1 Outbound webhooks** from events: signed (HMAC with Key Vault secret), retries with backoff, allow-listed destination hosts, payload minimisation. Tests first: signature verification, replay protection headers, SSRF protection (block private ranges and metadata endpoints).
- **S18-2 Inbound API access for services** through Entra app registrations or managed identities with app roles and JWT bearer validation; no API keys. Tests first: wrong audience, expired token, missing role, user token on service endpoint.
- **S18-3 Service identity administration**: list, scopes, last used, rotate or disable; alerting on unused or over-privileged identities.
- **S18-4 Rate limits and quotas** per identity.
- **S18-5 API documentation page** for internal consumers generated from OpenAPI.

## S19 — Microsoft 365 integration (E23), 23 Aug – 3 Sep

- **S19-1 Graph connector design** (ADR): application permissions versus delegated; prefer delegated user context or minimal application permissions with scoped access policies; confirm tenant policy.
- **S19-2 Meeting and email activity capture**: matched by participant email to people, creates timeline entries with metadata only (subject, time, participants), never body content by default. Tests first with a fake Graph server: mapping, duplicates, user opt-out, deleted items.
- **S19-3 Calendar events from deal and bid milestones** (optional per user).
- **S19-4 Teams or email notifications** for rule actions.
- **S19-5 Failure handling**: throttling (429) with backoff, consent errors surfaced to admins, monitoring metrics.

## S20 — Finance and delivery hand-off, bulk operations (E24, E25), 6 – 17 Sep

- **S20-1 Won-deal hand-off package**: on `DealWon`, produce a hand-off record (customer, scope summary, contract value, key dates, billing contacts, PO reference) with a checklist; export a file or call the finance interface defined by an adapter (Sage Intacct or CSV) behind an interface so it can change. Tests first with a fake adapter: idempotent, retries, partial failure visible.
- **S20-2 Delivery project stub**: create a delivery record reference for the delivery team (link to Jira or project tool by URL/ID only).
- **S20-3 Bulk operations**: bulk reassign owner, bulk update field, bulk archive with preview, limits, background execution, per-record result report, full audit. Tests first for permission per record and rollback of failed batches.
- **S20-4 R4 hardening**: pen-test delta on webhooks and service identities, evidence refresh.

## R4 release gate

No service identity with standing broad permissions; webhook SSRF tests green; automation actions audited with rule IDs; evidence in `docs/evidence/r4/`.
