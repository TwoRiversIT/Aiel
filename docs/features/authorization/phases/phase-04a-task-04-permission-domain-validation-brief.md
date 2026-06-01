# Phase 04a Task 4 Permission Domain Validation Brief

- **Date:** 2026-05-25T16:48:07.854-07:00
- **Author:** Verin
- **Scope:** Task 4 only. Create `Aiel.Permissions.Domain` and `Aiel.Permissions.Domain.UnitTests` for the first permission domain model slice. Do not mix Task 5+ contracts, runtime services, EF mappings, analyzers, generators, or ASP.NET adapters into this change.
- **Layer:** Domain. This slice belongs here because it owns the `PermissionGrant` aggregate, the permission catalog root, and the invariant/lifecycle behavior that must not leak into `Domain.Shared`, application services, or infrastructure.
- **Baseline observed in this worktree:**
  - `Aiel/src/Aiel.Permissions.Domain/` does not exist yet.
  - `Aiel/tests/Aiel.Permissions.Domain.UnitTests/` does not exist yet.
  - `Aiel.Permissions.Domain.Shared.UnitTests` is currently green: 7/7 passing.
- **Non-collision rule:** Safe QA work for this task is a review/validation artifact plus future isolated test files. Do not edit Perrin's likely implementation files outside the new domain slice just to make this task compile.

## Focused acceptance gate

Treat `Aiel/tests/Aiel.Permissions.Domain.UnitTests/Aiel.Permissions.Domain.UnitTests.csproj` as the authoritative acceptance gate for Task 4.

Task 4 is not done until all of the following are true:
1. `Aiel/src/Aiel.Permissions.Domain/Aiel.Permissions.Domain.csproj` and `Aiel/tests/Aiel.Permissions.Domain.UnitTests/Aiel.Permissions.Domain.UnitTests.csproj` exist and are added to `TwoRivers.slnx`, `Aiel.slnx`, and `virtual-folders.json`.
2. `dotnet test .\Aiel\tests\Aiel.Permissions.Domain.UnitTests\Aiel.Permissions.Domain.UnitTests.csproj --nologo --verbosity minimal` passes cleanly.
3. `dotnet test .\Aiel\tests\Aiel.Permissions.Domain.Shared.UnitTests\Aiel.Permissions.Domain.Shared.UnitTests.csproj --nologo --verbosity minimal` still passes. Task 4 must not weaken the Task 3 shared contract.
4. The domain tests prove the public slice contains:
   - a behavior-owning `PermissionGrant` aggregate, not a passive persistence DTO
   - one explicit permission catalog root (`PermissionCatalogEntry`, `PermissionDefinition`, or equivalent) that owns published permission identity/name/lifecycle invariants
   - non-null public properties returning shared value objects / strong IDs / empty collections instead of raw nullable primitives
5. Grant-creation tests prove the aggregate fails closed for invalid construction inputs:
   - default `PermissionGrantId`
   - invalid `PermissionName`
   - empty or invalid `PermissionSubjectTypeName` / `PermissionSubjectKey`
   - empty or invalid `PermissionScopeTypeName` / `PermissionScopeKey`
   - missing explicit `PermissionGrantDecision`
6. Matching tests prove subject and scope comparison is infrastructure-free and exact:
   - exact subject + scope matches succeed
   - subject mismatch fails closed
   - scope mismatch fails closed
   - stored grant polarity (`Granted` vs `Prohibited`) remains explicit on the aggregate and is not silently normalized away
7. Lifecycle tests prove the catalog root owns explicit transition methods instead of public enum mutation:
   - `Active -> Deprecated` is allowed
   - `Deprecated -> Removed` is allowed
   - `Removed -> Active`, `Removed -> Deprecated`, and `Deprecated -> Active` are rejected unless Perrin documents and tests a different policy in this slice
   - invalid transitions return `Result` or another explicit non-exception outcome for expected control flow

## First red tests that should exist

Start with these failing tests before Perrin implements the domain package:

1. **`PermissionGrantCreationTests`**
   - Fail until `PermissionGrant` rejects default `PermissionGrantId`.
   - Fail until invalid permission name, subject type/key, and scope type/key inputs are blocked.
   - Fail until the aggregate requires an explicit `PermissionGrantDecision` instead of assuming a default branch.

2. **`PermissionGrantMatchingTests`**
   - Fail until the domain model exposes an infrastructure-free way to compare a grant against subject + scope.
   - Fail until exact matches succeed and any subject or scope mismatch fails closed.
   - Fail if a `Prohibited` grant is flattened into a boolean and loses its stored polarity.

3. **`PermissionCatalogLifecycleTests`**
   - Fail until the catalog model starts in a valid lifecycle state and exposes explicit transition methods.
   - Fail if `Active -> Removed` succeeds directly.
   - Fail if `Deprecated -> Active` or any move out of `Removed` succeeds without an explicit, documented Task 4 policy.
   - Fail if invalid lifecycle moves throw instead of returning a result/outcome object.

4. **`PermissionDomainSurfaceTests`**
   - Fail until the domain assembly exposes the grant aggregate plus one catalog root in `Aiel.Permissions.Domain`.
   - Fail if public properties return nullable values, raw strings, or raw IDs where Task 3 value objects already exist.
   - Fail if Task 4 types leak into `Aiel.Permissions.Domain.Shared` or depend outward on application/infrastructure packages.

## Verin rejection criteria

I will reject the Task 4 attempt if any of the following are true:
- `PermissionGrant` is implemented as an anemic record/DTO with public setters and no invariant-owning methods.
- Public models expose raw strings, primitive IDs, or nullable state where Task 3 already established value objects and strong IDs.
- Lifecycle is modeled as direct enum assignment instead of validated domain transitions with explicit outcomes.
- The slice uses exceptions for expected invalid moves or invalid creation paths instead of `Try...`, `Result`, or another explicit failure model.
- Scope/subject matching depends on infrastructure lookups, ambient execution context, or evaluator precedence logic. Task 4 should stay infrastructure-free and exact-match only.
- The change invents Task 5+ behavior in the domain package: permission manager/store contracts, application evaluators, EF records/mappings, ASP.NET adapters, or analyzer concerns.
- The catalog lifecycle policy is ambiguous because multiple types share ownership of lifecycle invariants, or both `Domain` and `Domain.Shared` claim the same behavior.
- The PR does not prove a red-to-green path for the new tests before the implementation landed.
