# Phase 04a Task 9 — Aiel.Authorization.Generators QA Brief

- **Date:** 2026-05-30T00:00:00Z
- **Author:** Verin
- **Scope:** Task 9 only. Add `Aiel.Authorization.Generators` (src) and `Aiel.Authorization.Generators.UnitTests` (tests). Add `[DefinePermission]` and `[GeneratedPermission]` to `Aiel.Authorization.Application.Contracts`. Extend `PermissionDefinitionManifest` with `Lifecycle` and `PreviousNames`. Extend `ActionAuthorizationAnalyzer` with the third passing condition. Wire the generator DLL into `Aiel.Authorization.Application.Contracts`.
- **Layer:** Roslyn source generator (compile-time only) + application contract extensions + analyzer extension. No EF Core. No persistence. No runtime service registration beyond the emitted registration helper.
- **Baseline:** Task 8 committed and all tests passing. `ActionAuthorizationAnalyzer` fires `AIEL20001` on any concrete `IAction` with no checker and no `[DoesNotRespectAuthority]`. Task 9 closes the explicit gap documented in that diagnostic.

---

## Slice-boundary verdict: Task 9 is a standalone slice ✅ — no EF/persistence coupling permitted

Task 9 can land independently before any EF schema, migration tooling, or persistence infrastructure work. The generator produces pure C# source code. Its output — permission constants, manifest items, and a registration helper — exists entirely at compile-time and application startup. No database schema is involved.

**Decision D5 compliance:** The manifest item shape is now stable (`PermissionDefinitionManifest` exists; this task extends it additively). Task 9 is the correct moment to build the generator per the phase plan.

**Explicit boundary — what Task 9 MUST NOT absorb:**

- EF Core `DbContext` modifications or migration files.
- Persistence of permission grants, audit logs, or capability snapshots.
- ASP.NET Core middleware or endpoint filters.
- Anything in `Aiel.Authorization.Infrastructure.*`.

Task 9 MUST NOT merge with any task that touches persistence or the application service layer beyond what is strictly required to wire the generator DLL and extend the manifest contract.

---

## New attributes in Task 9 scope

### `[DefinePermission]` — generator input signal

**Where:** `Aiel.Authorization.Application.Contracts`, namespace `Aiel.Authorization`.

Applied by the developer to a concrete `IAction` class to instruct the generator to emit a permission definition for it.

**Shape (contract definition — not implementation):**

```csharp
/// <summary>
/// Marks a concrete <see cref="Aiel.Actions.IAction"/> for source-generated permission definition.
/// </summary>
/// <remarks>
/// The generator emits a constants class, a manifest registration helper, and a
/// <c>[GeneratedPermission]</c>-annotated marker class for the annotated action type.
/// The <see cref="ActionAuthorizationAnalyzer"/> recognizes the generated marker and suppresses
/// <c>AIEL20001</c> for the annotated action.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DefinePermissionAttribute : Attribute
{
    /// <summary>Gets the dot-delimited permission name (e.g., <c>"scheduling.reschedule-appointment"</c>).</summary>
    public required string Name { get; init; }

    /// <summary>Gets the scope type name (e.g., <c>"Organization"</c>).</summary>
    public required string ScopeType { get; init; }

    /// <summary>Gets the subject type name (e.g., <c>"User"</c>).</summary>
    public required string SubjectType { get; init; }

    /// <summary>Gets a human-readable display name for this permission.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Gets an optional description of what this permission allows or denies.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the lifecycle of this permission. Defaults to <see cref="PermissionLifecycle.Active"/>.</summary>
    public PermissionLifecycle Lifecycle { get; init; } = PermissionLifecycle.Active;

    /// <summary>Gets a semicolon-delimited list of previous permission names, used for rename tracking.</summary>
    /// <remarks>
    /// When a permission is renamed, add the old name here. The stable ID is preserved from the snapshot.
    /// </remarks>
    public string PreviousNames { get; init; } = string.Empty;
}
```

**Rules:**

