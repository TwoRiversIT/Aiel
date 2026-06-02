# Phase 04a Task 10 — Aiel.Authorization EF Core Store & Migration DSL QA Brief

- **Date:** 2026-06-01T00:00:00Z
- **Author:** Verin
- **Scope:** Task 10 only. Add `Aiel.Authorization.EntityFrameworkCore` (provider-neutral store + migration DSL), `Aiel.Authorization.EntityFrameworkCore.PostgreSql` (provider registration adapter), and `Aiel.Authorization.EntityFrameworkCore.IntegrationTests`. Implement the first three migration DSL operations: `Add`, `Rename`, `Deprecate`. Extend `Aiel.Authorization.Testing` with `ChangeAppointmentTestAction` and `RescheduleAppointmentTestAction`. Fix the `PermissionDefinitionManifest` mismatch carried forward from Task 9.
- **Layer:** Infrastructure adapter (`I` in the architecture key). `Aiel.Authorization.EntityFrameworkCore` depends inward on `Application.Contracts` and `Domain`. No business logic in the adapter. The migration DSL is a compile-time / startup-time declaration surface — NOT an EF Core `IMigration` subclass hierarchy.
- **Baseline:** Task 9 committed (generator + analyzer integration). All existing tests pass. **Two Task 9 residuals are unresolved** (see §Pre-conditions); they must be fixed as the first changeset of Task 10.

---

## Slice-boundary verdict: Task 10 is a standalone slice ✅ — with required pre-conditions

Task 10 can land independently before Task 11 (`RescheduleAppointment` reference slice). All rename test scenarios use fixture action types from `Aiel.Authorization.Testing` — there is no dependency on the production `RescheduleAppointment : ICommand` introduced in Task 11.

**Decision D6 compliance:** The EF Core store is an infrastructure adapter. `IPermissionStore` (the port) is already defined in `Aiel.Authorization.Application.Contracts`. Task 10 provides the first real implementation of that port.

**Explicit boundary — what Task 10 MUST NOT absorb:**

- ASP.NET Core middleware, endpoint filters, or HTTP-layer permission enforcement.
- The Roslyn generator, source-generated permission constants, or snapshot semantics (Task 9 scope, already landed).
- Any `RescheduleAppointment : ICommand` or other Task 11 production contracts.
- EF Core CLI migration tooling, `dotnet-ef` migration files, or `MigrationBuilder`-based DDL scripts — those are infrastructure ops concerns, not application architecture.
- Production-quality `IPermissionDefinitionRegistry` — the integration tests wire manifests by hand; full registry integration is deferred.

Task 10 MUST NOT merge with any task that touches the application service layer, the Roslyn generator pipeline, or production feature code.

---

## ⚠️ Critical pre-conditions — Task 9 residuals (fix before EF Core work begins)

Two bugs were introduced during Task 9 implementation where the code diverged from the QA brief specification. Both will cause compile-time failures in the Task 10 integration test project, which uses the real manifest type and the generator's emitted output.

### Pre-condition 1 — `PermissionDefinitionManifest` property name mismatch

**Current state (`PermissionDefinitionManifest.cs`):**
```csharp
public required PermissionName PermissionName { get; init; }
```

**Generator emits:**
```csharp
Name = global::Aiel.Authorization.PermissionName.From("..."),
```

The generator initializes property `Name`; the manifest declares property `PermissionName`. This is a compile-time mismatch. **Fix:** rename the manifest property from `PermissionName` to `Name`. Update all callsites (there are few — the type is new).

### Pre-condition 2 — `PermissionDefinitionManifest` missing `Lifecycle` and `PreviousNames`

**Current state:** `PermissionDefinitionManifest` has no `Lifecycle` or `PreviousNames` properties.

**Generator emits:**
```csharp
Lifecycle = global::Aiel.Authorization.PermissionLifecycle.Active,
PreviousNames = [],
```

The manifest cannot receive these initializers. **Fix:** add both properties additively (no `required`):

```csharp
/// <summary>Gets the lifecycle state of this permission definition. Defaults to <see cref="PermissionLifecycle.Active"/>.</summary>
public PermissionLifecycle Lifecycle { get; init; } = PermissionLifecycle.Active;

/// <summary>Gets the previous canonical names this permission was known by, in declaration order.</summary>
/// <remarks>
/// Populated via <see cref="DefinesPermissionAttribute.PreviousNames"/> when renaming a permission.
/// The EF Core migration DSL reads this to find and update existing grants after a rename.
/// </remarks>
public IReadOnlyList<PermissionName> PreviousNames { get; init; } = [];
```

