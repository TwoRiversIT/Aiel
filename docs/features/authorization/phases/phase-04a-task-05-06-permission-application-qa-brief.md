# Phase 04a Tasks 5+6 Permission Application Contracts and Implementation Brief

- **Date:** 2026-05-25T17:33:03.006-07:00
- **Author:** Verin
- **Scope:** Tasks 5 and 6, treated as one atomic slice. Create `Aiel.Authorization.Application.Contracts`, `Aiel.Authorization.Application`, and `Aiel.Authorization.Application.UnitTests`. Do not mix EF mappings, ASP.NET adapters, analyzers, generators, or client-side capability concerns into this change.
- **Layer:** Application. This slice belongs here because it owns the gate orchestration contract (`IActionGate<TAction>`), the permission manager, and the concrete implementations that coordinate domain behavior with infrastructure ports — all of which belong at the application boundary, not in the domain and not in infrastructure.
- **Baseline observed in this worktree:**
  - `Aiel/src/Aiel.Authorization.Application.Contracts/` does not exist yet.
  - `Aiel/src/Aiel.Authorization.Application/` does not exist yet.
  - `Aiel/tests/Aiel.Authorization.Application.UnitTests/` does not exist yet.
  - `Aiel.Authorization.Domain.UnitTests` is green: all tests passing.
  - `Aiel.Authorization.Domain.Shared.UnitTests` is green: all tests passing.
- **Non-collision rule:** Safe QA work for this task is a review/validation artifact plus isolated test files. Do not edit files in the domain projects, the infrastructure skeleton, or the existing application framework (`Aiel.Application`) just to make this slice compile.

## Slice boundary decision: Tasks 5 and 6 are atomic

The plan stages Task 5 ("contracts + failing gate tests") as a standalone slice before Task 6 ("implementations"). That split is not viable under the clean-build rule. The behavioral failing tests specified in Task 5 — gate ordering, permission checker skip, `PermissionErrors` return paths, and manager contract tests — all require calling a concrete implementation. Without `DefaultActionGate<TAction>` and `DefaultPermissionManager` (Task 6 scope), the test project must reference `Aiel.Authorization.Application` to compile those tests, and that project does not exist in a Task-5-only slice.

A Task-5-only test project that omits the reference to `.Application` can only carry structural surface tests (type existence, namespace, interface member shapes). Surface tests are welcome and are listed below, but they are green-on-landing checks, not meaningful red-first behavioral tests. Delivering them as the "failing gate tests" would misrepresent the test gate.

**Decision: Tasks 5 and 6 ship in one PR.** The red-first path, described below, is achievable within a single slice.

## Focused acceptance gate

Treat `Aiel/tests/Aiel.Authorization.Application.UnitTests/Aiel.Authorization.Application.UnitTests.csproj` as the authoritative acceptance gate for Tasks 5+6.

Tasks 5+6 are not done until all of the following are true:

1. All three new projects exist and are registered in `TwoRivers.slnx`, `Aiel/Aiel.slnx`, and `Aiel/virtual-folders.json`.
2. `dotnet test .\Aiel\tests\Aiel.Authorization.Application.UnitTests\Aiel.Authorization.Application.UnitTests.csproj --nologo --verbosity minimal` passes cleanly.
3. `dotnet test .\Aiel\tests\Aiel.Authorization.Domain.UnitTests\Aiel.Authorization.Domain.UnitTests.csproj --nologo --verbosity minimal` still passes. Tasks 5+6 must not weaken the Task 4 domain contract.
4. `dotnet test .\Aiel\tests\Aiel.Authorization.Domain.Shared.UnitTests\Aiel.Authorization.Domain.Shared.UnitTests.csproj --nologo --verbosity minimal` still passes.
5. The contracts project depends only on `Aiel.Application.Contracts`, `Aiel.Authorization.Domain`, and `Aiel.Results`. It must not carry references to EF, HTTP, or any other infrastructure assembly.
6. The implementation project depends only on `Aiel.Authorization.Application.Contracts` and `Aiel.Authorization.Domain`. It must not reference EF, HTTP, or any concrete infrastructure assembly.
7. The test project depends on `Aiel.Authorization.Application.Contracts`, `Aiel.Authorization.Application`, and xUnit. All test doubles (fakes for `IPermissionStore`, `IActionValidator<TAction>`, `IActionPermissionChecker<TAction>`) are hand-written inline in the test project; no mocking framework is required.
8. Gate orchestration tests prove the execution ordering contract with hand-written call-order fakes.
9. Permission manager tests prove the manager never calls persistence code directly — all store interactions flow through `IPermissionStore` fakes.
10. The PR demonstrates a red-to-green path: stub implementations that compile and fail, then full implementations that pass.