- `AttributeUsage` MUST be `AttributeTargets.Class` only.
- `AllowMultiple = false` — applying twice is a compile error.
- `Name`, `ScopeType`, `SubjectType`, and `DisplayName` MUST be `required`.
- `Name` MUST conform to `PermissionName` validation (dot-delimited lowercase identifiers). The generator MUST report a warning-level diagnostic if `Name` is not valid.
- `PreviousNames` is a semicolon-delimited string rather than a `string[]` because attributes cannot have array arguments with `required` init syntax across all use cases.

### `[GeneratedPermission]` — analyzer recognition token

**Where:** `Aiel.Authorization.Application.Contracts`, namespace `Aiel.Authorization`.

Emitted by the generator on the generated constants class. Recognized by the extended `ActionAuthorizationAnalyzer` by fully qualified metadata name string — no compile-time reference from the analyzer to the contracts package.

**Shape:**

```csharp
/// <summary>
/// Applied by <c>Aiel.Authorization.Generators</c> to the generated constants class.
/// Signals to <c>ActionAuthorizationAnalyzer</c> that a permission definition exists for
/// the specified action type, suppressing <c>AIEL20001</c>.
/// </summary>
/// <remarks>
/// This attribute MUST NOT be applied manually. It is exclusively for generator output.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class GeneratedPermissionAttribute : Attribute
{
    /// <param name="actionType">The action type this generated definition covers.</param>
    public GeneratedPermissionAttribute(Type actionType) => ActionType = actionType;

    /// <summary>Gets the action type governed by this generated permission definition.</summary>
    public Type ActionType { get; }
}
```

**Rules:**

- Must NOT be `required` or have init-only properties that break the constructor pattern — the generator emits `[GeneratedPermission(typeof(TAction))]`.
- The analyzer recognizes this by metadata name `Aiel.Authorization.GeneratedPermissionAttribute` — it does NOT reference the contracts package at compile time.
- This attribute MUST NOT be applied manually. Any hand-written usage is a code smell and may produce undefined analyzer behavior.

---

## `PermissionDefinitionManifest` contract extension

The existing `PermissionDefinitionManifest` in `Aiel.Authorization.Application.Contracts` is missing `Lifecycle` and `PreviousNames`. Task 9 adds these as **non-required, non-breaking** properties with defaults:

```csharp
/// <summary>Gets the lifecycle state of this permission definition. Defaults to <see cref="PermissionLifecycle.Active"/>.</summary>
public PermissionLifecycle Lifecycle { get; init; } = PermissionLifecycle.Active;

/// <summary>Gets the list of previous permission names, used to track renames across versions.</summary>
public IReadOnlyList<PermissionName> PreviousNames { get; init; } = [];
```

**Constraints:**

- Neither property may be `required`. Making them required would break `PermissionTestData.CreateSampleManifest()`, `FakePermissionDefinitionRegistry`, and every existing test that constructs a manifest.
- All existing consumers MUST compile and pass tests without modification after this change.
- `PreviousNames` MUST return an empty collection, never `null`. This is enforced by the default initializer.

---

## Required projects and their dependencies

### `Aiel/src/Aiel.Authorization.Generators/Aiel.Authorization.Generators.csproj`

Mirror `Aiel.Generators.csproj` exactly:

| Property | Value |
|---|---|
| `TargetFramework` | `netstandard2.0` |
| `IsGenerator` | `true` |
| `IsPackable` | `true` |
| `Deterministic` | `false` ⚠️ — generators embed timestamps; this is correct and intentional |
| `IncludeBuildOutput` | `false` — DLL MUST NOT land in `lib/` |
| `IsRoslynComponent` | `true` |
| `EmitCompilerGeneratedFiles` | `true` |
| `EnforceExtendedAnalyzerRules` | `true` |
| `NoWarn` | `$(NoWarn);NU5128;RS2000` |
| DLL pack path | `analyzers/dotnet/cs` via `<None Include="$(OutputPath)$(AssemblyName).dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />` |

**Allowed package references:**

- `Microsoft.CodeAnalysis.CSharp` v5.3.0
- `Microsoft.CodeAnalysis.Analyzers` v5.3.0, `PrivateAssets="all"`

**Forbidden references:**

