# Sprint 25 — Assistant foundation (15 – 26 Nov 2027)

**Release**: R6 AI assistant and assurance. **Epics**: E42.

## Goal

Principles: the assistant runs in-process as the signed-in user and can do nothing the user cannot already do in the UI; tools are typed C# functions calling module Contracts (no free SQL, HTTP, shell or script execution); writes are proposals the user approves; handling-marked and out-of-scope data never reach the model; production uses Azure OpenAI in an Australian region unless ADR-008 is amended; prompts, tool calls and approvals are audited. This sprint builds the host, skill loading, tool registry and chat UI.

## Demo

1. Ask the assistant a question; a scripted fake model calls a tool and answers.
2. A ReadOnly user's agent cannot reach a write tool.
3. Hostile text in a note does not change tool behaviour.
4. Show skills loaded for a user's roles.

## Entry criteria

- S24 merged; `make verify` green on `main`.
- R5 released; AI provider configuration approved (ADR-008) and Azure OpenAI deployment available in test.
- Spike code from S2-7 reviewed.

## Stories

Common rules (see `CLAUDE.md`): tests first; each feature is built as Controller → Service → EF Model with separate request/response DTOs and explicit mappers (words like "aggregate" or "endpoint" below mean the Model plus its service and controller); module boundary rules, audit events on data change, positive and negative permission tests, OpenAPI and generated client regenerated, `make verify` green.

### S25-1 Assistant module and storage

*As the system I keep conversations and approvals.*

Tests first:
- `Conversation_BelongsToUser_NotReadableByOthers`
- `Retention_DeletesOldConversations`
- `ToolCallLog_StoresNameArgsHashAndResultSummary`
- Migrations tests

Implement: `Crm.Assistant` (schema `assistant`).

Done when: tested.

### S25-2 Agent host

*As a user I chat with an agent.*

Tests first:
- `Agent_WithFakeChatClient_CallsToolThenAnswers`
- `Streaming_EmitsEventsInOrder`
- `PerUserConcurrency_AndTokenLimits_Enforced`
- `ProviderOutage_ReturnsProblemDetails`

Implement: Microsoft Agent Framework agent over `IChatClient` (factory from S2-7), SSE endpoint.

Done when: tests with fake client.

### S25-3 Skills loading

*As a developer I add capabilities as skills.*

Tests first:
- `SkillsLoaded_ByUserRole`
- `InvalidFrontmatter_FailsStartup`
- `ScriptTool_NeverRegistered`
- `SkillResources_ReadOnly`

Implement: `SKILL.md` per module loaded through the framework's skills provider; script execution disabled.

Done when: tests green.

### S25-4 Tool registry with user context

*As the security officer I know tools respect permissions.*

Tests first:
- `ReadOnlyUser_CannotInvokeWriteTool_EvenIfModelRequests`
- `Tool_ResolvesCurrentUser_NotModelSuppliedIdentity`
- `ToolArguments_ValidatedLikeApiInput`
- Architecture: tools live in module projects and call only Contracts

Implement: Tools registered via `IModule`; each declares required role and `writes: true|false`.

Done when: tested.

### S25-5 Chat UI

*As a user I use the assistant in the app.*

Tests first:
- Component tests: streaming render, stop button, record links, error state
- axe checks
- Playwright with fake model

Implement: Side panel `features/assistant/`.

Done when: journey green.

### S25-6 Prompt injection baseline

*As the security officer I test hostile content.*

Tests first:
- `NoteWithInstructions_DoesNotChangeToolSelection` (scripted fake model gets record text as data)
- `RecordText_IsWrappedAsUntrustedData`
- `ToolResults_ContainNoSystemInstructions`

Implement: Wrapping and delimiting of record content; tests with hostile corpus.

Done when: baseline tests green.

## Compliance evidence

AI design note, data boundary decision, injection test corpus. NIST 3.1.x, 3.13.x.

## Definition of done

All stories ticked; demo performed; `make verify` green on `main`; permission tests (including negative cases) present for every new endpoint and tool; evidence saved under `docs/evidence/s25/`; sprint plan and compliance matrix updated; retrospective appended.

## Claude kickoff prompt

```
Read CLAUDE.md, docs/architecture.md and docs/sprints/s25-assistant-foundation.md.
Check every entry criterion for sprint 25 and report which are not met.
If any story lacks implementation detail that the current code now makes possible, propose edits to this file first and wait for my approval.
Order: S25-1, S25-2, S25-3, S25-4, S25-6, S25-5.
Then start the first story with strict TDD: restate it, list the first three tests, write the first failing test and run it.
```

## Retrospective

_(append notes here)_
