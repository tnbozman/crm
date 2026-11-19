# CRM implementation pack

A set of markdown files for building the CRM incrementally with Claude, test first, in two-week sprints. Copy `CLAUDE.md` and `docs/` into the root of the new repository.

## Contents

| File | Purpose |
| --- | --- |
| `CLAUDE.md` | Standing instructions Claude reads every session: TDD rules, architecture and security rules, commands |
| `docs/architecture.md` | Modular monolith design, communication, identity, AI, testing layers, decisions to confirm |
| `docs/environment.md` | Devcontainer, Makefile, frontend-in-dotnet-build, VS Code configuration (specification for Sprint 0) |
| `docs/compliance-matrix.md` | Essential Eight ML3 and NIST 800-171 controls mapped to sprints and stories |
| `docs/sprints/00-sprint-plan.md` | All 31 sprints, dates, releases and epic coverage |
| `docs/sprints/s00-…` to `s30-…` | One file per sprint (31 in total), each with goal, demo, entry criteria, stories with tests-first lists, evidence and a kickoff prompt |

## How to work

1. Open the repo in the devcontainer. Start Claude Code (or your Claude client) in the repo root so `CLAUDE.md` loads.
2. Pick the next unticked story in the current sprint file. Paste the **story prompt** below.
3. Let Claude write the failing test, run it, make it pass, refactor, commit. Review each commit.
4. When all stories are ticked, run the **sprint review prompt**, then the sprint's *Demo* script yourself.
5. Before starting each sprint, run the **refresh prompt** so the file is adjusted to what the code now looks like.

Keep one story per Claude session or per conversation segment. Small, reviewed commits beat large generated changes.

## Prompts

**Story prompt**

```
Read CLAUDE.md and docs/sprints/<file>.md. Implement story <ID> using strict TDD.
Start by restating the story and listing the tests you will write first.
Write the first failing test and run it before any production code.
Work in small red-green-refactor steps, commit after each green step, and finish with make verify.
Tick the story in the sprint file and add the evidence notes. Do not start another story.
```

**Sprint kickoff prompt**

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/<file>.md.
Check every entry criterion for sprint <N>. Report which are met and which are not.
Propose the order of stories and any risks. Do not write code yet.
```

**Sprint review prompt**

```
Review the work for sprint <N> against docs/sprints/<file>.md and docs/compliance-matrix.md.
List: stories not ticked, Done-when items not evidenced, tests that are skipped or weak, any module boundary or
security rule from CLAUDE.md that was bent, and any follow-ups. Run make verify and report the result.
```

**Refresh prompt (before each sprint)**

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/<file>.md.
Compare the stories with the code and ADRs as they now exist. Propose edits to the sprint file only where
reality has changed (renamed types, decisions made, stories already done, new risks). Show me the diff; do not implement.
```

## Assumptions and flags

- **OpenVue** is treated as the PrimeVue fork you named; I could not verify its package name or whether its unstyled mode works with Tailwind, so Sprint 0 has a time-boxed spike (ADR-001) with the in-house PrimeVue fork as fallback.
- **ASP.NET Core Identity** is used in external-login-only mode so it does not conflict with the "Entra ID only, no local accounts" compliance requirement. A test enforces it (S1).
- **Claude and OpenAI APIs** are supported behind `IChatClient`. For Essential Eight and 800-171 purposes the production default is Azure OpenAI in an Australian region; direct provider APIs are development-only with synthetic data until you approve them (ADR-008).
- **pnpm**, **xUnit v3**, SQL `LIKE` search for the MVP and the `Crm.Migrator` approach to migrations are defaults you can change through ADRs.
- Sprint dates assume a start on Monday 30 November 2026 and no public-holiday adjustment. S1 and S2 span Christmas and New Year; plan for reduced capacity.
- Package versions are deliberately not pinned in these files. Claude must query the feed and use current stable releases.