- Any `Aiel.*` project or package reference. The generator identifies the `[DefinePermission]` attribute by fully qualified metadata name string — no compile-time dependency on the contracts package is permitted.
- Any `net*` `TargetFramework`. Must be `netstandard2.0`.

The project MUST include `AnalyzerReleases.Shipped.md` and `AnalyzerReleases.Unshipped.md` as `<AdditionalFiles>`. It MUST import `Aiel.Roslyn.projitems`.

### Runtime package wiring — `Aiel.Authorization.Application.Contracts.csproj`

The generator DLL is delivered to consumers via `Aiel.Authorization.Application.Contracts` — the same pattern as `Aiel.Results.Generators` wired into this package. Add:

```xml
<!-- Pack the generator DLL into the contracts package's NuGet payload -->
<None Include="..\Aiel.Authorization.Generators\bin\$(Configuration)\netstandard2.0\Aiel.Authorization.Generators.dll"
      Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />

<!-- Reference the generator as an analyzer during compilation of the contracts package itself -->
<ProjectReference Include="..\Aiel.Authorization.Generators\Aiel.Authorization.Generators.csproj"
                  OutputItemType="Analyzer" ReferenceOutputAssembly="false" PrivateAssets="all" />
```

### `Aiel/tests/Aiel.Authorization.Generators.UnitTests/Aiel.Authorization.Generators.UnitTests.csproj`

Mirror `Aiel.Analyzers.UnitTests.csproj`:

| Property | Value |
|---|---|
| `TargetFramework` | `net10.0` |
| `IsPackable` | `false` |
| `IsTestProject` | `true` |

**Required package references:**

- `Microsoft.CodeAnalysis.CSharp.SourceGenerators.Testing` (same version family as `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` — currently `1.1.3`)
- `Microsoft.CodeAnalysis.CSharp.Workspaces`
- `Microsoft.CodeAnalysis.Workspaces.Common`

**Project reference:** `Aiel.Authorization.Generators` only. All Aiel type stubs are provided inline in test sources — no real `Aiel.*` assembly reference.

---

## Generator output specification

Given a concrete `IAction` class annotated with `[DefinePermission]`, the generator MUST emit the following in the annotated assembly's compilation:

### 1. Constants class

```csharp
// <auto-generated />
// Generated by Aiel.Authorization.Generators. Do not edit manually.
using Aiel.Authorization;

namespace {ActionNamespace}.Generated;

/// <summary>Generated permission constants for <see cref="{ActionType}"/>.</summary>
[GeneratedPermission(typeof({ActionType}))]
internal static class {ActionTypeSimpleName}PermissionConstants
{
    /// <summary>The stable, dot-delimited permission name.</summary>
    public const string Name = "{DefinePermission.Name}";

    /// <summary>The stable unique identifier for this permission definition.</summary>
    public const string StableId = "{derived-stable-id}";

    /// <summary>The scope type name.</summary>
    public const string ScopeType = "{DefinePermission.ScopeType}";

    /// <summary>The subject type name.</summary>
    public const string SubjectType = "{DefinePermission.SubjectType}";

    /// <summary>The human-readable display name.</summary>
    public const string DisplayName = "{DefinePermission.DisplayName}";
}
```

The `[GeneratedPermission(typeof({ActionType}))]` annotation is the analyzer recognition token.

### 2. Manifest registration helper

