# Phase 04 - Aiel Permission System

## Status

**Planned** (Planning document retained for reference and project history)

---

## Authority Notice

The Aiel codebase (contracts, implementations, and tests) is the authoritative specification for work in progress. This document describes the planning context, implementation sequence, and design rationale. As Phase 04 is implemented, refer to the implementation in `Aiel/src/` and test coverage in `Aiel/tests/` as the definitive current behavior.

---

## Problem Statement

Aiel now has the foundation needed to implement permissions as a first-class framework feature. The
action-centered permission design is accepted enough to move from exploration to execution, and the
Strong ID extraction has landed on `develop`, which removes the largest prerequisite for permission
IDs, scope keys, subject keys, manifest IDs, and capability snapshot IDs.

The remaining risk is implementation order. The permission system touches public application
contracts, package boundaries, analyzers, source generators, EF Core migration tooling, ASP.NET Core
adapters, and client capability services. If those pieces are built opportunistically, Aiel can
accidentally recreate an invisible command pipeline, leak permission infrastructure into application
contracts, or persist permission names before the stable identity model is enforceable.

Phase 04 defines the implementation sequence that keeps the first slice small, testable, and aligned
with Clean Architecture.

Aiel and Aviendha are greenfield for this permission work. The implementation therefore chooses
clean moves over compatibility scaffolding: no type-forwarding shims, no obsolete wrappers, and no
temporary duplicate contracts.

---

## What Is Being Developed

### 4.1 Application contract boundary

Introduce `Aiel.Application.Contracts` as the public application boundary package for:

- `IAction`
- `ICommand`
- `IQuery<TResult>`
- `ICommandHandler<TCommand>`
- `IQueryHandler<TQuery, TResult>`
- command and query dispatcher interfaces
- command and query pipeline behavior interfaces
- `IExecutionContext`
- `IActionExecutionContext<TAction>`
- application service contract conventions

Command/query dispatcher abstractions remain lower-level application contracts, but generated
application services must not be implemented as facades over one handler per action.

### 4.2 Permission package family

Create the package family from the accepted permission design:

| Package | Purpose |
|---|---|
| `Aiel.Permissions.Domain.Shared` | Permission value objects, strong IDs, lifecycle enums, scope and subject type names |
| `Aiel.Permissions.Domain` | Permission catalog and grant domain model |
| `Aiel.Permissions.Application.Contracts` | Permission manager, checker, gate, manifest, resource authorization, and capability contracts |
| `Aiel.Permissions.Application` | Default permission checker, action gate, grant evaluator, scope orchestration |
| `Aiel.Permissions.EntityFrameworkCore` | Provider-neutral EF Core store mappings and migration DSL |
| `Aiel.Permissions.EntityFrameworkCore.PostgreSql` | PostgreSQL-specific migrations and provider wiring |
| `Aiel.Permissions.AspNetCore` | Execution-context integration and generated endpoint support |
| `Aiel.Permissions.Client` | Client capability abstractions |
| `Aiel.Permissions.Client.Blazor` | Blazor action-visibility helpers |
| `Aiel.Permissions.Testing` | Test doubles and sample permission fixtures |
| `Aiel.Permissions.Generators` | Permission constants, manifests, definitions, and registration helpers |
| `Aiel.Permissions.Analyzers` | Missing authorization, unsafe string, registration, rename, and client-safety diagnostics |

Future feature families should follow this package shape where it applies:
`<Family>.Domain.Shared`, `<Family>.Domain`, `<Family>.Application.Contracts`,
`<Family>.Application`, and outward adapter packages.

### 4.3 Explicit action gate

Implement `IActionGate<TAction>` as an explicitly invoked application service helper. It creates the
typed action execution context, runs structural validation, runs permission checks, and fails closed
when an action has no authorization story.

The gate must not load aggregates, save data, dispatch domain events, or support DI-ordered behavior
chains. Those remain application service and domain responsibilities.

### 4.4 Stable permission identity and migration tooling