Note: the generator already emits `PreviousNames` as a `string[]` array initializer (`PreviousNames = [...]`). The manifest property must be `IReadOnlyList<PermissionName>`, which means the generator must emit individual `PermissionName.From(...)` calls rather than raw strings. Verify the generator's `EmitAggregates` output handles this correctly — if not, the generator must be updated simultaneously.

### Pre-condition 3 — `ChangeAppointmentTestAction` and `RescheduleAppointmentTestAction` fixture types

`PermissionFixtureActions.cs` currently only contains `AlphaTestAction`, `BetaTestAction`, `GammaTestAction`. The rename migration test requires two fixture action types representing the "before" and "after" permission names.

**Fix:** add to `PermissionFixtureActions.cs` in `Aiel.Authorization.Testing`:

```csharp
/// <summary>
/// Fixture action representing a "before-rename" permission. Not a production contract.
/// </summary>
/// <remarks>Used in migration DSL tests to represent a permission that will be renamed to <see cref="RescheduleAppointmentTestAction"/>.</remarks>
public sealed class ChangeAppointmentTestAction : IAction;

/// <summary>
/// Fixture action representing an "after-rename" permission. Not a production contract.
/// </summary>
/// <remarks>Used in migration DSL tests to represent a permission renamed from <see cref="ChangeAppointmentTestAction"/>.</remarks>
public sealed class RescheduleAppointmentTestAction : IAction;
```

These are test-only fixture types. They MUST NOT reference, depend on, or be confused with any production `RescheduleAppointment : ICommand` introduced in Task 11.

---

## New projects

| Project | Path | Role |
|---|---|---|
| `Aiel.Authorization.EntityFrameworkCore` | `Aiel/src/` | Provider-neutral EF Core store implementation + migration DSL |
| `Aiel.Authorization.EntityFrameworkCore.PostgreSql` | `Aiel/src/` | PostgreSQL provider registration adapter |
| `Aiel.Authorization.EntityFrameworkCore.IntegrationTests` | `Aiel/tests/` | Integration tests; uses real PostgreSQL via Testcontainers |

All three MUST be registered in `TwoRivers.slnx`, `Aiel/Aiel.slnx`, and `Aiel/virtual-folders.json`.

### Allowed and forbidden dependencies

