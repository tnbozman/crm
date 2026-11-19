# S0-6 evidence: OpenVue spike and UI foundation

Date: 2026-11-19 (UTC). Commits `f429429`..HEAD on `main`. Decision: [ADR-001](../../adr/001-ui-library.md).

## Controls supported

| Control | What this story provides |
| --- | --- |
| E8 patch applications / NIST 800-171 3.14.1, E39 supply chain | Due diligence on the UI dependency: licence, maintainers, dependency tree, install scripts and provenance, recorded in ADR-001. Exact pins with the release-age policy. Import boundary enforced by ESLint. |
| NIST 800-171 3.4.1 / 3.4.2 (baseline configuration) | One documented way to build UI (wrappers in `src/ui`, unstyled mode, Tailwind pass-through), enforced by lint. |
| Accessibility (WCAG via axe) | Axe checks on the app shell and on the sample page, with the dialog closed and open. |

## Spike checks performed

- `pnpm view openvue` and `@openvue/core`, `@openvue/icons`, `@openuxkit/*`: versions, MIT licence, maintainers (2), repository `openvi-foundation`, no npm provenance.
- Installed `openvue@1.1.1`: `LICENSE.md` is MIT with PrimeTek and Openvi copyright. No install scripts in any OpenVue or `@openuxkit` package.
- `pnpm build`: the production CSS contains the pass-through classes (`bg-blue-700`, `border-slate-300`, `divide-slate-200`, `bg-slate-900/50`, `max-h-[90vh]`, `focus:not-sr-only`) and no `.p-button`, `.p-dialog` or `.p-datatable` rules. The dev-only sample page is not in the bundle.

## Tests (Vitest + Testing Library + MSW), each seen failing first

| Test | Proves |
| --- | --- |
| `AppButton › renders an unstyled OpenVue button styled by Tailwind pass-through classes` | `data-pc-name="button"`, our classes, no `p-*` |
| `AppInput › renders an unstyled OpenVue input with a visible label and Tailwind pass-through classes` | label association, our classes, no `p-*` |
| `AppInput › emits the typed value for v-model` | `v-model` works |
| `AppDialog › renders an open, labelled, unstyled OpenVue dialog with Tailwind pass-through classes` | `role="dialog"` named by its header, our classes, no `p-*` |
| `AppDialog › asks to close when the close button is pressed` | `update:visible` is `false` |
| `AppDataTable › renders column headers and one row per item from an unstyled OpenVue table` | headers and rows from typed columns |
| `AppDataTable › styles the table through Tailwind pass-through classes only` | our classes, no `p-*` |
| `App › renders the page inside the layout shell with a banner, skip link and main landmark` | `AppLayout` |
| `UI sample page › shows every UI wrapper and has no axe violations` | story test: axe passes for the sample page |
| `UI sample page › has no axe violations with the dialog open` | axe on `document.body`, including the teleported dialog |

Planted and removed: a direct `openvue/button` import in a view. `pnpm lint` passed (exit 0) before the `no-restricted-imports` rule and failed (exit 1) after it.

## Open item

- The in-house PrimeVue fork (the fallback) has no recorded location or package name yet. Record it in ADR-001 when known.