Generate permission identity from configured root + relative action namespace + action type name.
Once a permission appears in a committed manifest snapshot, its human-readable permission name is a
published contract. Stable IDs are preserved across renames, and manifest-diff tooling drives EF Core
permission migrations.

### 4.5 First implementation slice

The first working vertical slice is deliberately narrow:

- `RescheduleAppointment` action
- one structural validator
- one resource-aware permission checker
- one application service method
- one hand-written endpoint/client sample pair
- one missing-authorization analyzer failure
- one manifest-driven rename migration test from `ChangeAppointment` to `RescheduleAppointment`

---

## Decisions

### D1 - Strong ID is available and should be used from the first permission package

**Decision:** depend on `Aiel.StrongIds` directly for permission IDs, grant IDs, scope keys, subject
keys, actor IDs, manifest IDs, and capability snapshot IDs.

**Rationale:** Strong ID is now a foundation feature. Permission packages no longer need temporary
primitive IDs or a dependency on `Aiel.Domain` just to obtain typed identifiers.

---

### D2 - Application contracts move before permission packages are implemented

**Decision:** introduce `Aiel.Application.Contracts` before adding permission runtime packages,
move the existing CQRS and execution-context contracts into it, delete the old locations in the same
change, and update all repository consumers immediately.

**Rationale:** Permissions attach to actions, and application service contracts are the boundary used
by generated endpoints, HTTP clients, Blazor Server, background workers, and tests. The permission
feature should build on that boundary rather than forcing one later.

Aiel has no external consumers yet. Compatibility shims would add technical debt without protecting
a real consumer.

---

### D3 - Permission-free marker name is selected

**Decision:** use `DoesNotRespectAuthority` as the explicit permission-free marker, and require a
non-empty `Reason`.

**Rationale:** The planning draft now uses `DoesNotRespectAuthority` in prose and code. The marker is
intentionally uncomfortable and searchable, and the required reason prevents casual use.

---

### D4 - The first analyzer is fail-closed, not cosmetic

**Decision:** the first analyzer must make a missing authorization story a build failure in a sample
project.

**Rationale:** The design depends on fail-closed enforcement. Warnings can come later for naming style,
client metadata, and raw permission strings, but missing authorization must be an error from the first
implementation slice.

---

### D5 - Generator work starts only after manifest shape is executable

**Decision:** do not build broad generator templates until the manifest item shape, stable ID behavior,
and manifest snapshot diff test exist.

**Rationale:** The generator should consume stable contracts, not invent them. This avoids persisting
permission names before rename behavior is testable.

---

### D6 - Execution context remains interface-based in Phase 04

**Decision:** keep `IExecutionContext` as an interface for Phase 04. Add
`IActionExecutionContext<TAction>` as an interface and provide a sealed framework implementation named
`ActionExecutionContext<TAction>`.

**Rationale:** This is the simplest shape for the first permission slice and lets the action gate
create typed child contexts without turning execution context hardening into a blocker. Because there
are no external consumers, a later Phase 04b or Phase 04c refactor can still harden the context model
without compatibility debt if the implemented slice proves that is the better shape.

---

### D7 - Edge adapters are samples first, not generated endpoints first

**Decision:** Phase 04 requires hand-written ASP.NET Core endpoint and HTTP client samples. Broad
endpoint/client generation is deferred.

**Rationale:** The phase must prove that edge adapters call application service contracts and preserve
`Result` semantics. It does not need a full endpoint generator to prove that contract.

---

### D8 - Use review checkpoints inside the phase

**Decision:** keep Phase 04 as one phase, but land it through review checkpoints 04a, 04b, and 04c.

**Rationale:** Agent throughput is high and human review is the scarce resource. Review checkpoints
keep the work reviewable without splitting the phase into separate planning documents.

---

## Completion Criteria

Phase 04 is complete when all of the following are true:

