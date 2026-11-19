# Sprint 3 — Azure hosting and the R0 compliance gate (11 – 22 Jan 2027)

**Release**: R0. **Epics**: E1 (Azure platform), E2 (Conditional Access, PIM), E35 (patching), E36, E37 (monitoring, detection), E40 (assurance evidence). This sprint is mostly infrastructure, pipeline and configuration. TDD applies through Bicep validation, policy-as-code tests, smoke tests after deployment and scripted evidence checks.

## Goal

The application deploys to a non-production Azure environment through a pipeline using OIDC federation and no stored credentials. Controls required before any real data enters the CRM are in place and verified. The **R0 compliance gate** passes.

## Demo

1. Merge to `main` → pipeline builds, tests, scans, deploys to `test`, runs smoke tests.
2. Sign in to the deployed app with a real Entra account that satisfies Conditional Access (phishing-resistant MFA).
3. Show: private endpoint to SQL, Entra-only SQL auth, managed identity, TDE, WAF in front, logs in Log Analytics, an alert firing from a test event.
4. Restore the database from backup into a scratch server and run the smoke tests against it.
5. Walk through the gate checklist below with evidence links.

## Entry criteria

- S2 merged.
- Azure subscription(s) and resource groups agreed (separate `test` and `prod` subscriptions recommended).
- Entra permissions to create app registrations, app roles, Conditional Access policies and PIM assignments (or an admin available for those steps).
- Decision on Azure region (Australian region) and on the AI provider region (ADR-008).

## Stories

### S3-1 Infrastructure as code (E1)

Tests first (`infra/tests`, run in `make verify` and CI):
- `bicep build` and `bicep lint` pass for all templates.
- `what-if` against an empty scratch resource group produces the expected resource list (script compares with a committed expectation).
- Policy-as-code checks (PSRule for Azure, Checkov, or similar) fail on: public network access enabled for SQL, HTTP allowed, TLS below 1.2, missing diagnostic settings, missing tags.

Implement `infra/` Bicep: resource group modules, App Service plan and Web App (Linux or Windows, Always On, health check path, HTTPS only, minimum TLS 1.2, FTP disabled, system-assigned managed identity), Azure SQL server and database (Entra-only authentication, TDE, auditing to Log Analytics, long-term retention), Key Vault (RBAC, purge protection), VNet with private endpoints, private DNS, Front Door or Application Gateway with WAF, Log Analytics workspace, Application Insights, storage for immutable backups and evidence, budgets and tags.

### S3-2 Database access model (E1, E36)

Tests first (post-deploy script `scripts/checks/sql-permissions.sh`):
- Runtime managed identity can read and write module schemas but cannot `CREATE TABLE`, `ALTER`, or access another module's schema it should not.
- Runtime identity can only `INSERT` into `audit`.
- Deployment identity can migrate; it is not used by the app.
- No SQL logins with passwords exist.

Implement `infra/sql/permissions.sql` generated per module list, run by `Crm.Migrator` with `--grants`.

### S3-3 Pipeline with OIDC and environments (E4, E1)

Tests first: workflow tests assert no `secrets.*` containing long-lived Azure credentials are referenced, actions are pinned by SHA, environments `test` and `prod` require approvals for prod, and the deploy job uses `azure/login` with federated credentials.

Implement: GitHub Actions deploy workflow: build, `make verify`, SBOM, container or zip artifact, run `Crm.Migrator` against the target (deployment identity), deploy to App Service using a slot, run smoke tests, swap. Rollback documented and tested once (swap back).

### S3-4 Smoke and post-deploy tests (E4)

Tests first: after deploy, `scripts/smoke.sh`: `/api/v1/health` returns 200; `/` returns the SPA; unauthenticated `/api/v1/access/me` returns 401 (redirect or 401 as designed); security headers present; outbox dispatcher heartbeat visible.

### S3-5 Entra configuration as code and Conditional Access (E2, E36)

