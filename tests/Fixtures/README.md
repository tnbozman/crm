# Architecture test fixtures

Two test-only modules that `Crm.Architecture.Tests` checks its module rules against (S1-1). They are never composed into the host.

- **Sample** follows every rule. Each rule must report nothing for it.
- **Rogue** breaks each rule once, on purpose. Each rule must report exactly its Rogue violation.

When you add a module rule, add the matching violation to Rogue and keep Sample compliant.
