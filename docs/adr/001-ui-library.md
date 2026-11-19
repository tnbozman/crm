# ADR-001: UI library

- Status: Accepted (2026-11-19, S0-6 spike, within the half-day time-box)
- Supersedes: the default in `docs/architecture.md` §10

## Context

The plan names "OpenVue (PrimeVue fork), unstyled + Tailwind", with the in-house PrimeVue fork as the fallback. PrimeVue itself moved to 5.x under a non-MIT licence ("SEE LICENSE IN LICENSE.md", described as "premium"). OpenVue is a community continuation of the MIT-licensed PrimeVue 4.

## Spike findings

| Question | Finding |
| --- | --- |
| Package name and version | `openvue` 1.1.1 and `@openvue/core` 1.1.1 (published 2026-11-04). Components are imported per entry point: `openvue/button`, `openvue/inputtext`, `openvue/dialog`, `openvue/datatable`, `openvue/column`. Plugin: `openvue/config`. |
| Licence | MIT. `LICENSE.md` keeps "Copyright (c) 2018-2025 PrimeTek" and adds "Copyright (c) 2026 Openvi Foundation and contributors". |
| Dependencies | Only `@openvue/icons` and `@openuxkit/{utils,styled,styles}`, all MIT, from the same GitHub organisation (`openvi-foundation`). Optional peers `quill` and `chart.js` are not installed. No package has install scripts. |
| Unstyled mode with Tailwind | Works. `app.use(OpenVue, { unstyled: true })`, with styling through each component's `pt` prop. Rendered elements carry only our classes (no `p-*`). The production CSS contains every pass-through Tailwind class and no theme CSS. Tailwind 4 picks the classes up from the wrapper `.vue` files with no extra configuration. |
| Accessibility | Button, InputText (with our label), Dialog (`role="dialog"`, labelled by its header, close button named "Close") and DataTable (named by `aria-label`) pass axe in jsdom, including with the dialog open. |
| API compatibility | Matches the PrimeVue 4 API (`pt`, `unstyled`, `v-model:visible`, `tableProps`), so PrimeVue 4 documentation applies. |

## Decision

Adopt **OpenVue 1.1.1 in unstyled mode with Tailwind pass-through**.

- OpenVue is installed once, in `src/web/src/ui/openvue.ts`, with `unstyled: true` and no global theme.
- Only `src/web/src/ui/` imports OpenVue. ESLint `no-restricted-imports` enforces this; `main.ts` and the test helpers are the only exceptions. Features use `AppButton`, `AppInput`, `AppDialog`, `AppDataTable` and `AppLayout`, and new wrappers are added there.
- Each wrapper owns its Tailwind classes in its `pt` object. There are no CSS files besides the Tailwind entry.

## Risks and mitigations

| Risk | Mitigation |
| --- | --- |
| Young project: first release July 2026, 1.0 in September 2026 | The wrappers confine OpenVue to `src/ui` (lint-enforced). Switching to the in-house PrimeVue fork or another library means rewriting five small wrappers, not the app. |
| Two maintainers, publishing from personal accounts | Exact pins, committed lockfile, pnpm 24-hour release-age policy (ADR-002). Every OpenVue or `@openuxkit` bump gets a reviewed diff of the published package and is logged in ADR-010. |
| No npm provenance attestations | As above. S0-10 adds `pnpm audit` and an SBOM. S3 should consider mirroring through an internal registry. |
| Fork drifts from upstream fixes | Accept for now. Review at each release (R1 retrospective). |

## Fallback

Not needed after the spike. If OpenVue becomes unviable (unmaintained, licence change or a security issue), move `src/ui` to the in-house PrimeVue fork. Its location and package name still need to be recorded here.
