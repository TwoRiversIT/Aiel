# Phase 04a Task 12 — ASP.NET Core and HTTP client adapter sample QA brief

- **Date:** 2026-05-25T20:29:57.517-07:00
- **Author:** Verin
- **Scope:** Task 12 only. Add the hand-written ASP.NET Core endpoint sample and matching HTTP client adapter sample on top of the Task 11 `RescheduleAppointment` reference slice.
- **Layer:** Edge adapter sample in `Aiel.*`. Transport code only. The application-service contract remains the authority boundary.
- **Baseline:** Task 11 is complete in `Aiel/tests/Aiel.Permissions.Application.UnitTests/Aiel/Permissions/RescheduleAppointmentReferenceSliceTests.cs`. Phase 04 decision D7 says edge adapters are samples first, not generated endpoints first.

---

## Standalone slice verdict ✅

Task 12 stands alone from Tasks 13 and 14, but its two transport halves must land together.

Why this slice is clean:

1. The phase plan gives Task 12 its own projects and test gate: a hand-written ASP.NET Core sample plus a hand-written HTTP client sample.
2. Task 13 starts a different concern set: capability contracts, snapshot refresh, and Blazor visibility behavior.
3. Task 14 is documentation only and depends on the Task 12 package names and transport shape being real first.
4. Task 11 already proved the action gate and application service flow. Task 12 should only prove that transport adapters call that contract correctly.

**QA conclusion:** Do not merge Task 12 into Task 13 or Task 14. Also do not split the endpoint half and client half into separate slices; the adapter proof is only credible when both round-trip together.

---

## Mandatory red-first tests

Task 12 starts with failing transport tests, not package plumbing.

### Required new failing tests

1. **Endpoint delegates once to the application service**
   - Add a failing integration test in a dedicated ASP.NET Core sample host proving the endpoint binds the HTTP request, creates `RescheduleAppointment`, and calls `IAppointmentApplicationService.RescheduleAsync(...)` exactly once.
   - Assert the forwarded action carries the appointment ID, scope key, and schedule values from the request body without transport-only mutation.

2. **Endpoint does not duplicate permission enforcement metadata**
   - Add a failing test that inspects the endpoint surface and proves there is no action-level permission attribute, no hard-coded permission policy, and no direct permission-name string on the route handler or controller action.
   - Ordinary authentication metadata is fine. Permission evaluation must still happen through the Task 11 application-service pipeline.

3. **HTTP client preserves success `Result` semantics**
   - Add a failing integration test proving the sample client calls the sample endpoint through `ResultHttpClientExtensions` and returns a successful `Result` or `Result<T>` without custom deserialization code.
   - Assert the success payload survives the round-trip in the shape promised by the application-service contract.

4. **HTTP client preserves failure semantics**
   - Add a failing integration test proving a validation or permission denial returned by the sample endpoint comes back through the client as a failed `Result` with the same error type family.
   - Expected failures must stay in `Result` form. No exception-based control flow.

### Recommended homes for the new Task 12 tests

- `Aiel/tests/Aiel.Permissions.AspNetCore.IntegrationTests/Aiel.Permissions.AspNetCore.IntegrationTests.csproj`
- `Aiel/tests/Aiel.Permissions.AspNetCore.IntegrationTests.WebApplication/Aiel.Permissions.AspNetCore.IntegrationTests.WebApplication.csproj`
- Keep the fake `IAppointmentApplicationService` and sample request/response DTOs inside the Task 12 sample test boundary, not in `Aviendha.*`.

### Mandatory regression guards to keep green while implementing

1. **Task 11 application slice regression**
   - `Aiel/tests/Aiel.Permissions.Application.UnitTests/Aiel.Permissions.Application.UnitTests.csproj`
   - Keep the `RescheduleAppointment` gate-order tests green. Task 12 must consume the application contract, not reshape it.

2. **Result transport regression**
   - `Aiel/tests/Aiel.Results.IntegrationTests/Aiel.Results.IntegrationTests.csproj`
   - Keep existing `Result` HTTP serialization and client extension behavior green.

3. **Result analyzer regression**
   - `Aiel/tests/Aiel.Results.Generators.UnitTests/Aiel.Results.Generators.UnitTests.csproj`
   - Keep `AIEL00003` coverage green so the sample client does not normalize a bad `HttpClient` pattern.

---

## Mandatory validation commands

Run these in order:

