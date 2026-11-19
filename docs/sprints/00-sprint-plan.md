# Sprint plan

Two-week sprints from Monday 30 November 2026. Dates are indicative. Each release ends with a demo and a compliance gate re-check.

| Sprint | Dates | Release | Theme | Epics | File |
| --- | --- | --- | --- | --- | --- |
| S0 | 30 Nov – 11 Dec 2026 | R0 | Walking skeleton: devcontainer, make, frontend in the .NET build, CI, test harness | E1, E4, E39, E46 | `s00-walking-skeleton.md` |
| S1 | 14 – 25 Dec 2026 | R0 | Platform kernel, module template, Access module (Entra only) | E2, E36, E46 | `s01-kernel-and-access.md` |
| S2 | 28 Dec 2026 – 8 Jan 2027 | R0 | Audit module, API baseline, logging, AI provider spike | E3, E37, E46 | `s02-audit-and-api-baseline.md` |
| S3 | 11 – 22 Jan 2027 | R0 | Azure hosting, pipeline, compliance gate | E1, E2, E35, E36, E37, E40 | `s03-azure-and-compliance-gate.md` |
| S4 | 25 Jan – 5 Feb | R1 | Config module, app shell | E6 | `r1-core-crm.md` |
| S5 | 8 – 19 Feb | R1 | Directory: organisations and people | E5 | `r1-core-crm.md` |
| S6 | 22 Feb – 5 Mar | R1 | Relationships, notes, tasks, timeline | E7, E8 | `r1-core-crm.md` |
| S7 | 8 – 19 Mar | R1 | Search, saved views, CSV import | E9 | `r1-core-crm.md` |
| S8 | 22 Mar – 2 Apr | R1 | Hardening, backup and restore, pen test, go-live | E38, E39, E40 | `r1-core-crm.md` |
| S9 | 5 – 16 Apr | R2 | Deals and pipelines | E10 | `r2-deals.md` |
| S10 | 19 – 30 Apr | R2 | Deal roles and conflict flags | E11 | `r2-deals.md` |
| S11 | 3 – 14 May | R2 | Bid management, notifications | E12, E14 | `r2-deals.md` |
| S12 | 17 – 28 May | R2 | Teams, permissions, R2 hardening | E13 | `r2-deals.md` |
| S13 | 31 May – 11 Jun | R3 | Product catalogue and price books | E15 | `r3-product-research.md` |
| S14 | 14 – 25 Jun | R3 | Quotes, market eligibility | E16, E17 | `r3-product-research.md` |
| S15 | 28 Jun – 9 Jul | R3 | Programmes and grants, probity register | E18, E19 | `r3-product-research.md` |
| S16 | 12 – 23 Jul | R3 | Probity gate, business-unit separation, R3 hardening | E19, E20 | `r3-product-research.md` |
| S17 | 26 Jul – 6 Aug | R4 | Rules engine | E21 | `r4-automation.md` |
| S18 | 9 – 20 Aug | R4 | Webhooks and service identities | E22 | `r4-automation.md` |
| S19 | 23 Aug – 3 Sep | R4 | Microsoft 365 integration | E23 | `r4-automation.md` |
| S20 | 6 – 17 Sep | R4 | Finance and delivery hand-off, bulk operations | E24, E25 | `r4-automation.md` |
| S21 | 20 Sep – 1 Oct | R5 | Dashboards | E26 | `r5-reporting.md` |
| S22 | 4 – 15 Oct | R5 | Forecasting and targets | E27 | `r5-reporting.md` |
| S23 | 18 – 29 Oct | R5 | Data quality, scheduled reports | E29, E30 | `r5-reporting.md` |
| S24 | 1 – 12 Nov | R5 | Performance and hardening | NFRs | `r5-reporting.md` |
| S25 | 15 – 26 Nov | R6 | Assistant foundation | E42 | `r6-ai-assurance.md` |
| S26 | 29 Nov – 10 Dec | R6 | Query skills | E43 | `r6-ai-assurance.md` |
| S27 | 13 – 24 Dec | R6 | Manage skills with approval | E44 | `r6-ai-assurance.md` |
| S28 | 27 Dec 2027 – 7 Jan 2028 | R6 | AI safety and quality | E45 | `r6-ai-assurance.md` |
| S29 | 10 – 21 Jan 2028 | R6 | Configuration as code, annual assurance | E34, E41 | `r6-ai-assurance.md` |
| S30 | 24 Jan – 4 Feb 2028 | R6 | Independent review, Essentials mapping, handover | E40, E41 | `r6-ai-assurance.md` |

Removed from scope (internal-only decision): E28 relationship intelligence, E31 AI assistance as first drafted (replaced by E42–E45), E32 extension points, E33 partner access.

## Gates

- **R0 compliance gate (end of S3)**: no real data until Conditional Access with phishing-resistant MFA is enforced, private endpoints are in place, logs reach the central workspace and an alert fires in test, a restore from immutable backup succeeds, dependency and secret scans are clean of critical findings, and break-glass accounts are tested.
- **R1 go-live (end of S8)**: penetration test findings of high severity closed, restore exercise recorded, data migration rehearsed twice, business owner sign-off.
- **Each later release**: permission matrix tests green, evidence exported, no critical vulnerabilities open.

## Sprint ceremonies

- Day 1: kickoff prompt, confirm entry criteria, order stories.
- Daily: one story at a time with Claude; review every commit.
- Day 9: sprint review prompt, fix gaps.
- Day 10: demo script, retrospective notes appended to the sprint file, refine prompt for the next sprint.
