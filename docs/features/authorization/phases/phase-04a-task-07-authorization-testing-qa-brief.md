# Phase 04a Task 7 — Aiel.Authorization.Testing QA Brief

- **Date:** 2026-05-27T00:00:00Z
- **Author:** Verin
- **Scope:** Task 7 only. Create `Aiel.Authorization.Testing` (src) and `Aiel.Authorization.Testing.UnitTests` (tests). No product code changes. No changes to any existing Authorization project.
- **Layer:** Testing support library. Depends inward on `Application.Contracts` and `Aiel.Testing`. Never on any concrete infrastructure or the `Application` impl assembly.
- **Baseline:** Tasks 5+6 landed and validated. 17/17 tests pass in `Aiel.Authorization.Application.UnitTests`. Worktree is dirty with uncommitted Tasks 5+6 work; Task 7 is stacked on top.

---

## Slice-boundary verdict: Task 7 is a standalone slice ✅

Task 7 can land as an independent, green slice stacked on top of the committed (or pre-committed) Tasks 5+6 work. The reasoning:

- `Aiel.Authorization.Testing` depends only on `Aiel.Authorization.Application.Contracts` and (optionally) `Aiel.Testing`. Both are present and stable.
- `Aiel.Authorization.Testing` does **not** require `Aiel.Authorization.Application` (the impl). The helpers implement and consume the **contracts** layer only; they provide fakes *of* those interfaces, not of the implementations.
- `Aiel.Authorization.Testing.UnitTests` tests only the helpers themselves. No behavioral gate or manager tests are duplicated here.
- No circular references; the clean-build rule is satisfiable within a single Task 7 PR.

Task 7 must **not** merge with Task 8 (infrastructure adapter). If it cannot be completed cleanly without implementation work, stop and surface the blocker rather than absorbing scope.

---

## Primary risk: Testing package as accidental production sample surface 🚨

This is the dominant risk for Task 7 and must be addressed architecturally, not by convention alone.

`Aiel.Authorization.Testing` is `IsPackable=true` (matching `Aiel.Testing`). Every `public` type it ships becomes part of the published NuGet API. Unlike the `private sealed class TestAction : IAction;` fakes in Tasks 5+6 (invisible outside their test class), the helpers in this package are deliberately visible — that is their purpose.

The danger: if the package ships types with domain-semantic names, downstream consumers will copy them as starting points or, worse, import the package into production code to get "sample" permission names and action types. That turns the testing package into an accidental blueprint document, not just a helper.

### Non-negotiable naming rules

All fixture types shipped in `Aiel.Authorization.Testing` MUST obey these rules:

| What | Allowed | REJECT |
| --- | --- | --- |
| Action fixture types | `PermissionTestAction`, `FakeAction`, `StubAction` | `ReadDocumentsAction`, `CreateOrderAction` |
| Permission name constants | `PermissionTestData.ValidPermissionName`, `PermissionTestData.AltPermissionName` | `PermissionTestData.DocumentsRead`, `PermissionTestData.TenantAdmin` |
| Subject key constants | `PermissionTestKeys.SubjectKey`, `PermissionTestKeys.AltSubjectKey` | `PermissionTestKeys.UserId`, `PermissionTestKeys.AdminId` |
| Scope key constants | `PermissionTestKeys.ScopeKey`, `PermissionTestKeys.AltScopeKey` | `PermissionTestKeys.TenantId` |
| Class names | `FakePermissionStore`, `FakeExecutionContextFactory`, `PermissionTestData` | `SamplePermissionStore`, `ExampleAuthorization` |

Fixture action types MUST be declared in the `Aiel.Authorization.Testing` namespace (or a clearly test-only sub-namespace). They MUST implement only `IAction` and MUST NOT carry properties that model real domain concepts.

Stable IDs used in fixtures MUST use `PermissionStableId.From(string)` with values that begin with the prefix `perm_test_` or a similarly unambiguous test sentinel, so they can never be confused with IDs generated in real migration scripts.

