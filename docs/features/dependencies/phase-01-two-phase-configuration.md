# Phase 01 — Two-Phase Dependency Configuration

## Status

**Complete** (Implementation code is now the authoritative reference; this document retained for reference and project history)

---

## Authority Notice

The Aiel codebase (contracts, implementations, and tests) is the authoritative specification for this phase. This document describes the planning context and historical rationale. Refer to the implementation in `Aiel/src/` and test coverage in `Aiel/tests/` as the definitive current behavior.

---

## Problem Statement (Historical Context)

This phase addressed a critical lifecycle gap. Initially, `AielDependencyConfigurator` declared a virtual `PreConfigureAsync` method alongside `ConfigureAsync`, but neither startup path (source-generated nor reflection-based) invoked it, causing any module override to be silently ignored.

The two-phase configuration lifecycle has now been implemented across both orchestration paths. The runtime now guarantees that all modules' `PreConfigureAsync` completes — in topological order across the entire dependency graph — before any module's `ConfigureAsync` begins.

---

## What Is Being Developed

A functional two-phase configuration lifecycle for all dependency modules:

| Phase | Method | Purpose |
|-------|--------|---------|
| 1 — Pre-Configure | `PreConfigureAsync` | Early shared setup. Register options builders, configure integration entry points, or perform any work that other modules need to have completed before their own configure phase begins. |
| 2 — Configure | `ConfigureAsync` | Main service registration and configuration. Reads and finalises options established in phase 1. |

**Core guarantee:** all modules' `PreConfigureAsync` completes — across the entire dependency graph, in topological order — before any module's `ConfigureAsync` begins. This guarantee holds in both startup paths.

---

## Decisions

### D1 — Wire it or remove it

**Options:**

- **Wire it.** Implement the two-phase model in both orchestration paths so the method works as documented.
- **Remove it.** Delete `PreConfigureAsync` from `AielDependencyConfigurator` until the full design is settled and the implementation is ready.

**Decision: Wire it.**

**Rationale:** The two-phase model has concrete value in a layered framework. A low-level infrastructure module (e.g., an options provider or a shared service bus configuration) may need to register extension points that higher-level modules then populate during their own configure phase. Removing `PreConfigureAsync` would discard a well-understood pattern, require documentation retraction, and necessitate re-introduction later. The implementation is straightforward — it is simply a second iteration pass in an existing loop.

---

### D2 — Separate optional interface vs. full contract

**Options:**

- **Separate interface.** Introduce `IPreConfigureDependency` or similar; orchestrators cast-and-call if the interface is present. `IDependencyConfigurator` remains unchanged.
- **Add to `IDependencyConfigurator`.** Declare `PreConfigureAsync` directly on the existing interface alongside `ConfigureAsync`.

**Decision: Add to `IDependencyConfigurator`.**

**Rationale:** The framework's stated philosophy is explicit, predictable contracts — not optional duck-typed behaviour. A hidden secondary interface creates a discoverable gap: a developer reading `IDependencyConfigurator` would see only `ConfigureAsync` and have no signal that `PreConfigureAsync` exists. Adding it to the primary interface makes the full lifecycle visible at the point of implementation. Since `AielDependencyConfigurator` already provides a virtual no-op implementation of `PreConfigureAsync`, this is non-breaking for all `AielDependencyConfigurator` subclasses. Only classes that directly implement `IDependencyConfigurator` (currently limited to test stubs) require a trivial update.

---

### D3 — Semantics of the two-phase boundary

**Options:**

- **Interleaved.** Each node runs `PreConfigureAsync` then `ConfigureAsync` before moving to the next node.
- **Separated.** A full pre-configure pass over all nodes completes before the configure pass begins.

**Decision: Separated (full pass).**

**Rationale:** Interleaving defeats the purpose. If `DependencyB.PreConfigureAsync` and `DependencyB.ConfigureAsync` run before `DependencyA.PreConfigureAsync`, then `DependencyA.PreConfigureAsync` cannot observe anything `DependencyB` registered. The separated model guarantees that by the time any module's `ConfigureAsync` begins, the entire graph has finished pre-configure — making `PreConfigureAsync` useful for establishing shared state that `ConfigureAsync` can depend on unconditionally.

---

### D4 — Placement of the two-phase split

**Options:**

- **New top-level method.** Add `PreConfigureAsync` to `IDependencyManager` and call it explicitly from the startup extensions.
- **Internal to `ConfigureAsync`.** Keep the single `ConfigureAsync` call on `IDependencyManager`; implement both passes inside it.

**Decision: Internal to `ConfigureAsync`.**