- [ ] `Aiel.Application.Contracts` exists and owns the public action and execution-context contracts.
- [ ] Existing CQRS contracts are moved to `Aiel.Application.Contracts`; old definitions are deleted in the same change.
- [ ] Permission package projects exist with dependency direction matching the package ownership table.
- [ ] `Aiel.Permissions.Domain` owns the grant aggregate, catalog entry, and lifecycle invariants.
- [ ] `IPermissionStore` and `IPermissionManager` exist before EF Core persistence work begins.
- [ ] Permission public models use `Aiel.StrongIds` and value objects from the first implementation.
- [ ] `IActionGate<TAction>` validates, authorizes, and returns typed `Result` failures without running business logic.
- [ ] Runtime permission errors exist for missing authorization story, denial, missing definition, invalid scope, and stale client capability.
- [ ] A missing authorization story fails an analyzer test.
- [ ] Permission manifests include stable ID, current name, source action type, lifecycle, scope type, and previous names.
- [ ] Manifest-diff migration tooling preserves grants when `ChangeAppointment` is renamed to `RescheduleAppointment`.
- [ ] `RescheduleAppointment` has a validator, resource-aware checker, application service method, and test coverage proving the gate order.
- [ ] Endpoint and HTTP client samples call the application service contract and preserve `Result` / `Result<T>` semantics.
- [ ] Client capability contracts support selected-permission requests and explicit empty continuation tokens.
- [ ] New projects are listed in `TwoRivers.slnx` and `Aiel.slnx`, `virtual-folders.json`, and central package/version files where required.
- [ ] All new tests fail before their implementation and pass after implementation.
- [ ] Full solution build is clean with no warnings.

---

## Implementation Plan

Tasks execute in this order to keep public contracts stable before runtime, analyzer, and generator
work depends on them. Each project-creation task includes the required updates to `TwoRivers.slnx`,
`Aiel.slnx`, `virtual-folders.json`, central package/version files, and package README files.

### Task 0 - Verify accepted design inputs

**Files:**
- `Aiel/docs/planning/permission-system.md`

Verify that the planning draft uses `DoesNotRespectAuthority` in prose and code, and that no current
planning example uses `[AllowWithoutPermission]`.

No production code changes in this task.

### Task 1 - Add `Aiel.Application.Contracts`

**Projects:**
- Add `Aiel/src/Aiel.Application.Contracts/Aiel.Application.Contracts.csproj`
- Add `Aiel/tests/Aiel.Application.Contracts.UnitTests/Aiel.Application.Contracts.UnitTests.csproj`

**Initial contracts:**
- `IAction`
- `ICommand : IAction`
- `IQuery<TResult> : IAction`
- `ICommandHandler<TCommand>`
- `IQueryHandler<TQuery, TResult>`
- command and query dispatcher interfaces
- command and query pipeline behavior interfaces
- `IExecutionContext`
- `IActionExecutionContext<TAction>`
- sealed `ActionExecutionContext<TAction>` implementation

**Test gate:**
- Existing `Aiel.Application` tests must continue to pass after consumer references are updated.
- Contract tests verify child action context preserves actor, correlation, causation, properties, and action payload.

### Task 2 - Move and delete existing CQRS and execution contracts

**Projects:**
- `Aiel/src/Aiel.Application/Aiel.Application.csproj`
- `Aiel/src/Aiel.Domain/Aiel.Domain.csproj`
- `Aiel/tests/Aiel.Application.UnitTests/Aiel.Application.UnitTests.csproj`

Move the current command, query, dispatcher, pipeline behavior, handler, and execution-context
contracts to `Aiel.Application.Contracts`. Delete the old source locations in the same change and
update all repository references.

No type-forwarding, no `[Obsolete]`, and no duplicate compatibility wrappers.

**Test gate:**
- Existing command and query dispatcher tests pass without duplicate `ICommand`, `IQuery<TResult>`, or `IExecutionContext` definitions.

### Task 3 - Create permission domain shared contracts

**Projects:**
- Add `Aiel/src/Aiel.Permissions.Domain.Shared/Aiel.Permissions.Domain.Shared.csproj`
- Add `Aiel/tests/Aiel.Permissions.Domain.Shared.UnitTests/Aiel.Permissions.Domain.Shared.UnitTests.csproj`

