# Phase 04a Task 8 — Aiel.Authorization.Analyzers QA Brief

- **Date:** 2026-05-28T00:00:00Z
- **Author:** Verin
- **Scope:** Task 8 only. Add `Aiel.Authorization.Analyzers` (src) and `Aiel.Authorization.Analyzers.UnitTests` (tests). Add the `DoesNotRespectAuthority` attribute to `Aiel.Authorization.Application.Contracts`. No other product code changes.
- **Layer:** Roslyn analyzer package. Depends inward on `Microsoft.CodeAnalysis.CSharp` only. Identifies types by metadata name strings; no runtime project reference to `Aiel.Authorization.*` packages.
- **Baseline:** Task 7 committed (7b9fe71). Worktree is clean. All existing tests pass.

---

## Slice-boundary verdict: Task 8 is a standalone slice ✅ — with one required pre-req change

Task 8 can land independently before Task 9. The plan explicitly defers generated-definition recognition to Task 9 because the generator contract does not exist yet. The Task 8 analyzer surface is deliberately narrow:

- Recognize a **concrete `IActionPermissionChecker<TAction>`** for the analyzed action type.
- Recognize the **`[DoesNotRespectAuthority(Reason = "...")]`** attribute with a non-empty `Reason`.
- Report an error if **neither** is present.

The **known gap** — actions with only a generated permission definition (no concrete checker, no marker) — is an accepted, documented false-positive until Task 9 lands. This gap MUST be called out in the analyzer diagnostic message and in the `AnalyzerReleases.Unshipped.md` notes.

**Pre-req change required in Task 8 scope:** `DoesNotRespectAuthority` does not exist anywhere in the codebase. It MUST be defined in `Aiel.Authorization.Application.Contracts` as part of this slice. It is an application contract extension, not an analyzer implementation detail. The analyzer recognizes it by its fully qualified metadata name — no compile-time project reference between the analyzer DLL and the runtime package is required or permitted.

Task 8 MUST NOT merge with Task 9 (generator coupling) or absorb any EF Core, ASP.NET Core, or other adapter scope.

---

## The `DoesNotRespectAuthority` attribute — placement and shape

**Where:** `Aiel.Authorization.Application.Contracts`, namespace `Aiel.Authorization`.

**Shape (contract definition — not implementation):**

```csharp
/// <summary>
/// Marks an action as explicitly not subject to the permission system.
/// </summary>
/// <remarks>
/// A non-empty <see cref="Reason"/> is required. The analyzer rejects empty or whitespace reasons.
/// This marker should be uncomfortable to use. It is intentionally verbose and searchable.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DoesNotRespectAuthorityAttribute : Attribute
{
    /// <summary>Gets the mandatory explanation of why this action bypasses the permission system.</summary>
    public required string Reason { get; init; }
}
```

**Rules:**

- `AttributeUsage` MUST be `AttributeTargets.Class` only. It is not valid on interfaces, structs, or records.
- `AllowMultiple = false` — applying the attribute twice is a compile error.
- `Reason` MUST be `required` — the C# `required` modifier ensures callers supply it. The analyzer still validates for non-empty content at analysis time.
- No constructor overloads. The `required` property pattern is the only API.

---

## Required projects and their dependencies

### `Aiel/src/Aiel.Authorization.Analyzers/Aiel.Authorization.Analyzers.csproj`

Mirror the structure of `Aiel.Analyzers.csproj`:

| Property | Value |
| --- | --- |
| `TargetFramework` | `netstandard2.0` |
| `IsPackable` | `true` |
| `IsRoslynComponent` | `true` |
| `EnforceExtendedAnalyzerRules` | `true` |
| `IncludeBuildOutput` | `false` |
| `Deterministic` | `true` |
| DLL pack path | `analyzers/dotnet/cs` |

**Allowed package references:**

- `Microsoft.CodeAnalysis.CSharp` (same version as `Aiel.Analyzers` — currently `5.3.0`), `PrivateAssets="all"`
- `Microsoft.CodeAnalysis.Analyzers` (same version), `PrivateAssets="all"`