```powershell
dotnet test D:\source\worktrees\tr\squad\copilot-aiel-permissions-04a-contracts\Aiel\tests\Aiel.Permissions.AspNetCore.IntegrationTests\Aiel.Permissions.AspNetCore.IntegrationTests.csproj --no-restore -v minimal
dotnet test D:\source\worktrees\tr\squad\copilot-aiel-permissions-04a-contracts\Aiel\tests\Aiel.Permissions.Application.UnitTests\Aiel.Permissions.Application.UnitTests.csproj --no-restore -v minimal
dotnet test D:\source\worktrees\tr\squad\copilot-aiel-permissions-04a-contracts\Aiel\tests\Aiel.Results.IntegrationTests\Aiel.Results.IntegrationTests.csproj --no-restore -v minimal
dotnet test D:\source\worktrees\tr\squad\copilot-aiel-permissions-04a-contracts\Aiel\tests\Aiel.Results.Generators.UnitTests\Aiel.Results.Generators.UnitTests.csproj --no-restore -v minimal
dotnet test D:\source\worktrees\tr\squad\copilot-aiel-permissions-04a-contracts\Aiel\Aiel.slnx --no-restore -v minimal
```

Interpretation:

- The new ASP.NET Core integration suite is the Task 12 red/green loop.
- The next three commands protect the Task 11 contract boundary and the existing `Result` transport behavior.
- The solution run is the release gate. No warnings. No analyzer suppressions. No "the sample works, but the framework regressed" acceptance.

---

## Rejection conditions

Implementation must be blocked or rejected if any of the following occurs.

### Architecture blockers

1. **Aviendha leakage**
   - Reject any implementation that places the adapter sample in `Aviendha.*` or couples the sample host to Aviendha production services.

2. **Wrong authority source**
   - Reject if the endpoint bypasses `IAppointmentApplicationService` and talks directly to repositories, grant evaluators, or resource authorization services.
   - Reject if the endpoint repeats action-level permission attributes, hard-coded permission policies, or permission-name strings that the Task 11 pipeline already owns.

3. **Wrong transport semantics**
   - Reject if the sample client uses `GetFromJsonAsync`, `ReadFromJsonAsync`, `JsonSerializer.Deserialize`, or equivalent generic JSON APIs for `Result` or `Result<T>` payloads.
   - Reject any `#pragma` or suppression that hides `AIEL00003` instead of fixing the client call.

4. **Generated-endpoint creep**
   - Reject source-generated endpoint/client work, broad endpoint scaffolding, or reusable transport abstractions that reach past the narrow Task 12 proof.
   - Phase 04 decision D7 requires hand-written samples first.

5. **Task 13 drift**
   - Reject capability snapshot contracts, continuation-token work, refresh-on-failure logic, or Blazor visibility helpers in this task. Those belong to Task 13.

6. **Null or exception drift**
   - Reject public null-return shapes, nullable success payloads, or exceptions used for expected validation and denial paths.

### Test and behavior blockers

1. Reject if the endpoint creates more than one `RescheduleAppointment` action or calls the application service more than once per request.
2. Reject if request binding mutates, renames, or drops appointment ID, scope key, or schedule data before forwarding to the action.
3. Reject if the endpoint/client round-trip turns validation or permission denial into an exception, an untyped transport error, or a success response.
4. Reject if the client sample cannot deserialize the endpoint response through `ResultHttpClientExtensions` while keeping the `Result` failure intact.
5. Reject if the Task 11 application tests or the `Aiel.Results.*` regressions fail after the adapter sample lands.
6. Reject if the solution build or test run produces warnings, including analyzer warnings from incorrect `HttpClient` usage.

---

## Implementation owner — route Task 12 to Perrin

This remains framework-owned work.

- The new package name is `Aiel.Permissions.AspNetCore`, which squarely belongs to the Aiel hosting and extension-point layer.
- The task proves transport adapters around the existing framework application-service contract; it does not deliver an Aviendha user journey.
- Perrin owns Aiel framework composition, hosting, and extension points, which is the exact seam Task 12 exercises.

**Owner:** Perrin  
**Reviewer stance:** QA should reject Aviendha-first implementations, duplicate permission enforcement at the endpoint, and any client code that sidesteps `ResultHttpClientExtensions`.

---

## Ready-for-implementation verdict

Task 12 is ready once the owner starts with the four red transport tests above.

The fastest wrong move is to treat this as controller plumbing or to jump ahead to Blazor capability work. The correct move is to prove one narrow server-plus-client transport loop around the existing `RescheduleAppointment` contract and keep the permission pipeline as the single source of truth.