---

## Required projects and their dependencies

### `Aiel/src/Aiel.Authorization.Testing/Aiel.Authorization.Testing.csproj`

| Allowed references | Forbidden references |
| --- | --- |
| `Aiel.Authorization.Application.Contracts` | `Aiel.Authorization.Application` (the impl) |
| `Aiel.Testing` | `Aiel.EntityFrameworkCore` |
| `Aiel.Results` (if not transitively available) | `Aiel.AspNetCore` or any HTTP package |
| xUnit v3 extensibility (if needed for assertion helpers) | Any mocking framework (Moq, NSubstitute, etc.) |

The package is `IsPackable=true`. It MUST carry XML documentation comments on every public type and member.

### `Aiel/tests/Aiel.Authorization.Testing.UnitTests/Aiel.Authorization.Testing.UnitTests.csproj`

References `Aiel.Authorization.Testing`. Does NOT reference `Aiel.Authorization.Application` directly.

---

## Required helper surface

The following public types MUST exist in `Aiel.Authorization.Testing` in the `Aiel.Authorization` namespace:

| Type | Kind | Purpose |
| --- | --- | --- |
| `FakePermissionStore` | `class` | In-memory, non-null `IPermissionStore` implementation. Records all calls; returns configurable results. All public properties are non-null. |
| `FakePermissionDefinitionRegistry` | `class` | In-memory `IPermissionDefinitionRegistry`. Populated via constructor or factory. Returns non-null lists. |
| `FakeExecutionContextFactory` | `static class` | Creates valid, non-null `IActionExecutionContext<TAction>` instances for use in gate and checker tests. |
| `PermissionTestData` | `static class` | Named constants for `PermissionName`, `PermissionStableId`, `PermissionScopeTypeName`, `PermissionSubjectTypeName`. Names are opaque test sentinels, not domain names. |
| `PermissionTestKeys` | `static class` | Named constants for `PermissionSubjectKey` and `PermissionScopeKey`. |
| `PermissionTestAction` | `sealed class` | Minimal `IAction` implementation for use as a fixture action in gate/checker tests. Carries no domain properties. |

The `FakePermissionStore` MUST expose call-recording lists (`IReadOnlyList<T>` or similar) as non-null public properties so callers can assert on interaction counts and arguments without coupling to a mocking framework.

---

## Focused acceptance gate

Task 7 is NOT done until ALL of the following are true:

1. `dotnet test .\Aiel\Aiel.slnx --nologo --tl:off -v minimal` passes cleanly. All 17 Tasks 5+6 tests still pass. New `Aiel.Authorization.Testing.UnitTests` tests also pass.
2. `Aiel.Authorization.Testing.csproj` does NOT reference `Aiel.Authorization.Application` or any infrastructure package.
3. Every public property on every public type in `Aiel.Authorization.Testing` is non-nullable and returns a valid non-default value (verified by dedicated surface tests described below).
4. Every fixture action type and every constant in `PermissionTestData` and `PermissionTestKeys` uses an opaque test-sentinel name (no domain business names).
5. `FakePermissionStore.CreateGrantAsync`, `RevokeGrantAsync`, and `GetGrantsForSubjectAsync` record their arguments in non-null, publicly accessible lists.
6. `FakePermissionDefinitionRegistry.GetAll()` returns a non-null list (possibly empty).
7. `FakePermissionDefinitionRegistry.TryGet` and `TryGetForAction<TAction>` never set the `out` parameter to null when returning `true`.
8. All new projects are registered in `TwoRivers.slnx`, `Aiel/Aiel.slnx`, and `Aiel/virtual-folders.json`.
9. Build is clean: no warnings, no nullable annotation suppressions, no `#pragma warning disable`.

---

## Red-first test path

Because this is a testing-support package rather than a behavioral implementation, the red-first path is narrower but still required:

**Phase 1 — stubs (tests RED):**