| Project | Allowed | Forbidden |
|---|---|---|
| `Aiel.Authorization.EntityFrameworkCore` | `Aiel.Authorization.Application.Contracts`, `Aiel.Authorization.Domain`, `Aiel.Authorization.Domain.Shared`, `Aiel.StrongIds.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore` (provider-neutral), `Aiel.Common` | Any `Npgsql.*`, any presentation layer, `Aiel.Authorization.Application` (service implementations) |
| `Aiel.Authorization.EntityFrameworkCore.PostgreSql` | `Aiel.Authorization.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL` | Direct `Aiel.Authorization.Domain` reference beyond what flows transitively, any business-logic code |
| `Aiel.Authorization.EntityFrameworkCore.IntegrationTests` | `Aiel.Authorization.EntityFrameworkCore`, `Aiel.Authorization.EntityFrameworkCore.PostgreSql`, `Aiel.Authorization.Testing`, `Aiel.Authorization.Application`, `Aiel.Testing`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Testcontainers.PostgreSql`, xUnit, FluentAssertions | `Aiel.Authorization.Generators` (test code must not depend on generator internals), any presentation layer |

---

## EF Core mapping surface — minimum for Task 10

The persistence entity (`PermissionGrantRecord`) is an **internal infrastructure type**. It MUST NOT appear in any signature in `Aiel.Authorization.Application.Contracts`, `Aiel.Authorization.Domain`, or any public API.

### `PermissionGrantRecord` entity shape

| Column name | DB type (PostgreSQL) | CLR type | Strong-ID converter |
|---|---|---|---|
| `PermissionGrantId` | `uuid` (PK) | `Guid` | `PermissionGrantId` → `Guid` via `Aiel.StrongIds.EntityFrameworkCore` |
| `PermissionName` | `varchar(256)` NOT NULL | `string` | `PermissionName` → `string` value converter |
| `ScopeTypeName` | `varchar(128)` NOT NULL | `string` | `PermissionScopeTypeName` → `string` value converter |
| `ScopeKey` | `varchar(512)` NOT NULL | `string` | `PermissionScopeKey` → `string` value converter |
| `SubjectTypeName` | `varchar(128)` NOT NULL | `string` | `PermissionSubjectTypeName` → `string` value converter |
| `SubjectKey` | `varchar(512)` NOT NULL | `string` | `PermissionSubjectKey` → `string` value converter |
| `Decision` | `integer` NOT NULL | `int` | `AuthorizationGrantDecision` enum cast (Granted=0, Prohibited=1) |
| `GrantedAt` | `timestamp with time zone` NOT NULL | `DateTimeOffset` | None needed |

**Deferred:** `RevokedAt` column (soft-delete / audit trail) — not needed for Task 10 test gates. Flag for Task 13 (hardening).

### Index requirements

- Composite index on `(SubjectTypeName, SubjectKey)` for `GetGrantsForSubjectAsync` query performance.
- Composite index on `(PermissionName, ScopeTypeName, ScopeKey, SubjectTypeName, SubjectKey)` for grant lookup in `IPermissionGrantEvaluator`.

---

## Migration DSL surface

The migration DSL is a **compile-time / startup-time declaration surface**. Operations describe intent; they are NOT EF Core `IMigration` subclasses and they do NOT execute raw SQL during test teardown.

### Interfaces and types — in `Aiel.Authorization.EntityFrameworkCore`

```csharp
/// <summary>A single permission definition migration operation.</summary>
public interface IPermissionMigrationOperation { }

/// <summary>Records the addition of a new permission definition to the store.</summary>
public sealed class AddPermissionOperation : IPermissionMigrationOperation
{
    public required PermissionName Name { get; init; }
}

/// <summary>Renames a permission definition, preserving all existing grants.</summary>
public sealed class RenamePermissionOperation : IPermissionMigrationOperation
{
    public required PermissionName From { get; init; }
    public required PermissionName To { get; init; }
}