```csharp
// <auto-generated />
// Generated by Aiel.Authorization.Generators. Do not edit manually.
using Aiel.Authorization;
using System.Collections.Generic;

namespace {AssemblyRootNamespace}.Generated;

/// <summary>
/// Provides the generated <see cref="PermissionDefinitionManifest"/> instances for this assembly.
/// </summary>
/// <remarks>
/// Invoke <see cref="GetManifests"/> from your <c>AielDependencyConfigurator.ConfigureAsync</c> to register
/// all generated permission definitions with the <see cref="IPermissionDefinitionRegistry"/>.
/// </remarks>
internal static class GeneratedPermissionManifests
{
    /// <summary>
    /// Returns all generated <see cref="PermissionDefinitionManifest"/> instances declared in this assembly.
    /// </summary>
    public static IReadOnlyList<PermissionDefinitionManifest> GetManifests() =>
    [
        new PermissionDefinitionManifest
        {
            StableId = PermissionStableId.From("{stable-id}"),
            PermissionName = PermissionName.From("{DefinePermission.Name}"),
            ActionType = typeof({ActionType}),
            ScopeType = PermissionScopeTypeName.From("{DefinePermission.ScopeType}"),
            SubjectType = PermissionSubjectTypeName.From("{DefinePermission.SubjectType}"),
            DisplayName = "{DefinePermission.DisplayName}",
            Description = "{DefinePermission.Description}",
            Lifecycle = PermissionLifecycle.{DefinePermission.Lifecycle},
            PreviousNames = [{comma-separated PermissionName.From(...) for each previous name}],
        },
        // … one entry per [DefinePermission]-annotated action in the assembly
    ];
}
```

**Note:** `GetManifests()` is the registration contract surface — the developer calls it. How they wire the returned manifests into the DI container (e.g., adding to an `IServiceCollection`-registered implementation of `IPermissionDefinitionRegistry`) is determined by the application layer, not this generator. The generator's obligation ends at producing a callable, correct `GetManifests()`.

### 3. Stable ID semantics

The stable ID for a permission is computed on first generation and then preserved:

- **First generation (no snapshot):** The stable ID is derived deterministically as `{permission-name}` — the value of `DefinePermission.Name` itself. Because permission names are already globally unique dot-delimited identifiers, the permission name IS the stable ID initially.
- **Snapshot file:** The generator reads an optional snapshot file at `{ProjectDir}/Authorization.snapshot.json` (committed to source control). This file records the assigned stable ID for each permission name.
- **Subsequent generation (snapshot exists):** If the snapshot contains an entry for the current permission name, that ID is used unchanged. If the permission was renamed (old name appears in `DefinePermission.PreviousNames`), the snapshot is searched by old name — the previously assigned stable ID is preserved.
- **Rename-safety invariant:** Once a stable ID appears in a committed snapshot, re-running the generator MUST produce the same stable ID regardless of action type renames, namespace moves, or permission name changes (provided `PreviousNames` is populated correctly).

The snapshot file shape:

```json
{
  "version": 1,
  "entries": [
    {
      "permissionName": "scheduling.reschedule-appointment",
      "stableId": "scheduling.reschedule-appointment"
    }
  ]
}
```

The snapshot MUST be included in source control. The generator MUST treat a missing snapshot as "first run" and a corrupt/unreadable snapshot as a generator error diagnostic (not a silent fallback).

---

## Analyzer extension — third passing condition

`ActionAuthorizationAnalyzer` in `Aiel.Authorization.Analyzers` MUST be extended in Task 9 to recognize generated permission definitions. This is the primary integration gate.

### What changes

In `CollectCoveredActions` (or a new `CollectGeneratedPermissionActions` helper), the analyzer MUST additionally walk the compilation for:

- Any class carrying `[GeneratedPermissionAttribute]` (metadata name: `Aiel.Authorization.GeneratedPermissionAttribute`)
- Extracting the `ActionType` constructor argument from the attribute data
- Adding that `ITypeSymbol` to the set of covered actions

The analyzer MUST NOT reference `Aiel.Authorization.Application.Contracts` at compile time. Recognition is by metadata name string, exactly as with `DoesNotRespectAuthorityAttribute`.

### Passing-condition summary after Task 9

| Condition | Task |
|---|---|
| Concrete `IActionPermissionChecker<TAction>` in compilation | Task 8 |
| `[DoesNotRespectAuthority(Reason = "...")]` with non-empty `Reason` | Task 8 |
| Class in compilation annotated `[GeneratedPermission(typeof(TAction))]` | **Task 9** |

### Diagnostic behavior — must be unchanged

- `AIEL20001` MUST still fire for actions with none of the three conditions.
- `AIEL20001` MUST NOT fire for actions covered by the generated marker.
- `AIEL20002` behavior is entirely unchanged.
- No new diagnostic IDs are introduced in Task 9 (exception: the optional malformed-Name warning below).