- Create both projects.
- Add public stubs: `FakePermissionStore` with all interface methods throwing `NotImplementedException`; `PermissionTestData` and `PermissionTestKeys` with placeholder `default` values; `FakeExecutionContextFactory` returning `default!`.
- Write `PermissionTestingHelperTests` with the tests listed below.
- Run: all tests fail (NotImplementedException or assertion failures on default/null).

**Phase 2 — implement helpers (tests GREEN):**

- Implement `FakePermissionStore` call-recording.
- Implement `PermissionTestData`, `PermissionTestKeys` with valid non-default values.
- Implement `FakeExecutionContextFactory`.
- Run: all tests pass.

---

## Required tests in `Aiel.Authorization.Testing.UnitTests`

### `FakePermissionStoreTests`

1. `CreateGrantAsync_RecordsCallArguments` — calls `CreateGrantAsync`, asserts `CreateGrantCalls` has one entry with matching arguments.
2. `GetGrantsForSubjectAsync_RecordsLookup` — calls `GetGrantsForSubjectAsync`, asserts `GetGrantsCalls` has one entry.
3. `RevokeGrantAsync_RecordsGrantId` — calls `RevokeGrantAsync`, asserts `RevokeGrantCalls` has one entry with matching ID.
4. `AllCallListProperties_AreNonNull_BeforeAnyCall` — creates a new `FakePermissionStore`; asserts `CreateGrantCalls`, `GetGrantsCalls`, and `RevokeGrantCalls` are not null and are empty.

### `FakePermissionDefinitionRegistryTests`

1. `GetAll_WithNoRegistrations_ReturnsEmptyNonNullList`.
2. `TryGet_WhenRegistered_ReturnsTrueAndNonNullManifest`.
3. `TryGetForAction_WhenRegistered_ReturnsTrueAndNonNullManifest`.

### `PermissionTestDataTests`

1. `AllPermissionNames_AreNonDefault` — each `PermissionName` constant on `PermissionTestData` is not equal to `default(PermissionName)`.
2. `AllStableIds_AreNonDefault` — each `PermissionStableId` constant is not equal to `default(PermissionStableId)`.
3. `AllConstants_AreDistinct` — all `PermissionName` constants on `PermissionTestData` are pairwise distinct (no duplicate values).

### `PermissionTestKeysTests`

1. `AllSubjectKeys_AreNonDefault`.
2. `AllScopeKeys_AreNonDefault`.
3. `AllKeys_AreDistinct`.

### `FakeExecutionContextFactoryTests`

1. `Create_ReturnsNonNullContext`.
2. `Create_ContextActionIsAssigned`.

---

## Verin rejection criteria

I will reject the Task 7 attempt if any of the following are true:

- `Aiel.Authorization.Testing.csproj` carries a reference to `Aiel.Authorization.Application` (the impl). The package must be coupled to contracts only.
- Any fixture action type has a name that describes a real domain concept (`ReadDocumentAction`, `CreateTenantAction`, etc.).
- Any constant in `PermissionTestData` or `PermissionTestKeys` uses a domain-semantic name (`DocumentsRead`, `TenantAdminRole`, `UserId`).
- Any `PermissionStableId` value in the testing package does NOT begin with a clearly non-production prefix (e.g., `perm_test_`).
- Any public property on a public helper type is declared nullable (`?`) or can return `null` at runtime.
- `FakePermissionStore` uses a mocking framework internally (its implementation must be hand-written).
- `FakePermissionStore` exposes mutable lists as `List<T>` instead of read-only views (`IReadOnlyList<T>`).
- Tests in `Aiel.Authorization.Testing.UnitTests` reference `Aiel.Authorization.Application` (the impl) directly. If interaction with the gate or manager is needed, use the inline fakes from that project's own test file or duplicate them locally; do not create a cross-project dependency.
- The package does not carry XML documentation on public types.
- The slice is merged with Task 8 (infrastructure adapter) to avoid having to make the testing helpers standalone.
- The build has any warnings at solution level after this change lands.
