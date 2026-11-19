# Sprint 30 — Independent review and handover (24 Jan – 4 Feb 2028)

**Release**: R6 AI assistant and assurance. **Epics**: E40, E41.

## Goal

An independent assessment confirms the system, the compliance mapping is current, and operations are handed over.

## Demo

1. Present the independent review findings and closure status.
2. Show the updated mapping to the current ASD guidance and NIST revision.
3. Run a tabletop incident exercise.
4. Hand over runbooks and the improvement backlog.

## Entry criteria

- S29 merged; `make verify` green on `main`.
- Assessor engaged; current ASD and DoD guidance checked at the time (the plan's mapping is a starting point only).

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S30-1 Independent assessment

*As the security officer I have external assurance.*

Tests first:
- Findings tracked as issues; each high finding gets a regression test or control check before fix
- Retest confirms closure

Implement: Support the assessor with evidence and access.

Done when: no open highs.

### S30-2 Compliance mapping refresh

*As the security officer I stay current.*

Tests first:
- `scripts/checks/matrix-links.sh` fails on matrix rows without evidence links
- Mapping reviewed against the current Essentials series and NIST 800-171 revision

Implement: Update `docs/compliance-matrix.md` and the system security plan.

Done when: matrix and plan current.

### S30-3 Tabletop exercise

*As the team we practise.*

Tests first:
- Scenario script and observer checklist
- Actions logged with owners

Implement: Exercise run and report.

Done when: report saved.

### S30-4 Handover and closeout

*As the organisation we own the system.*

Tests first:
- Runbook walkthrough by someone new to the system
- Module boundary review: candidates for extraction recorded
- Final retrospective and decision-log review

Implement: Handover pack, backlog, lessons learned.

Done when: handover accepted.

## Compliance evidence

Independent assessment report, closure evidence, tabletop report.

## Release gate

- [ ] AI red-team suite green
- [ ] No write executed without approval in any test
- [ ] Independent assessment highs closed
- [ ] Evidence exported to docs/evidence/r6/

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s30/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s30-independent-review-and-handover.md.
Check every entry criterion for sprint 30 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S30-1, S30-2, S30-3, S30-4.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