### Optional diagnostic for malformed `[DefinePermission]` input

If `DefinePermission.Name` does not match `PermissionName` validation rules (dot-delimited lowercase identifiers, no empty segments), the generator SHOULD emit `TRAF01003`:

| ID | Title | Severity | Suppressible |
|---|---|---|---|
| `TRAF01003` | `DefinePermissionNameIsInvalid` | `Warning` | No |

This diagnostic is reported by the generator, not the analyzer. If added, it MUST appear in `AnalyzerReleases.Unshipped.md`.

**`TRAF01003` is OPTIONAL for Task 9.** It is listed here to reserve the ID slot and define the shape. It MUST NOT be added to the shipped analyzer releases file.

---

## Sample action for all generator tests

All generator unit tests MUST use `RescheduleAppointmentAction` (or `RescheduleAppointment`) as the specimen action — the canonical first-slice example from the phase plan. Tests MUST NOT reference real `Aiel.*` assemblies. All stubs (`IAction`, `DefinePermissionAttribute`, `PermissionDefinitionManifest`, `PermissionName`, `PermissionStableId`, etc.) are provided inline as test source strings.

---

## Focused acceptance gate

Task 9 is NOT done until ALL of the following are true:

1. `dotnet test .\Aiel\Aiel.slnx --nologo --tl:off -v minimal` passes cleanly. **All 14 existing `ActionAuthorizationAnalyzerTests` still pass.** All new generator and analyzer-extension tests pass.
2. `Aiel.Authorization.Generators.csproj` has `TargetFramework=netstandard2.0`, `IsGenerator=true`, `Deterministic=false`, `IncludeBuildOutput=false`, `EnforceExtendedAnalyzerRules=true`.
3. The generator DLL appears at `analyzers/dotnet/cs` in the NuGet pack, NOT in `lib/`.
4. `Aiel.Authorization.Application.Contracts.csproj` references `Aiel.Authorization.Generators` as `OutputItemType="Analyzer"` and includes the DLL in the NuGet payload via `<None Pack="true" PackagePath="analyzers/dotnet/cs" />`.
5. `Aiel.Authorization.Generators.csproj` carries NO project or package reference to any `Aiel.*` package. All type recognition is by metadata name string.
6. `[DefinePermission]` and `[GeneratedPermission]` exist in `Aiel.Authorization.Application.Contracts`, namespace `Aiel.Authorization`, with the shapes defined above.
7. `PermissionDefinitionManifest` gains `Lifecycle` (default `Active`) and `PreviousNames` (default `[]`) — neither is `required`. `PermissionTestData.CreateSampleManifest()` compiles and runs without modification.
8. The generator emits a `[GeneratedPermission(typeof(TAction))]`-annotated constants class for each `[DefinePermission]`-annotated action.
9. `ActionAuthorizationAnalyzer` recognizes `[GeneratedPermission(typeof(TAction))]` as the third passing condition — no `TRAF01001` fires on a covered action.
10. The stable ID for `RescheduleAppointmentAction` is identical across two separate generator invocations with the same input (idempotency).
11. The stable ID is preserved when the action type is renamed but `PreviousNames` is populated (rename-safety).
12. `Authorization.snapshot.json` schema is documented (or self-documented by the file shape); a missing snapshot is treated as first run, not an error.
13. All new projects are registered in `TwoRivers.slnx`, `Aiel/Aiel.slnx`, and `Aiel/virtual-folders.json`.
14. Build is clean: no warnings (beyond suppressed `NU5128`/`RS2000`), no nullable suppressions, no `#pragma warning disable` outside the generator project's intentional suppressions.
15. XML documentation is present on all public types in the generator project, the two new attribute types, and the two new manifest properties.

---

## Red-first test path

**Phase 1 — stubs (tests RED):**

- Create `Aiel.Authorization.Generators` with an empty `[Generator] PermissionDefinitionGenerator : IIncrementalGenerator` that registers nothing.
- Add `[DefinePermission]` and `[GeneratedPermission]` to `Aiel.Authorization.Application.Contracts`.
- Add `Lifecycle` and `PreviousNames` to `PermissionDefinitionManifest`.
- Write all tests in `Aiel.Authorization.Generators.UnitTests` (see below).
- Write the new analyzer extension tests in `Aiel.Authorization.Analyzers.UnitTests` (see below).
- Run: all new tests fail — generator emits no output; analyzer does not yet recognize the marker.