/// <summary>Marks a permission definition as deprecated without removing existing grants.</summary>
public sealed class DeprecatePermissionOperation : IPermissionMigrationOperation
{
    public required PermissionName Name { get; init; }
}
```

**All parameters are strongly typed `PermissionName` value objects.** Raw `string` parameters are forbidden. The `From` and `To` type on `RenamePermissionOperation` MUST be `PermissionName`, not `string`.

### Migration runner — in `Aiel.Authorization.EntityFrameworkCore`

```csharp
/// <summary>Applies a sequence of permission migration operations against the permission store.</summary>
public interface IPermissionMigrationRunner
{
    Task<Result> ApplyAsync(
        IEnumerable<IPermissionMigrationOperation> operations,
        CancellationToken cancellationToken = default);
}
```

The EF Core implementation (`EfCorePermissionMigrationRunner`) MUST:

- Apply `AddPermissionOperation` as a no-op if the permission name already exists in the store snapshot (idempotent).
- Apply `RenamePermissionOperation` by updating the `PermissionName` column on all matching `PermissionGrantRecord` rows — the `PermissionGrantId` (PK) MUST be preserved unchanged.
- Apply `DeprecatePermissionOperation` by updating the lifecycle state in the manifest snapshot (not in the grants table — grants are immutable by lifecycle state).
- Wrap all operations in a transaction. Partial success is not permitted.
- Return `Result.Failure(...)` on any storage error, not throw.

---

## Authoritative acceptance gates

All four gates must pass before Task 10 may be declared done and submitted for review.

### Gate 1 — Round-trip fidelity

Create a permission grant via `EfCorePermissionStore.CreateGrantAsync`. Read it back via `GetGrantsForSubjectAsync`. Assert:

1. The returned `PermissionGrantSummary` contains the same `PermissionGrantId` as the create response.
2. `PermissionName` on the summary matches the name passed to `CreateGrantAsync`.
3. `ScopeType`, `ScopeKey`, `SubjectType`, `SubjectKey` all round-trip without mutation.
4. `Decision` is `AuthorizationGrantDecision.Granted`.

This test MAY use the EF InMemory provider (schema accuracy is not the subject here).

### Gate 2 — Rename preserves existing grants

Create grants for permission `ChangeAppointmentTestAction`'s name. Run `RenamePermissionOperation` from the old name to `RescheduleAppointmentTestAction`'s name. Assert:

1. `GetGrantsForSubjectAsync` with the **new** permission name returns the same grants.
2. `GetGrantsForSubjectAsync` with the **old** permission name returns an empty collection.
3. The `PermissionGrantId` values are identical before and after rename (no re-creation).
4. The `PermissionStableId` in the registered manifest entry for the new name matches the stable ID that was registered for the old name (stable ID preserved across rename).

This test MUST use the real PostgreSQL provider (Testcontainers). The InMemory provider ignores column-level updates and would give a false-positive result.

### Gate 3 — Manifest snapshot carries `PreviousNames` after rename

After applying `RenamePermissionOperation` (old → new), retrieve the `PermissionDefinitionManifest` for the new permission name from `IPermissionDefinitionRegistry`. Assert:

1. `Manifest.PreviousNames` contains the old `PermissionName` value.
2. `Manifest.Name` equals the new `PermissionName`.
3. `Manifest.Lifecycle` is `PermissionLifecycle.Active`.

### Gate 4 — Null-safe collection return

Call `GetGrantsForSubjectAsync` for a subject that has no grants. Assert:

1. The result is `Result.Success(...)`.
2. The returned collection is empty — not `null`, not a failed result.
3. `collection.Count == 0`.

---

## First red tests — method stubs

These tests must be **written first** and fail before implementation begins. All names are in `Aiel.Authorization.EntityFrameworkCore.IntegrationTests`.

```csharp
// Gate 1 — Round-trip
[Fact]
public async Task EfCorePermissionStore_CreateGrant_RoundTrips_AllFields()
{
    // Arrange
    var store = /* resolve EfCorePermissionStore from test fixture */;
    var subjectKey = PermissionSubjectKey.From("user-001");
    var subjectType = PermissionSubjectTypeName.From("User");
    var permissionName = PermissionName.From("testing.alpha-test-action");
    var scopeType = PermissionScopeTypeName.From("Location");
    var scopeKey = PermissionScopeKey.From("loc-001");

    // Act — create
    var createResult = await store.CreateGrantAsync(
        permissionName, scopeType, scopeKey, subjectType, subjectKey,
        AuthorizationGrantDecision.Granted, TestCancellation);

    // Assert — create succeeded
    createResult.IsSuccess.Should().BeTrue();
    var grantId = createResult.Value;

    // Act — retrieve
    var getResult = await store.GetGrantsForSubjectAsync(
        subjectType, subjectKey, TestCancellation);

    // Assert — round-trip
    getResult.IsSuccess.Should().BeTrue();
    var grant = getResult.Value.Should().ContainSingle().Subject;
    grant.PermissionGrantId.Should().Be(grantId);
    grant.Name.Should().Be(permissionName);
    grant.ScopeType.Should().Be(scopeType);
    grant.ScopeKey.Should().Be(scopeKey);
    grant.Decision.Should().Be(AuthorizationGrantDecision.Granted);
}

// Gate 2 — Rename preserves grants (PostgreSQL required)
[Fact]
public async Task RenamePermission_Preserves_ExistingGrants_AndPermissionGrantIds()
{
    // Arrange
    var store = /* resolve EfCorePermissionStore using real Npgsql provider */;
    var runner = /* resolve EfCorePermissionMigrationRunner */;
    var subjectKey = PermissionSubjectKey.From("user-rename-001");
    var subjectType = PermissionSubjectTypeName.From("User");
    var oldName = PermissionName.From("testing.change-appointment");
    var newName = PermissionName.From("testing.reschedule-appointment");
    var scopeType = PermissionScopeTypeName.From("Location");
    var scopeKey = PermissionScopeKey.From("loc-rename-001");

    var createResult = await store.CreateGrantAsync(
        oldName, scopeType, scopeKey, subjectType, subjectKey,
        AuthorizationGrantDecision.Granted, TestCancellation);
    var originalGrantId = createResult.Value;

    // Act
    var renameResult = await runner.ApplyAsync(
        [new RenamePermissionOperation { From = oldName, To = newName }],
        TestCancellation);

    // Assert
    renameResult.IsSuccess.Should().BeTrue();

    var byNewName = await store.GetGrantsForSubjectAsync(subjectType, subjectKey, TestCancellation);
    byNewName.IsSuccess.Should().BeTrue();
    var grant = byNewName.Value.Should().ContainSingle().Subject;
    grant.Name.Should().Be(newName);
    grant.PermissionGrantId.Should().Be(originalGrantId); // ID preserved

    var byOldName = await store.GetGrantsForSubjectAsync(subjectType, subjectKey, TestCancellation);
    // After rename, querying by old name should return nothing
    // NOTE: this requires the store to filter by PermissionName; the old name no longer exists in the DB
    byOldName.IsSuccess.Should().BeTrue();
    byOldName.Value.Should().BeEmpty();
}