**Forbidden references:**

- Any `Aiel.*` project or package reference — the analyzer MUST NOT take a runtime dependency on `Aiel.Authorization.Application.Contracts` or any other Aiel package. All type recognition is by metadata name string.
- Any package that targets `net*` instead of `netstandard2.0`.

The project MUST include `AnalyzerReleases.Shipped.md` and `AnalyzerReleases.Unshipped.md` as `<AdditionalFiles>`.

### `Aiel/tests/Aiel.Authorization.Analyzers.UnitTests/Aiel.Authorization.Analyzers.UnitTests.csproj`

Mirror `Aiel.Analyzers.UnitTests.csproj`:

| Property | Value |
| --- | --- |
| `TargetFramework` | `net10.0` |
| `IsPackable` | `false` |
| `IsTestProject` | `true` |

**Required package references:**

- `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` (same version as the existing test project — currently `1.1.3`)
- `Microsoft.CodeAnalysis.CSharp.Workspaces`
- `Microsoft.CodeAnalysis.Workspaces.Common`

**Project reference:** `Aiel.Authorization.Analyzers` only.

---

## Diagnostic ID scheme

`Aiel.Authorization.Analyzers` occupies the `AIEL20000`–`AIEL20999` block.

| ID | Title | Severity | Suppressible |
| --- | --- | --- | --- |
| `AIEL20001` | `ActionHasNoAuthorizationStory` | `Error` | No |
| `AIEL20002` | `DoesNotRespectAuthorityReasonIsEmpty` | `Error` | No |

**`AIEL20001` — `ActionHasNoAuthorizationStory`**

> Action type `{0}` implements `IAction` but has no concrete `IActionPermissionChecker<{0}>` and no `[DoesNotRespectAuthority]` attribute. Add a concrete checker or mark the action with `[DoesNotRespectAuthority(Reason = "...")]`. Note: generated permission definitions are recognized by the analyzer only after `Aiel.Authorization.Generators` is added (Task 9).

Category: `Authorization`. DefaultSeverity: **Error**. Must NOT be suppressible via `#pragma warning disable` (enforce per D4: missing authorization story is always a build failure).

**`AIEL20002` — `DoesNotRespectAuthorityReasonIsEmpty`**

> `[DoesNotRespectAuthority]` on `{0}` has an empty or whitespace `Reason`. Provide a non-empty explanation.

Category: `Authorization`. DefaultSeverity: **Error**.

Both diagnostics MUST appear in `AnalyzerReleases.Unshipped.md` before the PR can merge.

---

## Analyzer behavior — what it DOES and DOES NOT do

### DOES

- Registers `RegisterSymbolAction` for `SymbolKind.NamedType`.
- For each named type that is a **concrete, non-abstract class** (not interface, not abstract, not struct) implementing `IActionInterfaceName` (`Aiel.Actions.IAction`):
  - Walks the compilation for any concrete non-abstract class implementing `IActionPermissionCheckerInterfaceName` (`Aiel.Authorization.IActionPermissionChecker<TAction>`) where `TAction` is the current action type. If found → no diagnostic.
  - Checks whether the action type carries `[DoesNotRespectAuthority]` (metadata name `Aiel.Authorization.DoesNotRespectAuthorityAttribute`). If present with a non-empty `Reason` → no diagnostic. If present with empty or whitespace `Reason` → `AIEL20002`.
  - If neither check passes → `AIEL20001`.
- Respects `GeneratedCodeAnalysisFlags.None` — does not fire on generated code.
- Enables `EnableConcurrentExecution`.

### DOES NOT

- Does NOT probe for generated permission definition markers — this is the explicit Task 9 gap.
- Does NOT fire on abstract action types, action interfaces, or action records that are not concrete classes.
- Does NOT fire on `ICommand` or `IQuery<TResult>` subtypes via a separate code path — those interfaces transitively implement `IAction`, so the same rule applies uniformly. No special case for command vs. query.
- Does NOT validate string-based permission name literals (future diagnostic `AIEL20003` or later).
- Does NOT check client capability metadata.
- Does NOT hold a compile-time reference to any `Aiel.*` assembly.