**Phase 2 — implement generator (generator tests GREEN):**

- Implement `PermissionDefinitionGenerator.Initialize` to scan for `[DefinePermission]`-annotated actions and emit constants + manifest helpers.
- Implement the stable ID algorithm with snapshot file support.
- Run: generator output tests pass. Analyzer extension tests still fail (analyzer not yet extended).

**Phase 3 — extend analyzer (all tests GREEN):**

- Extend `ActionAuthorizationAnalyzer.CollectCoveredActions` (or equivalent) to detect `[GeneratedPermission]` and extract `ActionType`.
- Run: all tests pass. All 14 original analyzer tests still pass.

---

## Required tests

### `PermissionGeneratorOutputTests` (new, in `Aiel.Authorization.Generators.UnitTests`)

All tests use `CSharpSourceGeneratorTest<PermissionDefinitionGenerator, DefaultVerifier>` with inline source stubs. Specimen action: `RescheduleAppointmentAction`.

1. `RescheduleAppointment_EmitsConstantsClass`
   - Input: `[DefinePermission(Name = "scheduling.reschedule-appointment", ScopeType = "Organization", SubjectType = "User", DisplayName = "Reschedule Appointment")] class RescheduleAppointmentAction : IAction {}`
   - Expected generated output: A file containing `RescheduleAppointmentActionPermissionConstants` with `Name = "scheduling.reschedule-appointment"` constant and `[GeneratedPermission(typeof(RescheduleAppointmentAction))]` annotation.

2. `RescheduleAppointment_EmitsManifestRegistrationHelper`
   - Same input as above.
   - Expected: A file containing `GeneratedPermissionManifests.GetManifests()` returning a manifest with matching `PermissionName`, `ScopeType`, `SubjectType`, `DisplayName`, `Lifecycle = PermissionLifecycle.Active`, and empty `PreviousNames`.

3. `RescheduleAppointment_StableId_IsDeterministicAcrossRuns`
   - Run the generator twice on identical input with no snapshot.
   - Expected: Both runs produce the same `StableId` constant value.

4. `RescheduleAppointment_StableId_IsPreservedFromSnapshot`
   - Input: Source with `[DefinePermission(Name = "scheduling.reschedule-appointment", ...)]`. Snapshot contains `{ "permissionName": "scheduling.reschedule-appointment", "stableId": "perm.scheduling.001" }`.
   - Expected: Generated `StableId` constant equals `"perm.scheduling.001"` (snapshot value, not freshly derived).

5. `RescheduleAppointment_StableId_IsPreservedAfterRename_ViaPreviousNames`
   - Input: `[DefinePermission(Name = "scheduling.rebook-appointment", PreviousNames = "scheduling.reschedule-appointment", ...)]`. Snapshot has entry for `scheduling.reschedule-appointment`.
   - Expected: Generated `StableId` for `scheduling.rebook-appointment` equals the snapshot value from the previous name.

6. `ActionWithoutDefinePermission_EmitsNothing`
   - Input: `class NakedAction : IAction {}` (no `[DefinePermission]`).
   - Expected: Generator emits no output files.

7. `MultipleAnnotatedActions_EmitsSeparateConstantsClasses`
   - Input: Two actions annotated with `[DefinePermission]`.
   - Expected: Two separate constants classes emitted; one `GeneratedPermissionManifests` with two entries.

8. `PermissionLifecycle_Deprecated_ReflectedInManifest`
   - Input: `[DefinePermission(..., Lifecycle = PermissionLifecycle.Deprecated)]` on an action.
   - Expected: Generated manifest entry has `Lifecycle = PermissionLifecycle.Deprecated`.

9. `PreviousNames_Populated_ReflectedInManifest`
   - Input: `[DefinePermission(..., PreviousNames = "scheduling.old-name;scheduling.older-name")]` on an action.
   - Expected: Generated manifest `PreviousNames` contains `PermissionName.From("scheduling.old-name")` and `PermissionName.From("scheduling.older-name")`.