## Required contract surface (Task 5 scope)

All of the following must exist in `Aiel.Authorization.Application.Contracts` in the `Aiel.Authorization` namespace:

| Type | Kind | Purpose |
| --- | --- | --- |
| `IActionGate<TAction>` | Interface | Validates and permission-checks an action; returns `Result<IActionExecutionContext<TAction>>` |
| `IActionValidator<TAction>` | Interface | Validates action payload; returns `Result` |
| `IActionPermissionChecker<TAction>` | Interface | Checks permissions for a specific action type; returns `Result` |
| `IPermissionDefinitionRegistry` | Interface | Exposes the permission catalog (read-only from application layer) |
| `IPermissionGrantEvaluator` | Interface | Evaluates whether a grant applies to a given subject, scope, and context |
| `IPermissionStore` | Interface | Application-layer port for storing and retrieving grants; accepts and returns domain types |
| `IPermissionManager` | Interface | Orchestrates grant creation and revocation through `IPermissionStore` |
| `PermissionErrors` | Static class | Typed `Error` members; no exceptions |

`IPermissionStore` must accept and return domain value objects (`PermissionName`, `PermissionSubjectTypeName`, `PermissionSubjectKey`, `PermissionScopeTypeName`, `PermissionScopeKey`, `AuthorizationGrantDecision`) and strong IDs. It must not expose EF entity types, nullable columns, or raw persistence primitives at the contract boundary.

## Required implementations (Task 6 scope)

All of the following must exist in `Aiel.Authorization.Application` in the `Aiel.Authorization` namespace:

| Type | Purpose |
| --- | --- |
| `DefaultActionGate<TAction>` | Concrete gate: validates first, then checks permission, then returns execution context |
| `DefaultPermissionManager` | Concrete manager: delegates all persistence to `IPermissionStore`; never owns persistence logic |

The gate is **not** a pipeline behavior and is **not** wired into `DefaultCommandDispatcher` via DI decoration. It is an explicitly invoked service. Application services that require permission gating resolve `IActionGate<TAction>` from DI and call it directly before dispatching to a command handler.

## Red-first test path

Before any implementation exists, write the test project with stub implementations that throw `NotImplementedException`. The test project compiles cleanly; all behavioral tests fail. Then implement in this order:

**Phase 1 — stubs (all tests RED):**

- Add `Aiel.Authorization.Application.Contracts` with all interfaces and `PermissionErrors`.
- Add `Aiel.Authorization.Application` with `DefaultActionGate<TAction>` and `DefaultPermissionManager` that throw `NotImplementedException`.
- Add `Aiel.Authorization.Application.UnitTests` with all four test classes below.
- Run: all tests fail (`NotImplementedException` or assertion failures).

**Phase 2 — implement `DefaultActionGate<TAction>` (gate tests GREEN):**

- Implement validation-first, then permission-check orchestration.
- Run: `ActionGateOrchestrationTests` and `ActionGatePermissionCheckTests` turn green.

**Phase 3 — implement `DefaultPermissionManager` (manager tests GREEN):**