**Initial types:**
- `PermissionName`
- `PermissionStableId`
- `PermissionGrantId`
- `PermissionScopeTypeName`
- `PermissionScopeKey`
- `PermissionSubjectTypeName`
- `PermissionSubjectKey`
- `CapabilitySnapshotVersion`
- lifecycle and decision enums

Use `Aiel.StrongIds` for IDs and explicit value objects for human-readable names.

**Test gate:**
- Invalid value object inputs return failures or throw only where construction is intentionally exceptional.
- Strong ID authoring compiles through the packaged generator.

### Task 4 - Create permission domain model

**Projects:**
- Add `Aiel/src/Aiel.Permissions.Domain/Aiel.Permissions.Domain.csproj`
- Add `Aiel/tests/Aiel.Permissions.Domain.UnitTests/Aiel.Permissions.Domain.UnitTests.csproj`

**Initial model:**
- `PermissionGrant` aggregate
- permission catalog entry entity or aggregate, depending on invariant ownership
- lifecycle transition rules
- grant decision invariants
- scope and subject matching invariants that do not require infrastructure

**Test gate:**
- Grants cannot be created with default IDs, empty subjects, empty scopes, or invalid permission names.
- Lifecycle transitions reject invalid moves without exceptions for expected control flow.

### Task 5 - Add permission application contracts and failing gate tests

**Projects:**
- Add `Aiel/src/Aiel.Permissions.Application.Contracts/Aiel.Permissions.Application.Contracts.csproj`
- Add `Aiel/tests/Aiel.Permissions.Application.UnitTests/Aiel.Permissions.Application.UnitTests.csproj`

**Contracts:**
- `IActionValidator<TAction>`
- `IActionPermissionChecker<TAction>`
- `IActionGate<TAction>`
- `IPermissionDefinitionRegistry`
- `IPermissionGrantEvaluator`
- `IPermissionScopeResolver`
- `IPermissionStore`
- `IPermissionManager`
- `IResourceAuthorizationService`
- manifest DTOs
- client capability DTOs
- `PermissionErrors`

**Failing tests before implementation:**
- Validation runs before permission checks.
- Permission checker does not run when validation fails.
- Missing authorization story returns `PermissionErrors.MissingAuthorizationStory`.
- Denied permission returns `PermissionErrors.PermissionDenied`.
- Permission manager creates and revokes grants through `IPermissionStore` without exposing persistence records.

### Task 6 - Implement permission application services

**Projects:**
- Add `Aiel/src/Aiel.Permissions.Application/Aiel.Permissions.Application.csproj`

Implement the default action gate, default action permission checker, grant evaluator, and permission
manager. The gate explicitly creates the action context, runs validation, resolves a concrete or
generated default permission checker, and returns `Result<IActionExecutionContext<TAction>>`.

The gate must not support ordered behavior chains.

**Test gate:**
- All Task 5 tests pass.
- Tests verify the gate never invokes business logic.
- Manager tests use an in-memory fake store from the unit test project until `Aiel.Permissions.Testing` exists.

### Task 7 - Create permission testing package

**Projects:**
- Add `Aiel/src/Aiel.Permissions.Testing/Aiel.Permissions.Testing.csproj`
- Add `Aiel/tests/Aiel.Permissions.Testing.UnitTests/Aiel.Permissions.Testing.UnitTests.csproj`

**Initial helpers:**
- fake permission store
- fake execution context factory
- sample permission IDs, subject keys, and scope keys
- inline action fixture types for analyzer, generator, and persistence tests

**Test gate:**
- Testing helpers return valid non-default IDs and never expose null public properties.
- Fixture actions are explicitly scoped to tests and do not become production sample contracts.

### Task 8 - Add fail-closed analyzer foundation

**Projects:**
- Add `Aiel/src/Aiel.Permissions.Analyzers/Aiel.Permissions.Analyzers.csproj`
- Add `Aiel/tests/Aiel.Permissions.Analyzers.UnitTests/Aiel.Permissions.Analyzers.UnitTests.csproj`

