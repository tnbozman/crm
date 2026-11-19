# Compliance matrix

Working mapping of controls to where this plan addresses them. **Not an assessment.** Essential Eight Maturity Level 3 applies to the whole environment, not only this application, and control wording must be checked against the current official ASD publications and the NIST SP 800-171 revision your contracts reference (Rev 2 today; DFARS/CMMC references have lagged Rev 3). ASD announced the Essential Eight is moving to a successor "Essentials" series; revisit in S30.

Statuses: ☐ planned, ◐ in progress, ● implemented with evidence. Update this file in each sprint and link evidence in `docs/evidence/`.

## Essential Eight (application and platform view)

| Strategy | What applies to the CRM | Sprint | Evidence |
| --- | --- | --- | --- |
| Application control | Only the deployed signed build runs; App Service PaaS; pipeline-only deployment; no arbitrary code upload | S0, S3 | ☐ |
| Patch applications | Dependency updates (NuGet, npm, Actions, runtime stack); critical fixes within the policy window; vulnerability gates | S0, S3, ongoing | ◐ [S0-2](evidence/s00/S0-2-build-baseline.md), [S0-5](evidence/s00/S0-5-vue-app.md), [S0-6](evidence/s00/S0-6-ui-foundation.md) |
| Configure Microsoft Office macro settings | Not applicable to the CRM; CSV export formula-injection protection | S7 | ☐ |
| User application hardening | Strict CSP and security headers; no unsupported browsers beyond managed devices | S1 | ☐ |
| Restrict administrative privileges | PIM just-in-time admin; separate runtime/deployment/audit identities; break-glass; admin role in CRM limited and audited | S1, S2, S3, S12 | ☐ |
| Patch operating systems | Inherited from Azure PaaS; track runtime stack versions; devcontainer and runner images refreshed | S3 | ☐ |
| Multi-factor authentication | Entra Conditional Access, phishing-resistant authentication strength for all users including admins and data repository access | S3 | ☐ |
| Regular backups | Point-in-time restore, long-term retention, immutable copies, restore drills, break-glass protections on backups | S3, S8, S24, S29 | ☐ |

## NIST SP 800-171 Rev 2 families

| Family | Key CRM and platform measures | Sprint |
| --- | --- | --- |
| 3.1 Access control | Entra only, roles, teams, business-unit filters, least privilege, session limits, private endpoints, permission matrix tests | S1, S3, S12, S16 |
| 3.2 Awareness and training | Operations runbook, user guidance (organisation-level), AI usage guidance | S8, S28 |
| 3.3 Audit and accountability | Audit module, correlation IDs, central logs, retention, hash chain, alerting | S2, S3 |
| 3.4 Configuration management | Infrastructure as code, baselines, pipeline-only change, SBOM, config export | S0, S3, S29 |
| 3.5 Identification and authentication | Entra ID, phishing-resistant MFA, no local passwords, service identities by managed identity or app registration | S1, S3, S18 |
| 3.6 Incident response | Alerts, runbooks, incident procedure, tabletop test | S3, S8, S30 |
| 3.7 Maintenance | PaaS inherited, maintenance windows, patch SLA | S3 |
| 3.8 Media protection | Backups encrypted and immutable, export controls, no removable media use | S3, S7 |
| 3.9 Personnel security | Onboarding and offboarding runbook, access reviews | S8, S29 |
| 3.10 Physical protection | Inherited from Azure; evidence from provider attestations | S3 |
| 3.11 Risk assessment | Vulnerability scanning, dependency review, risk register | S0, S3, S30 |
| 3.12 Security assessment | System security plan, POA&M, pen tests, independent review | S3, S8, S30 |
| 3.13 System and communications protection | TLS 1.2+, WAF, private endpoints, TDE with customer-managed keys where required, session protection, AI provider data boundary | S1, S3, S25 |
| 3.14 System and information integrity | Flaw remediation, malware protection at platform level, monitoring, input validation, dependency scanning | S0, S2, S3 |

## Story-level pointers

| Control theme | Stories |
| --- | --- |
| No local passwords | S1-6, S1-7 |
| Insert-only audit | S2-1, S2-2, S3-2 |
| Log scrubbing | S2-4 |
| Phishing-resistant MFA and Conditional Access | S3-5 |
| Central logging and alerts | S3-6 |
| Patch and vulnerability SLA | S0-10, S3-7 |
| Backup immutability and restore | S3-8, S8-1 |
| Penetration testing | S8-3, S12-5, S16-4, S30-1 |
| Business-unit isolation | S16-2, S16-3 |
| AI controls | S25–S28 |
| Evidence and assurance | S3-9, S29-2, S30-1 |

## Epic coverage

Every epic E1–E46 from the roadmap must appear in a sprint. Epics currently mapped: E1 (S0, S3), E2 (S1, S3), E3 (S2), E4 (S0), E5 (S5), E6 (S4), E7 (S6), E8 (S6), E9 (S7), E10 (S9), E11 (S10), E12 (S11), E13 (S12), E14 (S11), E15 (S13), E16 (S14), E17 (S14), E18 (S15), E19 (S15, S16), E20 (S16), E21 (S17), E22 (S18), E23 (S19), E24 (S20), E25 (S20), E26 (S21), E27 (S22), E29 (S23), E30 (S23), E34 (S29), E35 (S3), E36 (S1, S3), E37 (S2, S3), E38 (S3, S8), E39 (S0, S8), E40 (S3, S8, S30), E41 (S29, S30), E42 (S25), E43 (S26), E44 (S27), E45 (S28), E46 (S0, S1, S2). Removed from scope: E28, E31, E32, E33. If your roadmap numbering differs, reconcile before S4.