- Implement create and revoke using `IPermissionStore`.
- Run: `PermissionManagerContractTests` turns green.

**Phase 4 — surface tests always GREEN after Phase 1.**

## First red tests that should exist

1. **`ActionGateOrchestrationTests`**
   - Fails until `DefaultActionGate<TAction>` calls `IActionValidator<TAction>` before `IActionPermissionChecker<TAction>`.
   - Fails until the gate does not invoke the permission checker when validation returns a failure.
   - Use a call-order fake: a list that records invocation order by name; assert the list matches `["validate"]` on validation failure, and `["validate", "check"]` on validation success.

2. **`ActionGatePermissionCheckTests`**
   - Fails until the gate returns `PermissionErrors.MissingAuthorizationStory` when no `IActionPermissionChecker<TAction>` is registered for the action type.
   - Fails until the gate returns `PermissionErrors.PermissionDenied` when the checker returns a denied outcome.
   - Fails until the gate returns a non-null `Result<IActionExecutionContext<TAction>>` wrapping a valid context when both validation and permission check succeed.

3. **`PermissionManagerContractTests`**
   - Fails until `DefaultPermissionManager.CreateGrantAsync` calls `IPermissionStore` with the expected domain value objects and does not touch any persistence infrastructure directly.
   - Fails until `DefaultPermissionManager.RevokeGrantAsync` calls `IPermissionStore` with the expected grant ID and does not bypass the store.
   - Use an in-memory `FakePermissionStore` written in the test project that records all calls and returns controlled outcomes.

4. **`PermissionApplicationSurfaceTests`**
   - Compile-fails until the contracts assembly contains `IActionGate<TAction>`, `IActionValidator<TAction>`, `IActionPermissionChecker<TAction>`, `IPermissionStore`, `IPermissionManager`, and `PermissionErrors` in the `Aiel.Authorization` namespace.
   - Fails if `PermissionErrors` members are exception types instead of `Error` values.
   - Fails if any public interface member returns or accepts a nullable reference type where a value object or strong ID already exists in `Aiel.Authorization.Domain.Shared`.

## Verin rejection criteria

I will reject the Tasks 5+6 attempt if any of the following are true:

- `IActionGate<TAction>` returns `IActionExecutionContext<TAction>` directly instead of `Result<IActionExecutionContext<TAction>>`.
- The gate invokes a command handler, dispatcher, or any downstream business logic before returning the execution context. The gate's only job is to gate; the caller is responsible for dispatching.
- `DefaultActionGate<TAction>` is registered as a `DefaultCommandDispatcher` pipeline behavior or decorator. The gate must be explicitly invoked, not silently composed into the dispatch pipeline.
- `PermissionErrors` defines any of its error types as exception subclasses rather than `Error` values.
- Behavioral gate tests are written against mocks of `IActionGate<TAction>` itself. Behavioral tests must exercise `DefaultActionGate<TAction>` directly with hand-written fakes of `IActionValidator<TAction>`, `IActionPermissionChecker<TAction>`, and/or `IPermissionStore`.
- `IPermissionStore` exposes EF entity types, `DbContext`, nullable column projections, or any infrastructure persistence type at the contract boundary.
- `DefaultPermissionManager` directly instantiates or injects any concrete persistence class (EF `DbContext`, Dapper connection, etc.) instead of depending solely on `IPermissionStore`.
- Task 5 contracts reference `Aiel.Authorization.Domain` internal types (aggregates, domain events) at the contract boundary instead of `Aiel.Authorization.Domain.Shared` value objects and strong IDs.
- The test project uses a mocking framework to substitute `IActionGate<TAction>` itself and then asserts on mock setup — this proves nothing about the implementation.
- The PR does not demonstrate a red-to-green path: stub `NotImplementedException` implementations that make the tests compile and fail at runtime, then full implementations that make the tests pass.
- Tasks 5 and 6 are split into separate PRs. Either both ship together with a clean, fully-passing build, or neither ships.