**First diagnostic:**
- Action has no concrete checker, generated permission definition, or explicit permission-free marker.

Generated permission definition recognition is deferred to Task 9 because the generator contract does
not exist yet.

**Test gate:**
- A sample action without an authorization story fails analyzer tests.
- A sample action with a concrete checker passes.
- A sample action with `[DoesNotRespectAuthority(Reason = "...")]` passes.
- An empty or whitespace `Reason` fails analyzer tests.

### Task 9 - Define manifest snapshot and generator minimum viable output

**Projects:**
- Add `Aiel/src/Aiel.Permissions.Generators/Aiel.Permissions.Generators.csproj`
- Add `Aiel/tests/Aiel.Permissions.Generators.UnitTests/Aiel.Permissions.Generators.UnitTests.csproj`

**Generator output:**
- permission constants
- manifest items with stable ID, permission name, action type name, lifecycle, scope type, and previous names
- registration helper invoked explicitly from a `AielDependency`

Follow the existing Aiel packaging pattern used by `Aiel.Results` and `Aiel.StrongIds`: runtime
packages that require generation must deliver the generator DLL under `analyzers/dotnet/cs` and use
`ProjectReference` with `OutputItemType="Analyzer"`.

**Test gate:**
- `RescheduleAppointment` emits deterministic constants and manifest output.
- Re-running generation preserves the stable ID for an existing manifest item.
- The Task 8 analyzer accepts generated permission definitions through the real generator output.

### Task 10 - Add permission EF Core store and migration DSL prototype

**Projects:**
- Add `Aiel/src/Aiel.Permissions.EntityFrameworkCore/Aiel.Permissions.EntityFrameworkCore.csproj`
- Add `Aiel/src/Aiel.Permissions.EntityFrameworkCore.PostgreSql/Aiel.Permissions.EntityFrameworkCore.PostgreSql.csproj`
- Add `Aiel/tests/Aiel.Permissions.EntityFrameworkCore.IntegrationTests/Aiel.Permissions.EntityFrameworkCore.IntegrationTests.csproj`

**First migration DSL operations:**
- `Add`
- `Rename`
- `Deprecate`

The rename test uses inline fixture action types from `Aiel.Permissions.Testing`; it does not depend
on the Task 11 `RescheduleAppointment` reference slice.

**Test gate:**
- Grants are created through `IPermissionManager` before the rename migration runs.
- Rename from fixture `ChangeAppointment` to fixture `RescheduleAppointment` preserves existing grants.
- Manifest snapshot records the previous permission name.
- EF Core mappings round-trip permission IDs, scope, subject, and grant decision strong IDs.

### Task 11 - Add `RescheduleAppointment` reference slice

**Projects:**
- Prefer a sample or fixture project rather than Aviendha production code for the first Aiel slice.
- Use `Aiel.Permissions.Testing` for shared sample IDs and fake services.

**Slice components:**
- `RescheduleAppointment : ICommand`
- `RescheduleAppointmentValidator`
- `RescheduleAppointmentPermissionChecker`
- `IAppointmentApplicationService`
- `AppointmentApplicationService`
- fake appointment repository and resource authorization service

**Test gate:**
- Default appointment ID fails structural validation.
- Grant denial never loads the aggregate.
- Resource denial returns denial after grant success.
- Success path calls aggregate reschedule and saves once.

### Task 12 - Add ASP.NET Core and HTTP client adapter sample

**Projects:**
- Add `Aiel/src/Aiel.Permissions.AspNetCore/Aiel.Permissions.AspNetCore.csproj`
- Add integration tests or sample web app as needed.

Hand-written endpoint sample code creates the action and calls `IAppointmentApplicationService`.
Hand-written HTTP client sample code implements the same application service contract and uses
`ResultHttpClientExtensions`.

**Test gate:**
- Endpoint does not repeat action-level permission attributes.
- HTTP client preserves `Result` semantics.

### Task 13 - Add client capability contracts and Blazor helper sample

