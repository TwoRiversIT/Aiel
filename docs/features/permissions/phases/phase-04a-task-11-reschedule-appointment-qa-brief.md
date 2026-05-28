# Phase 04a Task 11 — `RescheduleAppointment` Reference Slice QA Brief

- **Date:** 2026-05-25T20:29:57.517-07:00
- **Author:** Verin
- **Scope:** Task 11 only. Add the first end-to-end `RescheduleAppointment` reference slice on top of the Task 0-10 permission foundation.
- **Layer:** Application proof slice built on Aiel framework contracts. The slice belongs in Aiel sample/testing space, not Aviendha production code.
- **Baseline:** Task 10 fixes are in place. `PermissionDefinitionManifest` now carries action type, lifecycle, and previous names, the generator emits matching manifest data, rename fixtures live in `Aiel.Permissions.Testing`, and the current permission baseline is green in `Aiel.Permissions.Application.UnitTests`, `Aiel.Permissions.Testing.UnitTests`, `Aiel.Permissions.Generators.UnitTests`, and `Aiel.Permissions.EntityFrameworkCore.IntegrationTests`.

---

## Standalone slice verdict ✅

Task 11 is no longer coupled to Task 10 implementation work.

Why this is now clean:

1. The phase plan explicitly keeps the Task 10 rename proof independent of the production slice. The rename test uses fixture actions from `Aiel.Permissions.Testing`, not the Task 11 command.
2. The Task 10 contract mismatch is closed in `Aiel/src/Aiel.Permissions.Application.Contracts/Aiel/Permissions/PermissionDefinitionManifest.cs` and `Aiel/src/Aiel.Permissions.Generators/Aiel/Permissions/Generators/PermissionDefinitionSourceGenerator.cs`.
3. The rename fixture seam exists already in `Aiel/src/Aiel.Permissions.Testing/Aiel/Permissions/Testing/Fixtures/PermissionFixtureActions.cs` and `Aiel/src/Aiel.Permissions.Testing/Aiel/Permissions/Testing/PermissionTestData.cs`.
4. The rename and manifest snapshot behavior is already guarded by `Aiel/tests/Aiel.Permissions.EntityFrameworkCore.IntegrationTests/Aiel/Permissions/PermissionMigrationTests.cs`.

**QA conclusion:** Task 11 may proceed without waiting on Task 12+ and without reopening Task 10.

---

## Mandatory red-first tests

Task 11 does not start with production code. It starts with failing tests that prove the slice boundary and gate order.

### Required new failing tests

1. **Validator short-circuit**
   - Add a failing test proving the default appointment ID is rejected by `RescheduleAppointmentValidator`.
   - Assert the permission checker is never reached after validation failure.

2. **Grant denial short-circuit**
   - Add a failing test proving gate-level permission denial stops before the application service loads the appointment aggregate.
   - This is the key “grant denial never loads the aggregate” acceptance gate.

3. **Resource denial after gate success**
   - Add a failing test proving the application service calls resource authorization only after the gate succeeds, then returns a denial result without saving when resource authorization fails.

4. **Happy path orchestration**
   - Add a failing test proving the success path reschedules once and saves once.
   - Assert business logic happens only after validation, gate authorization, and resource authorization all succeed.

### Mandatory regression guards to keep green while implementing

1. Generator regression:
   - `Aiel/tests/Aiel.Permissions.Generators.UnitTests/Aiel/Permissions/Generators/PermissionDefinitionSourceGeneratorTests.cs`
   - Keep stable ID and manifest emission coverage green for `RescheduleAppointment`.

2. Persistence / rename regression:
   - `Aiel/tests/Aiel.Permissions.EntityFrameworkCore.IntegrationTests/Aiel/Permissions/PermissionMigrationTests.cs`
   - Keep the `ChangeAppointment` → `RescheduleAppointment` rename proof green while introducing the real reference slice.

### Recommended homes for the new Task 11 tests