---

## Focused acceptance gate

Task 8 is NOT done until ALL of the following are true:

1. `dotnet test .\Aiel\Aiel.slnx --nologo --tl:off -v minimal` passes cleanly. All previous tests still pass. New `Aiel.Authorization.Analyzers.UnitTests` tests pass.
2. `AIEL20001` has `DefaultSeverity = DiagnosticSeverity.Error` and is NOT in the suppression allow-list.
3. `AIEL20002` has `DefaultSeverity = DiagnosticSeverity.Error`.
4. `Aiel.Authorization.Analyzers.csproj` carries NO project or package reference to any `Aiel.*` package.
5. `DoesNotRespectAuthorityAttribute` exists in `Aiel.Authorization.Application.Contracts`, carries `required string Reason`, and has `AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)`.
6. Both `AIEL20001` and `AIEL20002` appear in `AnalyzerReleases.Unshipped.md` with the correct metadata format.
7. All new projects are registered in `TwoRivers.slnx`, `Aiel/Aiel.slnx`, and `Aiel/virtual-folders.json`.
8. Build is clean: no warnings, no nullable suppressions, no `#pragma warning disable`.
9. XML documentation is present on all public types in the analyzer project.

---

## Red-first test path

**Phase 1 — stubs (tests RED):**

- Create both projects.
- Add `DoesNotRespectAuthorityAttribute` to `Aiel.Authorization.Application.Contracts`.
- Add `ActionAuthorizationAnalyzer : DiagnosticAnalyzer` with `SupportedDiagnostics` returning the two descriptors but `Initialize` registering nothing (empty body).
- Write all four test classes below. Run: all tests fail (no diagnostics reported when failures expected; no error when empty reason should fire).

**Phase 2 — implement analyzer (tests GREEN):**

- Implement `ActionAuthorizationAnalyzer.Initialize` with symbol action registration and the two checks.
- Run: all tests pass.

---

## Required tests in `Aiel.Authorization.Analyzers.UnitTests`

All tests use inline source strings with `Aiel.Authorization.IAction`, `Aiel.Authorization.IActionPermissionChecker<TAction>`, and `Aiel.Authorization.DoesNotRespectAuthorityAttribute` stubs embedded as `TestState.Sources` entries — identical in structure to the `AssemblyAnalyzerTests.cs` pattern in `Aiel.Analyzers.UnitTests`. Tests DO NOT reference any `Aiel.*` runtime assembly.

### `ActionHasNoAuthorizationStoryTests`

1. `ReportsAIEL20001_WhenActionHasNoCheckerAndNoMarker`
   - Source: concrete class `SampleAction : IAction`. No checker. No attribute.
   - Expected: `AIEL20001` on the `SampleAction` type declaration.

2. `NoDiagnostic_WhenConcreteCheckerExists`
   - Source: `SampleAction : IAction` + `SampleActionPermissionChecker : IActionPermissionChecker<SampleAction>`.
   - Expected: no diagnostics.

3. `NoDiagnostic_WhenDoesNotRespectAuthorityWithValidReason`
   - Source: `[DoesNotRespectAuthority(Reason = "Public health endpoint, no auth required.")]` on `SampleAction : IAction`.
   - Expected: no diagnostics.

4. `NoDiagnostic_WhenActionIsAbstract`
   - Source: `abstract class BaseAction : IAction`.
   - Expected: no diagnostics.

5. `NoDiagnostic_WhenTypeIsInterface`
   - Source: `interface ICustomAction : IAction { }`.
   - Expected: no diagnostics.

6. `ReportsAIEL20001_ForEachUncoveredActionInCompilation`
   - Source: `SampleActionA : IAction`, `SampleActionB : IAction`, checker only for `SampleActionA`.
   - Expected: `AIEL20001` on `SampleActionB` only.

