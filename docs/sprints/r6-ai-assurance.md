# R6 — AI assistant and assurance (S25–S30, 15 Nov 2027 – 4 Feb 2028)

**Outcome**: staff can query and manage the CRM in natural language using their own credentials, through reviewed skills and typed tools, with approvals for changes and full audit. The system closes with independent assurance and a mapping to the successor to Essential Eight.

Use the **refine prompt** before each sprint. Common rules from `CLAUDE.md` apply. The AI design is in `docs/architecture.md` §8.

## Principles (apply to every R6 story)

- The assistant runs in-process as the signed-in user. It can do nothing the user cannot already do in the UI.
- Tools are typed C# functions calling module Contracts. No free SQL, no arbitrary HTTP, no shell, no script execution in skills.
- Reads are free; writes produce a **proposal** the user approves in the UI before execution.
- Handling-marked properties and business-unit-restricted records never reach the model for users without access (tests in every sprint).
- Production uses Azure OpenAI in an Australian region by default; direct Claude/OpenAI APIs are blocked in Production unless explicitly approved (ADR-008).
- Prompts, tool calls, approvals and results are audited with correlation IDs.

## S25 — Assistant foundation (E42), 15 – 26 Nov

- **S25-1 Assistant module**: conversations, messages, tool-call log, approvals table; retention policy.
- **S25-2 Agent host**: Microsoft Agent Framework agent behind `IChatClient`; provider selection (S2-7); streaming responses over SSE; per-user concurrency and token limits. Tests first with the fake chat client: scripted tool call → tool executes → final answer.
- **S25-3 Skills loading**: `SKILL.md` files in each module's `Skills/` folder, loaded by role via the skills provider; startup validation of frontmatter; script execution disabled (test asserts the script tool is never registered).
- **S25-4 Tool registry and user context**: tools registered per module through `IModule`, each declares required role and whether it writes; tool invocation resolves `ICurrentUser` and enforces the same policies as the API. Tests first: a ReadOnly user's agent cannot call a write tool even if the model asks.
- **S25-5 Chat UI**: side panel, streaming, citations to records (links), stop button, clear conversation.
- **S25-6 Prompt injection baseline**: record fields are passed to the model as data, tools never accept instructions from record content; tests with hostile record text.

## S26 — Query skills (E43), 29 Nov – 10 Dec

- **S26-1 Directory skill and tools**: find organisation, people, relationships.
- **S26-2 Deals skill and tools**: pipeline summary, my open deals, deal participants by role, upcoming milestones.
- **S26-3 Activities skill**: my tasks, recent notes, timeline summaries.
- **S26-4 Commerce and Programmes skills** (read-only).
- **S26-5 Reporting skill**: ask for dashboard figures and forecasts using the existing projections.
- **S26-6 Answer grounding**: every figure in an answer must come from a tool result; tests with scripted model replies check that the UI shows the tool results alongside the answer.
- **S26-7 Visibility tests**: for each tool, positive and negative tests per role and business unit.

## S27 — Manage skills with approval (E44), 13 – 24 Dec

- **S27-1 Proposal model**: a write tool returns a structured proposal (action, target IDs, before/after preview); no side effects.
- **S27-2 Approval UI and API**: approve, reject, edit; approval is bound to the user, the proposal hash and a short expiry. Tests first: replay, tampering with the proposal, approving someone else's proposal.
- **S27-3 Write tools**: create note, create task, update deal stage, add participant, log interaction (start narrow; add more only with tests).
- **S27-4 Bulk and destructive actions excluded** from the assistant (delete, merge, role changes, exports).
- **S27-5 Audit**: proposal, approval, execution and result are linked entries marked as assistant-originated.

## S28 — AI safety and quality (E45), 27 Dec 2027 – 7 Jan 2028

- **S28-1 Evaluation suite**: a golden set of questions with scripted fixtures run against the fake model for deterministic checks, and an optional live-model evaluation run (`make ai-eval`) tracked over time.
- **S28-2 Red-team tests**: prompt injection via notes, tool-argument fuzzing, privilege escalation attempts, data exfiltration attempts via links; all blocked and logged.
- **S28-3 Cost and rate controls**: per-user budgets, global kill switch, provider outage fallback behaviour.
- **S28-4 Model and prompt change control**: version skills and system prompt; evaluation required before release; record model identifiers in audit.
- **S28-5 User guidance**: what the assistant can see and do, how to report a bad answer.

## S29 — Configuration as code and annual assurance (E34, E41), 10 – 21 Jan 2028

- **S29-1 Configuration export/import** for Config, pipelines, rules and roles (JSON in Git) with diff, dry run and approval; environment promotion test → prod.
- **S29-2 Annual assurance pack**: scripts that gather evidence (access reviews, backup drills, patch status, pen-test status, audit sample) into `docs/evidence/annual/`.
- **S29-3 Access review workflow**: quarterly review of roles and service identities in the CRM with sign-off recorded.
- **S29-4 Retention and disposal**: configurable retention, archival, secure deletion procedures with audit.

## S30 — Independent review and handover (E40, E41), 24 Jan – 4 Feb 2028

- **S30-1 Independent security assessment** (system security plan review, pen test, E8 ML3 and NIST 800-171 control review); remediate findings.
- **S30-2 Mapping to ASD's Essentials series** and to NIST 800-171 Rev 3 differences; updated `docs/compliance-matrix.md`. Check current ASD and DoD guidance at the time; this plan's mapping is only a starting point.
- **S30-3 Operational handover**: runbooks, on-call, backlog of improvements, architecture review of module boundaries (candidates for extraction if ever needed).
- **S30-4 Closeout**: final retrospective, decision log review, lessons for next programme.

## R6 release gate

AI red-team suite green; no write executed without approval in any test; independent assessment findings (high) closed; evidence in `docs/evidence/r6/`.