Tests first: a script (`scripts/checks/entra-baseline.ps1` or Graph via `az rest`) asserts: the CRM app registration has the expected app roles and redirect URIs only, user assignment required, a Conditional Access policy targets the app with an authentication strength of phishing-resistant MFA, a compliant-device requirement (or document the exception), break-glass accounts excluded and monitored, PIM used for admin roles.

Implement: app registration (Bicep Graph extension or Terraform/Microsoft Graph scripts), app roles, groups to roles assignment, Conditional Access policy in report-only then enforced, PIM eligible assignments for Azure and Entra admin roles, break-glass procedure doc.

### S3-6 Central logging, alerts and detection (E37)

Tests first: a test script emits a synthetic event (failed sign-in, role change, outbox backlog) and asserts the corresponding Log Analytics query returns it within a minute and that an alert rule exists with an action group.

Implement: diagnostic settings for every resource, retention at least 12 months (check the requirement), Sentinel or Defender for Cloud onboarding if licensed, alert rules (auth failures, privilege changes, WAF blocks, SQL anomalies, outbox lag, 5xx rate), action group to the security mailbox and chat, KQL stored in `infra/kql/` with tests.

### S3-7 Patching and vulnerability response (E35)

Tests first: scheduled workflow `dependency-review.yml` fails on critical or high vulnerabilities older than the policy window (48 hours for critical on internet-facing components; confirm the exact policy wording in the compliance review). Verify with a seeded vulnerable package on a branch.

Implement: Dependabot auto-PRs, weekly rebuild of the base image or runtime stack version check, runtime version tracking (App Service stack version reported and alerted when behind), Defender for Cloud recommendations export, documented patch SLA.

### S3-8 Backup, restore and immutability (E38 groundwork, E1)

Tests first: `scripts/checks/restore-drill.sh` restores the latest backup to a scratch database, runs the smoke tests, records duration and result into `docs/evidence/`.

Implement: point-in-time restore configured, long-term retention, backup copies to immutable storage with a time-based retention lock (decide whether a locked policy is needed for ML3 and what the backup administrators can do), restore runbook.

### S3-9 Gate review and evidence pack (E40)

Tests first: `scripts/checks/gate.sh` evaluates the checklist automatically where possible and prints a pass/fail table; manual items require a file in `docs/evidence/s03/` with a date and approver.

Implement: evidence index, `docs/compliance-matrix.md` updated with real links, gate sign-off record.

## R0 compliance gate checklist

- [ ] Entra sign-in only; Conditional Access with phishing-resistant MFA enforced for all CRM users and admins
- [ ] PIM (just-in-time) for Azure and Entra administration; break-glass accounts tested and monitored
- [ ] SQL and Key Vault reachable only through private endpoints; no public access
- [ ] Runtime and deployment identities separated; no passwords or long-lived secrets in the pipeline or app settings
- [ ] Logs from app, SQL, Entra and platform flowing to the central workspace with retention set; alert fired in test
- [ ] Backup restore drill passed; immutable backup copy exists
- [ ] Dependency, secret and static scans in the pipeline with no unresolved critical findings
- [ ] Patch SLA documented and the runtime stack current
- [ ] Data classification note: CRM holds commercial data only; no classified or CUI; handling markers rule in place
- [ ] AI provider decision recorded (Azure OpenAI region; direct APIs development only)
- [ ] System security plan skeleton and plan of action and milestones created

## Definition of done

Gate checklist complete with evidence; `test` environment live; production environment provisioned but empty; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md, docs/compliance-matrix.md and docs/sprints/s03-azure-and-compliance-gate.md.
This sprint is infrastructure heavy. For each story propose the test or check that proves it before writing any Bicep or workflow.
Do not apply anything to Azure: produce files, scripts and a runbook, and list the commands I must run myself.
Flag anything that needs a tenant admin.
```

## Retrospective

_(append notes here)_
