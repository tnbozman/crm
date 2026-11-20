# Module exchange fixture (S1-5)

Copied by `scripts/checks/new-module.sh` into its temporary copy, after it scaffolds the modules Zzz and Yyy. Never built in the repository: Zzz and Yyy exist only in that copy.

- `src/Modules/Crm.Yyy/`: Yyy subscribes to Zzz's `ZzzItemCreated` (through its inbox) and sends Zzz's `GetZzzItemSummary` query, both through the mediator. Yyy's project still references only `Crm.SharedKernel` and `Crm.Mediator`.
- `tests/Crm.Host.Tests/ModuleExchangeTests.cs`: the API-level test, run against a database migrated by `make db-reset`.