- Put gate-order and application-service orchestration tests in `Aiel.Permissions.Application.UnitTests`.
- Put fake sample IDs, fake repository helpers, and reference-slice support seams in `Aiel.Permissions.Testing` and `Aiel.Permissions.Testing.UnitTests`.
- If a dedicated sample test project is introduced, it must stay under `Aiel.*`, not `Aviendha.*`.

---

## Mandatory validation commands

Run these in order:

```powershell
dotnet test D:\source\worktrees\tr\squad\copilot-aiel-permissions-04a-contracts\Aiel\tests\Aiel.Permissions.Application.UnitTests\Aiel.Permissions.Application.UnitTests.csproj --no-restore -v minimal
dotnet test D:\source\worktrees\tr\squad\copilot-aiel-permissions-04a-contracts\Aiel\tests\Aiel.Permissions.Testing.UnitTests\Aiel.Permissions.Testing.UnitTests.csproj --no-restore -v minimal
dotnet test D:\source\worktrees\tr\squad\copilot-aiel-permissions-04a-contracts\Aiel\tests\Aiel.Permissions.Generators.UnitTests\Aiel.Permissions.Generators.UnitTests.csproj --no-restore -v minimal
dotnet test D:\source\worktrees\tr\squad\copilot-aiel-permissions-04a-contracts\Aiel\tests\Aiel.Permissions.EntityFrameworkCore.IntegrationTests\Aiel.Permissions.EntityFrameworkCore.IntegrationTests.csproj --no-restore -v minimal
dotnet test D:\source\worktrees\tr\squad\copilot-aiel-permissions-04a-contracts\Aiel\Aiel.slnx --no-restore -v minimal
```

Interpretation:

- The first two commands are the Task 11 red/green loop.
- The next two commands are non-negotiable regressions from Tasks 9 and 10.
- The solution run is the release gate. No warnings. No skipped breakage. No “works on the slice only” acceptance.

---

## Rejection conditions

Implementation must be blocked or rejected if any of the following occurs.

### Architecture blockers

1. **Aviendha leakage**
   - Reject any implementation that places the first slice in `Aviendha.*` or depends on Aviendha production types.

2. **Wrong layer ownership**
   - Reject any infrastructure adapter or web endpoint work in Task 11. ASP.NET Core and HTTP samples belong to Task 12.

3. **Authorization logic in the wrong place**
   - Reject if the application service duplicates gate-level authorization.
   - Reject if resource checks are moved into infrastructure or presentation code.

4. **Bypassing the permission stack**
   - Reject if the slice bypasses `IActionGate<TAction>`.
   - Reject if it hardcodes free-text permission names instead of using the manifest/generator flow.

5. **Null or exception drift**
   - Reject public null-return patterns, nullable success models, or exceptions used for expected denial/validation outcomes.

### Test / behavior blockers

1. Reject if validation failure still hits the permission checker.
2. Reject if grant denial still touches the repository or aggregate.
3. Reject if resource denial still saves or mutates the appointment.
4. Reject if the success path performs more than one reschedule or more than one save.
5. Reject if Task 9/10 regression suites fail after adding the slice.
6. Reject if the slice introduces magic strings for permission name, scope type, subject type, or fake IDs where existing testing helpers already provide stable values.

---

## Implementation owner — Route Task 11 to Perrin

This remains framework-owned work.

- The slice is a Aiel proof-of-concept, not an Aviendha feature.
- The plan explicitly prefers sample/testing projects for the first vertical slice.
- The task validates framework seams: `IActionGate<TAction>`, generated permission definitions, and the application-service contract boundary.

**Owner:** Perrin  
**Reviewer stance:** QA review should reject Aviendha-first implementations and require the slice to stay inside the Aiel testing/sample boundary.

---

## Ready-for-implementation verdict

Task 11 is ready to start once the owner begins with the four red tests above.

The fastest wrong move is to treat this like an Aviendha feature. The correct move is to prove the Aiel permission stack end-to-end with one narrow, test-first reference slice and leave adapters to Task 12.