**Rationale:** The two-phase split is an implementation detail of the configuration lifecycle, not a concern the startup extension needs to manage. The `IHostApplicationBuilder` extensions (`AddApplicationAsync`, `RegisterDependenciesAsync`) call `ConfigureAsync` once and expect the full configuration contract to be fulfilled. Keeping both passes inside `ConfigureAsync` preserves the existing calling convention, minimises the change surface, and avoids leaking lifecycle internals into the startup API. If future requirements demand more granular control, the API can be extended at that point.

---

## Completion Criteria

The phase is complete when all of the following are true:

- [x] `IDependencyConfigurator` declares `PreConfigureAsync` with the same signature as `ConfigureAsync`.
- [x] `DependencyManager.ConfigureAsync` executes a full pre-configure pass (all nodes, topological order) before executing the configure pass.
- [x] `DependencyDiscoveryExtensions.ConfigureDependenciesAsync` executes a full pre-configure pass (all nodes, depth-descending order) before executing the configure pass.
- [x] Tests exist and pass that verify `PreConfigureAsync` is invoked exactly once per configurator in a diamond dependency graph (`DependencyManager` path).
- [x] Tests exist and pass that verify the two-phase guarantee: all `PreConfigureAsync` calls complete before any `ConfigureAsync` begins, for both the `DependencyManager` path and the reflection path.
- [x] All pre-existing tests continue to pass.
- [x] `Framework.md` naming drift and the `PreConfigureAsync` description have been corrected to reflect the now-implemented behaviour.

---

## Implementation Plan

Tasks execute in this order to maintain a green-to-green transition.

### Task 1 — Add `PreConfigureAsync` to `IDependencyConfigurator` and update direct implementors

**Files:**
- `Aiel\src\Aiel\Aiel\Dependencies\IDependencyConfigurator.cs` — add declaration
- `Aiel\tests\Aiel.UnitTests\Aiel\Dependencies\AielDependencyManagerTests.cs` — update 4 private configurator stubs
- `Aiel\tests\Aiel.UnitTests\Aiel\Dependencies\AssemblyDescriptorTests.cs` — update 1 private configurator stub

Build must be green after this task. No test logic changes yet.

### Task 2 — Write failing tests (G1)

Add to `AielDependencyManagerTests.cs`:
- `PreConfigureAsync_Is_Invoked_Once_Per_Configurator_In_Diamond_Graph` — verifies `PreConfigureAsync` is called exactly once on each configurator.
- `ConfigureAsync_Runs_All_PreConfigureAsync_Before_Any_ConfigureAsync_In_Linear_Graph` — verifies the two-phase guarantee on the `DependencyManager` path.

Add to `ApplicationConfigurationTests.cs`:
- `AddApplication_DependenciesArePreConfiguredBeforeConfigured` — verifies the two-phase guarantee on the reflection path.

These tests MUST fail at this point (G1 gate). Existing 698 tests MUST still pass.

### Task 3 — Implement two-phase pass in `DependencyManager`

**File:** `Aiel\src\Aiel\Aiel\Dependencies\DependencyManager.cs`

`ConfigureAsync` executes two sequential loops over `_orderedNodes`:
1. Call `PreConfigureAsync` on each configurator instance.
2. Call `ConfigureAsync` on each configurator instance.

### Task 4 — Implement two-phase pass in `DependencyDiscoveryExtensions`

**File:** `Aiel\src\Aiel\Aiel\Dependencies\DependencyDiscoveryExtensions.cs`

`ConfigureDependenciesAsync` executes two sequential loops over the depth-ordered node list:
1. Call `PreConfigureAsync` on each node's `Instance`.
2. Call `ConfigureAsync` on each node's `Instance`.

### Task 5 — Verify all tests pass (G2) and build is clean (G3)

Run full test suite. All new tests must now pass. All 698 pre-existing tests must still pass. Build must have zero warnings.

### Task 6 — Correct `Framework.md` naming drift and update `PreConfigureAsync` description

Update `Framework.md` to replace `TrDependency`/`TrApplication` with `AielDependencyConfigurator`/`AielApplication` and revise the `PreConfigureAsync` note to describe the now-implemented two-phase lifecycle.

---

## Notes

The `AssemblyAnalyzer` enforces that each assembly declares exactly one `AielDependencyConfigurator` subclass. It does not need updating for this phase — the pre-configure lifecycle is fully inside the existing module contract.

The source generator (`DependencyGraphSourceGenerator`) emits `DependencyDescriptor` objects whose `Configurators` list is consumed by `DependencyManager`. The generator itself does not need updating — the `DependencyManager` change is sufficient for the source-generated path.