### New tests in `Aiel.Authorization.Analyzers.UnitTests`

Test class: `GeneratedPermissionRecognitionTests`

1. `NoDiagnostic_WhenGeneratedPermissionMarkerExistsForAction`
   - Source: `[DefinePermission(...)] class RescheduleAppointmentAction : IAction {}` + inline stub for `[GeneratedPermission(typeof(RescheduleAppointmentAction))]` on a constants class (simulating generator output, since we cannot run the full generator pipeline in analyzer tests).
   - Expected: No `TRAF01001`.

2. `ReportsTRAF01001_WhenGeneratedMarkerCoversADifferentAction`
   - Source: `class ActionA : IAction {}`. `[GeneratedPermission(typeof(ActionB))]` exists for `ActionB`. No checker for `ActionA`.
   - Expected: `TRAF01001` on `ActionA`.

3. `NoDiagnostic_WhenGeneratedMarkerIsInDifferentNamespace`
   - Source: `ActionA : IAction` in namespace `Foo`; `[GeneratedPermission(typeof(ActionA))]` constants class in namespace `Bar.Generated`.
   - Expected: No `TRAF01001`. Recognition must be compilation-wide.

---

## Known accepted gap

`[DefinePermission]` on abstract action types is undefined behavior in Task 9. The generator SHOULD emit no output for abstract types (the `ActionAuthorizationAnalyzer` already ignores abstract actions). If the generator emits output for an abstract action, it is not a correctness error — it is a warning-level smell for a future diagnostic. This is NOT a Task 9 blocking issue.

---

## Verin rejection criteria

I will reject the Task 9 attempt if any of the following are true:

- `Aiel.Authorization.Generators.csproj` contains any project or package reference to any `Aiel.*` assembly. The generator identifies `[DefinePermission]` by metadata name string only.
- `Aiel.Authorization.Generators.csproj` has `TargetFramework` targeting `net*` instead of `netstandard2.0`.
- `Deterministic` is `true` in `Aiel.Authorization.Generators.csproj`. Generators are not deterministic; this must be `false`.
- The generator DLL lands in `lib/` (missing `IncludeBuildOutput=false`).
- `Aiel.Authorization.Application.Contracts.csproj` does not include the generator DLL at `analyzers/dotnet/cs` in the NuGet payload.
- `ActionAuthorizationAnalyzer` has NOT been extended — `TRAF01001` still fires on an action covered by a `[GeneratedPermission]` marker.
- Existing `ActionAuthorizationAnalyzerTests` are broken or modified to suppress failures. All 14 existing tests MUST pass unchanged.
- `[GeneratedPermission]` is applied manually (not emitted by the generator) to suppress a TRAF01001 that should fire — this is a test smell and a deliberate safety breach.
- The generator emits a class implementing `IActionPermissionChecker<TAction>` as a mechanism to fool the existing analyzer. This approach conflates two distinct contracts and is architecturally wrong.
- `PermissionDefinitionManifest.Lifecycle` or `.PreviousNames` is added as `required`. These additions MUST be non-breaking.
- `PreviousNames` returns `null`. The default MUST be an empty collection.
- The stable ID changes on a second generator run without a snapshot change or `PreviousNames` update. Idempotency is a hard gate.
- A missing snapshot causes a build error rather than triggering first-run derivation.
- Any test in `Aiel.Authorization.Generators.UnitTests` references a real `Aiel.*` assembly instead of inline stubs.
- EF Core, database migrations, or `IDbContext` are referenced anywhere in the generator or its tests.
- `EnforceExtendedAnalyzerRules` is missing or `false` in `Aiel.Authorization.Generators.csproj`.
- The build has warnings at solution level (beyond the suppressed `NU5128`/`RS2000` in the generator project).
- Public types in the generator project, new attribute types, or new manifest properties lack XML documentation.
- New projects are not registered in `TwoRivers.slnx`, `Aiel/Aiel.slnx`, and `Aiel/virtual-folders.json`.