**Projects:**
- Add `Aiel/src/Aiel.Permissions.Client/Aiel.Permissions.Client.csproj`
- Add `Aiel/src/Aiel.Permissions.Client.Blazor/Aiel.Permissions.Client.Blazor.csproj`

**Contracts and behavior:**
- selected-permission capability requests
- explicit empty continuation token
- snapshot version
- refresh on authorization failure
- `CanExecute` or generated action-specific component sample

**Test gate:**
- Unavailable `RescheduleAppointment` is hidden.
- Authorization failure invalidates or refreshes the capability snapshot.

### Task 14 - Documentation and Aviendha planning pointer

**Files:**
- `Aiel/docs/planning/permission-system.md`
- `Aviendha/docs/SAD.md`
- package READMEs for new Aiel permission packages

Update Aiel docs with the implemented contract names, packaging choices, and migration behavior.
Update `Aviendha/docs/SAD.md` only with a pointer that `Aiel.Permissions.*` is the chosen
permission mechanism. Do not add the Aviendha action-based permission matrix or Safety Plan
permission boundaries in Phase 04; those belong to the later Aviendha adoption phase.

---

## Review Checkpoints

These checkpoints are review boundaries inside Phase 04, not separate phase documents.

### 04a - Contracts, domain, gate, and analyzer foundation

Includes Tasks 0 through 8. This checkpoint establishes the public application contracts, permission
domain model, permission application contracts, action gate, permission manager/store contracts,
testing package, and fail-closed analyzer baseline.

### 04b - Identity, generator, persistence, and reference slice

Includes Tasks 9 through 11. This checkpoint establishes manifest generation, stable identity,
manifest-diff migration behavior, EF Core persistence, and the `RescheduleAppointment` reference
slice.

### 04c - Edge adapters and documentation

Includes Tasks 12 through 14. This checkpoint establishes ASP.NET Core and HTTP client samples,
client capability helpers, Blazor visibility behavior, package documentation, and the Aviendha SAD
pointer.

---

## Validation Strategy

For each coding task:

1. Write or update the failing test first.
2. Run the focused test project and confirm the new test fails for the intended reason.
3. Implement the smallest complete slice for that task.
4. Run the focused test project again.
5. Run the full Aiel test suite before declaring the task complete.

No task is complete while the solution has warnings, analyzer diagnostics, or uncommitted changes.

---

## Out Of Scope For Phase 04

- Full Aviendha production permission rollout.
- Push-based capability invalidation.
- Client-side execution of permission checkers by default.
- Broad generated endpoint/client coverage; Phase 04 uses hand-written samples.
- Dapper permission store support.
- Cross-service distributed permission cache.

---

## Risks And Mitigations

| Risk | Mitigation |
|---|---|
| Contract move creates temporary duplicate APIs | Move and delete in one change; no shims, obsolete wrappers, or duplicate compatibility contracts |
| Permission names drift before tooling is ready | Implement manifest stable IDs and rename test before broad generator work |
| Action gate becomes a new hidden pipeline | Keep `IActionGate<TAction>` explicitly invoked and prohibit ordered behavior chains |
| Analyzer cannot see generated definitions reliably | Make manifest and registration helper output deterministic and committed in tests |
| Client capability data becomes stale | Treat server rejection as authoritative and refresh snapshots after authorization failures |
| Package graph becomes circular | Keep `Aiel.Application.Contracts` and `Aiel.StrongIds` below permissions; keep EF, ASP.NET Core, and client packages outward |
| Agent squad fills ambiguous gaps differently | Keep each task's owner, deferred behavior, and test gate explicit before implementation begins |
| Aviendha SAD drifts ahead of implementation | Add only the selected-mechanism pointer in Phase 04 and defer Aviendha matrices to the adoption phase |

---

## Notes

`Aiel.StrongIds` is now the identity foundation for this phase. Permission packages should depend
on it directly instead of depending on `Aiel.Domain` for ID contracts.

The implementation should prefer sample or testing projects for the first vertical slice. Aviendha
adoption should happen only after the Aiel permission package contracts, analyzer behavior,
generator manifest output, and EF migration story are stable.