# Phase 04a Task 3 Permission Domain Shared Validation Brief

- **Date:** 2026-05-25T16:31:17.856-07:00
- **Author:** Verin
- **Scope:** Task 3 only. Bootstrap `Aiel.Authorization.Domain.Shared` and `Aiel.Authorization.Domain.Shared.UnitTests` without mixing Task 4 grant behavior, permission runtime, adapters, or generator/analyzer feature work.
- **Layer:** Domain.Shared. This slice belongs here because it owns reusable permission identities, lifecycle enums, and human-readable names with no grant or application behavior.
- **Baseline observed in this worktree:**
  - `Aiel.Authorization.Domain.Shared` does not exist yet.
  - `Aiel.Authorization.Domain.Shared.UnitTests` does not exist yet.
  - Current 04a boundary-move baseline is green: `Aiel.Application.Contracts.UnitTests` = 5/5 passing, `Aiel.Application.UnitTests` = 59/59 passing.

## Focused acceptance gate

Treat `Aiel/tests/Aiel.Authorization.Domain.Shared.UnitTests/Aiel.Authorization.Domain.Shared.UnitTests.csproj` as the authoritative acceptance gate for Task 3.

Task 3 is not done until all of the following are true:

1. `Aiel/src/Aiel.Authorization.Domain.Shared/Aiel.Authorization.Domain.Shared.csproj` and `Aiel/tests/Aiel.Authorization.Domain.Shared.UnitTests/Aiel.Authorization.Domain.Shared.UnitTests.csproj` exist and are added to `TwoRivers.slnx`, `Aiel.slnx`, and `virtual-folders.json`.
2. `dotnet test .\Aiel\tests\Aiel.Authorization.Domain.Shared.UnitTests\Aiel.Authorization.Domain.Shared.UnitTests.csproj --nologo --verbosity minimal` passes cleanly.
3. That test project proves the public surface contains exactly the Task 3 shared types that downstream slices need first:
   - `PermissionName`
   - `PermissionStableId`
   - `PermissionGrantId`
   - `PermissionScopeTypeName`
   - `PermissionScopeKey`
   - `PermissionSubjectTypeName`
   - `PermissionSubjectKey`
   - `CapabilitySnapshotVersion`
   - `PermissionLifecycle`
   - `AuthorizationGrantDecision`
4. The tests prove human-readable names are value objects, not raw-string aliases:
   - `PermissionName`, `PermissionScopeTypeName`, and `PermissionSubjectTypeName` reject null, empty, and whitespace input.
   - Canonicalization is limited to trimming outer whitespace. Reviewer gate should fail if the implementation silently lowercases or otherwise rewrites published permission names.
   - `PermissionScopeTypeName` remains open-ended. Built-in values such as `Platform`, `Host`, and `Tenant` must work, but application-defined values such as `Clinic` must also work without editing a framework enum.
5. The tests prove strong IDs are authored through `Aiel.StrongIds`, not hand-rolled wrappers:
   - `PermissionStableId`, `PermissionGrantId`, `PermissionScopeKey`, `PermissionSubjectKey`, and `CapabilitySnapshotVersion` compile with generated `From` and `TryFrom` members.
   - `PermissionGrantId` rejects default `Guid` values.
   - `PermissionStableId` follows the manifest shape already shown in planning docs (`perm_...`) and rejects null, empty, or whitespace values.
   - Scope keys, subject keys, and snapshot versions must not leak raw strings or primitives into the public API.

## First red tests that should exist

Start with these failing tests before Perrin implements the package:

1. **`PermissionDomainSharedSurfaceTests`**
   - Fail until the assembly exposes every Task 3 type listed above.
   - Fail if `PermissionLifecycle` or `AuthorizationGrantDecision` is missing from the shared package.

2. **`PermissionNameTests`**
   - Fail on null, empty, and whitespace-only inputs.
   - Fail if leading/trailing whitespace is preserved.
   - Fail if a valid canonical name such as `Aviendha.Scheduling.Appointments.ChangeAppointment` is mutated beyond trimming.

3. **`PermissionTypeNameTests`**
   - Cover both `PermissionScopeTypeName` and `PermissionSubjectTypeName`.
   - Fail on null, empty, and whitespace-only inputs.
   - Fail if the implementation hard-codes a closed enum/list that blocks application-defined names.

4. **`PermissionStrongIdTests`**
   - Prove `PermissionGrantId.From(Guid.NewGuid())` round-trips and `PermissionGrantId.TryFrom(Guid.Empty, out _)` fails.
   - Prove `PermissionStableId.From(" perm_01jz9p58d6d8m8n7x3t9q2a4bc ")` trims to the canonical stored value.
   - Prove the remaining shared IDs expose generated `From`/`TryFrom` members so the package is genuinely using the source generator.

## Verin rejection criteria

I will reject the Task 3 attempt if any of the following are true:

- Public models expose raw strings, primitive IDs, or nullable values instead of value objects / strong IDs.
- `PermissionScopeTypeName` or `PermissionSubjectTypeName` is modeled as a closed enum, static whitelist, or switch that blocks application-defined names.
- The package bypasses `Aiel.StrongIds` with hand-written ID wrappers or default-valued IDs that should fail closed.
- The slice lands only constructors that throw for expected invalid input and offers no explicit non-exception path where one is expected.
- Task 4+ behavior appears in the same slice: grant aggregates, evaluators, stores, managers, EF mappings, ASP.NET adapters, or analyzer/runtime decisions.
- The new projects are not wired into solution metadata, making the slice look green only because CI never builds it.
- The PR changes Perrin's runtime/application files just to make the shared package compile.