### `DoesNotRespectAuthorityReasonTests`

1. `ReportsAIEL20002_WhenReasonIsEmpty`
   - Source: `[DoesNotRespectAuthority(Reason = "")]` on `SampleAction : IAction`.
   - Expected: `AIEL20002` on `SampleAction`.

2. `ReportsAIEL20002_WhenReasonIsWhitespace`
   - Source: `[DoesNotRespectAuthority(Reason = "   ")]` on `SampleAction : IAction`.
   - Expected: `AIEL20002` on `SampleAction`.

3. `NoDiagnostic_WhenReasonIsNonEmpty`
   - Source: `[DoesNotRespectAuthority(Reason = "Reason supplied.")]` on `SampleAction : IAction`.
   - Expected: no diagnostics. (Covered by `ActionHasNoAuthorizationStoryTests.3` above; include here for clarity of intent.)

### `CheckerRecognitionTests`

1. `NoDiagnostic_WhenCheckerIsInDifferentNamespace`
   - Source: `SampleAction : IAction` in namespace `Foo`; checker `SampleActionPermissionChecker : IActionPermissionChecker<SampleAction>` in namespace `Bar`.
   - Expected: no diagnostics. Checker recognition must be compilation-wide, not namespace-restricted.

2. `ReportsAIEL20001_WhenCheckerExistsForDifferentAction`
   - Source: `SampleActionA : IAction`, `SampleActionB : IAction`, `SampleActionAChecker : IActionPermissionChecker<SampleActionA>`. No checker for B.
   - Expected: `AIEL20001` on `SampleActionB`.

3. `NoDiagnostic_WhenCheckerIsAbstractButConcreteSubclassExists`
   - Source: `SampleAction : IAction`; abstract `AbstractChecker : IActionPermissionChecker<SampleAction>`; concrete sealed `ConcreteChecker : AbstractChecker`.
   - Expected: no diagnostics. The concrete subclass satisfies the check.

---

## Known accepted gap — Task 9

Actions that have a generated permission definition (emitted by `Aiel.Authorization.Generators`) but no concrete checker and no `[DoesNotRespectAuthority]` marker will trigger `AIEL20001` in Task 8. This is an explicit, documented gap. It is NOT a Task 8 bug; it is the motivation for Task 9.

The `AIEL20001` diagnostic message MUST include a note that generated definitions are recognized after Task 9 lands:

> "Note: generated permission definitions are recognized by the analyzer only after `Aiel.Authorization.Generators` is added."

Task 9 will extend `ActionAuthorizationAnalyzer` (or a companion analyzer) to recognize the generated output and suppress `AIEL20001` accordingly.

---

## Verin rejection criteria

I will reject the Task 8 attempt if any of the following are true:

- `Aiel.Authorization.Analyzers.csproj` contains a project or package reference to any `Aiel.*` assembly. Analyzers identify types by metadata name string only.
- `AIEL20001` or `AIEL20002` has `DefaultSeverity = DiagnosticSeverity.Warning`. Both MUST be errors per D4.
- Either diagnostic ID is missing from `AnalyzerReleases.Unshipped.md`.
- The analyzer fires on abstract action classes, action interfaces, or action records declared as `abstract`.
- The analyzer does not recognize a checker defined in a different namespace from the action — checker lookup must be compilation-wide.
- `DoesNotRespectAuthorityAttribute` is not in `Aiel.Authorization.Application.Contracts`, or its `Reason` property is not `required`.
- Any test uses a real `Aiel.*` assembly reference instead of inline stubs. Tests MUST be self-contained using inline source strings.
- The slice absorbs any generator recognition logic, even a placeholder. Generated definition recognition belongs exclusively to Task 9.
- The build has any warnings at solution level after this slice lands.
- `EnforceExtendedAnalyzerRules` is missing or set to `false` in `Aiel.Authorization.Analyzers.csproj`.
- Public types in the analyzer project lack XML documentation comments.
