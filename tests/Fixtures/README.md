# Architecture test fixtures

Two test-only modules that `Crm.Architecture.Tests` checks its module rules against (S1-1, ADR-013). They are never composed into the host.

- `Modules/Fixture.Sample` follows every rule. Each rule must report nothing for it.
- `Modules/Fixture.Rogue` breaks each rule once, on purpose. Each rule must report exactly its Rogue violation.
- `Fixture.Contracts` stands in for `Crm.Mediator.Contracts`: the fixtures' messages, under `Fixture.Contracts.Sample` and `Fixture.Contracts.Rogue`.

Fixture module projects may reference `Crm.SharedKernel`, `Crm.Mediator` and `Fixture.Contracts`; Rogue also references Sample, which is its project-reference violation.

When you add a module rule, add the matching violation to Rogue and keep Sample compliant.