// Gate 3 — Manifest PreviousNames after rename
[Fact]
public async Task RenamePermission_ManifestSnapshot_ContainsPreviousName()
{
    // Arrange
    var runner = /* resolve EfCorePermissionMigrationRunner */;
    var registry = /* resolve IPermissionDefinitionRegistry */;
    var oldName = PermissionName.From("testing.change-appointment");
    var newName = PermissionName.From("testing.reschedule-appointment");

    // Act
    await runner.ApplyAsync(
        [new RenamePermissionOperation { From = oldName, To = newName }],
        TestCancellation);

    // Assert
    var manifest = registry.GetManifest(newName);
    manifest.IsSuccess.Should().BeTrue();
    manifest.Value.Name.Should().Be(newName);
    manifest.Value.PreviousNames.Should().Contain(oldName);
}

// Gate 4 — Null-safe empty collection
[Fact]
public async Task GetGrantsForSubjectAsync_WhenNoGrantsExist_ReturnsEmptyCollection_NotNull()
{
    // Arrange
    var store = /* resolve EfCorePermissionStore */;
    var subjectKey = PermissionSubjectKey.From("user-no-grants");
    var subjectType = PermissionSubjectTypeName.From("User");

    // Act
    var result = await store.GetGrantsForSubjectAsync(subjectType, subjectKey, TestCancellation);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Should().BeEmpty();
}

