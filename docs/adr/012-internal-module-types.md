# ADR-012: Module implementation types are internal; the IModule class is the only public type

- Status: Accepted (2026-11-19, S1-1)

## Context

CLAUDE.md makes services "registered by interface" and ADR-009 says only Contracts types cross module boundaries. Writing S1-1's rules exposed a conflict: a public controller cannot take an internal service interface in its constructor, or return an internal DTO from a public action (C# inconsistent accessibility, CS0050/CS0051). MVC's default `ControllerFeatureProvider` only discovers public controllers. So either service interfaces and DTOs become public, which other modules could then see and use, or controllers become internal and the platform finds them.

## Decision

1. In a module implementation project (`Crm.<Module>`), every type is `internal` except the class implementing `IModule`, which `Crm.Host` creates.
2. Controllers, API DTOs, services, models, mappers, validators and handlers are all `internal`.
3. `Crm.SharedKernel`'s `AddModules` adds a `ModuleControllerFeatureProvider` that also accepts non-public `ControllerBase` subclasses, **only in module assemblies** (each `IModule.ControllersAssembly`). Elsewhere MVC's public-only rule is unchanged.

Enforced by `Module_Types_AreInternal_ExceptTheModuleClass` (architecture tests), and by `AddModules_InternalControllerInModuleAssembly_IsDiscovered` and `AddModules_InternalControllerOutsideModuleAssemblies_IsNotDiscovered` (`Crm.SharedKernel.Tests`).

## Consequences

- Another module cannot compile against a module's services, DTOs or controllers, even by mistake; only the mediator and its messages are available to it (and since ADR-013 it cannot reference another module at all).
- Routing, model binding, filters and OpenAPI generation work as usual; they use the discovered controller types regardless of visibility.
- Unit tests inside a module's test project need `InternalsVisibleTo` from the module (the scaffolder adds it, S1-4).
- `Crm.Host`'s own controllers (health) stay public; the host is not a module.

## Alternatives rejected

- **Public service interfaces and DTOs**: simplest, but any module referencing the implementation project (a violation, but one the compiler would allow) could call them, and the visibility rule would need exceptions.
- **`InternalsVisibleTo` from modules to MVC**: MVC does not use it for discovery.