// Pre-condition check — PermissionGrantRecord must not appear in public contract surface
[Fact]
public void PermissionGrantRecord_IsNotExposedInApplicationContracts()
{
    var contractsAssembly = typeof(IPermissionStore).Assembly;
    var efAssembly = typeof(EfCorePermissionStore).Assembly;
    var efTypes = efAssembly.GetTypes()
        .Where(t => t.Name.Contains("Record") || t.Name.Contains("Entity"))
        .ToHashSet();

    foreach (var contractType in contractsAssembly.GetExportedTypes())
    {
        contractType.GetMembers()
            .SelectMany(m => GetTypeReferences(m))
            .Should().NotContain(t => efTypes.Contains(t),
                because: "EF Core record types must not leak into application contracts");
    }
}
```

**Note on Gate 2 dual-query assertion:** After a rename, the EF store filters by `PermissionName` column value. Once the column value has been updated from `testing.change-appointment` to `testing.reschedule-appointment`, a query scoped to the old name legitimately returns empty. Verify the query does NOT use an in-memory cache that would serve stale old-name results.

---

## PostgreSQL test infrastructure — recommended approach

Task 10 integration tests that exercise real schema behavior (Gate 2: rename migration, index correctness) MUST use a real PostgreSQL provider. Recommend **Testcontainers** (`Testcontainers.PostgreSql` NuGet package) to avoid requiring a locally installed PostgreSQL instance in CI.

**Open question for Doug:** Is `Testcontainers.PostgreSql` an approved external dependency? It introduces a Docker dependency in CI. If Testcontainers is not available, the alternative is a connection string supplied via environment variable / test configuration — but this is less hermetic. Flag before adding the package reference (rule AA1 applies: new external dependency requires explicit approval).

**If Testcontainers is approved:** The test fixture base class should inherit from `Aiel.Testing.IntegrationTestFixture` and configure the PostgreSQL container in `ConfigureServices`, following the pattern established in `Aiel.EntityFrameworkCore.IntegrationTests`.

**If Testcontainers is NOT approved:** Tests that require real PostgreSQL MUST be decorated with `[Trait("Category", "RequiresPostgres")]` and skipped automatically when no connection string is configured. The Gate 2 test MUST be skipped in this case — not silently passing on an InMemory provider.

---

## Reject conditions

| # | Condition | Severity |
|---|---|---|
| R1 | `PermissionGrantRecord`, `PermissionDbContext`, or any EF Core entity type appears in a public member of `Aiel.Authorization.Application.Contracts` or `Aiel.Authorization.Domain` | **BLOCK** |
| R2 | `PermissionDefinitionManifest` still lacks `Lifecycle` or `PreviousNames` when Task 10 is submitted | **BLOCK** |
| R3 | Generator emits `Name =` but manifest property is still named `PermissionName` (compile failure in integration tests) | **BLOCK** |
| R4 | Rename migration test (`Gate 2`) uses EF InMemory provider instead of real PostgreSQL or Testcontainers | **BLOCK** |
| R5 | `RenamePermissionOperation.From` or `.To` accepts a raw `string` instead of `PermissionName` | **BLOCK** |
| R6 | `PermissionGrantId` (PK) is changed or regenerated after a rename migration | **BLOCK** |
| R7 | `GetGrantsForSubjectAsync` returns `null` instead of an empty `IReadOnlyList<PermissionGrantSummary>` | **BLOCK** |
| R8 | Task 11's `RescheduleAppointment : ICommand` is referenced by the integration test project or migration DSL fixtures | **BLOCK** |
| R9 | PostgreSQL-specific configuration (column types, Npgsql-specific calls) in `Aiel.Authorization.EntityFrameworkCore` (must be in `.PostgreSql`) | **BLOCK** |
| R10 | `IPermissionMigrationRunner` or any migration DSL type defined in `Aiel.Authorization.Application.Contracts` or `Aiel.Authorization.Application` (belongs in the infrastructure adapter layer) | **BLOCK** |
| R11 | Migration DSL operations implemented as EF Core `IMigration` subclasses or `MigrationBuilder`-based DDL scripts | **REJECT** |
| R12 | `PermissionStableId` recomputed or changed on rename (stable ID must survive permission name changes) | **BLOCK** |
| R13 | New external package added without Doug approval (especially `Testcontainers.PostgreSql`) | **HOLD** |

---

## Implementation notes — known drift between Task 9 brief spec and actual code

The following divergences were observed between the Task 9 QA brief specification and the committed implementation. They are documented here for awareness — they are not newly introduced bugs, but they affect what Task 10 implementors will encounter:

| Brief spec | Actual implementation | Impact on Task 10 |
|---|---|---|
| Attribute named `[DefinePermission]` | Attribute named `[DefinesPermission]` | Harmless; naming consistent within Task 9. Task 10 just uses `DefinesPermissionAttribute` by its actual name. |
| `PreviousNames` in attribute is semicolon-delimited `string` | `PreviousNames` in attribute is `string[]` | Generator parses array; manifest must accept `IReadOnlyList<PermissionName>`. Verify the generator emits `PermissionName.From(...)` calls, not raw strings, in the `PreviousNames = [...]` initializer. |
| `DefinesPermissionAttribute.StableId` not in brief | `StableId` property present in actual attribute | No conflict; harmless additive property. |
| Manifest `PermissionName` property | Generator emits `Name =` | **Pre-condition 1** — must be fixed. |
| Manifest missing `Lifecycle`, `PreviousNames` | Generator emits both | **Pre-condition 2** — must be fixed. |

---

## Related decisions

- **D1–D3:** `IPermissionStore`, `IPermissionManager`, `IPermissionGrantEvaluator` are application contracts. EF Core implementation lives strictly in the infrastructure layer.
- **D4:** Strong IDs are non-negotiable. All ID columns use value converters via `Aiel.StrongIds.EntityFrameworkCore`.
- **D5:** `PermissionDefinitionManifest` shape is now stable. Task 10 extends it only with the pre-condition fixes from Task 9.
- **D6:** EF Core is the first-class persistence adapter for the permission system.
- **D7:** Rename semantics are grant-preserving. No grant rows are deleted; only the `PermissionName` column value is updated in-place.
- **D8:** `PermissionStableId` is immutable after initial assignment. The stable ID MUST survive all rename, deprecate, and lifecycle operations.
