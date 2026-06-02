# Critical Review — Phase 04 Aiel Permission System

Scope: critique of `Aiel/docs/phases/phase-04-aiel-permission-system.md` only.
Reference: `Aiel/docs/planning/permission-system.md` (accepted design), existing
`Aiel.Application` code, and the repository compliance / architecture rules in
`.github/copilot-instructions.md`.

The phase is sound in direction and well aligned with the action-centered design. The findings below
are about scope, sequencing, package completeness, and a few decisions that the phase still defers to
the implementation PR. Items are ordered by severity.

---

## 1. Phase scope is too large for a single phase (blocker)

Section 4.2 introduces twelve new runtime packages. Tasks 1–11 add those twelve runtime projects plus
at least seven new test projects (contracts, domain.shared, application, analyzers, generators,
EF Core integration, plus a reference slice). On top of that the phase asks for a source generator, a
fail-closed analyzer, an EF Core permission migration DSL with snapshot-diff tooling, ASP.NET Core
middleware, an HTTP client adapter, Blazor visibility helpers, and an Aviendha SAD update.

This is at least three phases of work packaged as one. Two concrete consequences:

- **`F1` / `F3.5` risk.** The "first slice" in 4.5 still depends on contracts, application,
  analyzer, generator, EF Core, EF Core PostgreSQL, ASP.NET Core, client, client.Blazor, testing,
  and analyzers — almost the entire package family. There is no meaningful executable slice before
  Task 9, and no end-to-end "real" call before Task 11.
- **Quality cliff.** With twelve packages added in one phase, the temptation will be to ship stub
  README files just to tick boxes against the completion criteria. That conflicts directly with
  `H7` (XML docs on public packable types) and the "No Technical Debt" core policy.

Recommendation: split into three phases that each deliver a buildable, testable surface:

- **04a — Contracts and gate.** `Aiel.Application.Contracts`, `Aiel.Authorization.Domain.Shared`,
  `Aiel.Authorization.Application.Contracts`, `Aiel.Authorization.Application`,
  `Aiel.Authorization.Analyzers`, `Aiel.Authorization.Testing`. End state: a sample slice exists in
  tests, the analyzer fails closed, no persistence, no generation.
- **04b — Identity and persistence.** `Aiel.Authorization.Domain`, `Aiel.Authorization.Generators`,
  `Aiel.Authorization.EntityFrameworkCore`, `Aiel.Authorization.EntityFrameworkCore.PostgreSql`,
  manifest snapshot + rename migration test.
- **04c — Edges.** `Aiel.Authorization.AspNetCore`, `Aiel.Authorization.Client`,
  `Aiel.Authorization.Client.Blazor`, Aviendha SAD update.

Add a phase-level risk row: "Phase scope is too large to ship cleanly within normal cadence."

---

## 2. `Aiel.Authorization.Domain` is listed in 4.2 but absent from every Task (blocker)

The package family table in 4.2 includes `Aiel.Authorization.Domain` ("Permission catalog and grant
domain model"), but no task in the Implementation Plan creates that project. Tasks 3–11 cover
`Domain.Shared`, `Application.Contracts`, `Application`, `Analyzers`, `Generators`,
`EntityFrameworkCore`, `EntityFrameworkCore.PostgreSql`, `AspNetCore`, `Client`, `Client.Blazor`, and
`Testing`. Where does the `PermissionGrant` aggregate referenced in the design draft live?

This matters because Task 8 implements EF Core mappings and a rename test that must operate on a
grant aggregate. Without `Aiel.Authorization.Domain`, either the aggregate is being conflated with
the EF Core persistence record (violates the design draft's explicit guidance — see lines 102–105 of
the planning doc) or it is being put in `Domain.Shared` (wrong layer for an aggregate).

Recommendation: add Task 3.5 — create `Aiel.Authorization.Domain` with the grant aggregate, catalog
entry, and lifecycle invariants, before Task 8 maps anything.

---

## 3. `IPermissionStore` and `IPermissionManager` are missing from the Tasks (high)

The design draft (planning doc, "Store, Manager, and Decorators") treats `IPermissionManager` as the
consumer-facing API for grant mutation and `IPermissionStore` as the infrastructure contract behind
it. Task 4 only lists `IPermissionGrantEvaluator` (read-side) and Task 8 implements the EF Core
store, but no task ever defines the manager or store contracts.

This causes a concrete problem in Task 8's test gate: "Rename from `ChangeAppointment` to
`RescheduleAppointment` preserves existing grants" requires a way to create those grants first. Today
that path does not exist anywhere in the plan.

Recommendation: in the contracts task (Task 4), add `IPermissionStore` and `IPermissionManager` with
`PermissionErrors.*` failures, and add a manager implementation task before Task 8.

---

## 4. Task 8 sequencing — rename test depends on actions defined in Task 9 (high)

Task 8 requires a test that renames `ChangeAppointment` to `RescheduleAppointment` and proves grants
survive. Task 9 is the task that defines `RescheduleAppointment` as a reference slice.

Two options, either is fine, but pick one explicitly:

- Make Task 8 use **inline test fixture action types** with their own lifecycle, fully independent
  of the Task 9 reference slice. Note this in the task body.
- Move the Task 9 action declarations forward into a smaller "fixture only" Task 8.5 so the rename
  test can reference them by name.

As written, the order reads as a circular dependency.

---

## 5. Task 6 — the analyzer's third valid state is not testable until Task 7 (medium)

Task 6 lists three accepting cases for the missing-authorization analyzer: concrete checker,
generated permission definition, explicit permission-free marker. The test gate explicitly covers
only the first and third. The second cannot be exercised until generators exist (Task 7). The phase
should either:

- Explicitly defer the "generated permission accepted" analyzer test to Task 7, or
- Move the analyzer after the generator.

I prefer the first because it preserves the fail-closed-first ordering, but the deferral must be
written down or the implementer will either skip it (silent gap) or fake a generator stub in Task 6
(rework).

---

## 6. Task 0 is stale; the design draft already resolved the marker mismatch (medium)

Task 0 / Decision D3 instruct the implementer to "normalize the design draft" because one example
still shows `[AllowWithoutPermission]`. A grep of `Aiel/docs/planning/permission-system.md` shows
only `DoesNotRespectAuthority` in both prose and code (lines 362 and 366). The stale `AllowWithout-`
text lives in the worktree copy and the historical review files only.

Recommendation: replace Task 0 with a one-line verification step ("confirm planning draft uses
`DoesNotRespectAuthority` exclusively; if so, this task is a no-op"). Update D3 to state the marker
name is selected: `DoesNotRespectAuthority`, with a non-empty `Reason` required by the analyzer.

---

## 7. Task 2 defers a public-API decision the phase must own (medium)

Task 2: "Decide in the PR whether old interface locations are removed immediately or kept
temporarily as type-forwarding or compatibility shims."

This is a public-API breaking-change decision and the repository compliance rules require it to be
surfaced and decided up front (Critical compliance rule on backward compatibility; `AA1` requires an
Ask for `PubAPIChange`). Deferring it to the PR pushes the policy gate downstream and forces the PR
author to interrupt.

Concrete observation: `Aiel.Application` already exposes `Aiel.Commands.ICommand`,
`Aiel.Commands.ICommandHandler<T>`, `Aiel.Queries.IQuery<T>`, dispatchers, and pipeline
behaviors. The new `ICommand : IAction` in `Aiel.Application.Contracts` is not assembly-equivalent
to the existing parameterless marker.

Recommendation: pick a strategy now and write it into D2. Three viable options:

- **Move + type-forward** the existing `ICommand` / `IQuery<T>` symbols into the new contracts
  assembly. Lowest churn for consumers.
- **Inherit-down bridge**: have the existing `Aiel.Commands.ICommand` inherit from the new
  `Aiel.Application.Contracts.ICommand`. Keeps both names valid for one release.
- **Breaking move** with a migration note in the package README and a one-release deprecation cycle.

Any of these is defensible. None of them is "decide later."

---

## 8. `IExecutionContext` open question is unresolved but Task 1 ships its contract (medium)

The planning draft explicitly flags an open question: should `IExecutionContext` become a
framework-owned sealed class to protect invariants? That decision changes whether Task 1 emits an
interface (today's shape) or a sealed class with a factory. It also affects how
`IActionExecutionContext<TAction>` is implemented (Task 1's "or factory, depending on the final
execution-context decision" leaves it ambiguous in the deliverable itself).

Recommendation: add a D6 that picks one of {keep interface, move to sealed class, both with the
interface marked for deprecation}. Without this decision, Task 1's deliverable shape is undefined and
Task 5's gate implementation can't choose a `CreateChild` signature.

---

## 9. Completion criteria overstate what some tasks deliver (low / medium)

Two mismatches:

- "Generated endpoint/client sample calls the application service contract" — Task 10 explicitly
  permits "Generated **or** sample endpoint code" and "Generated **or** sample client code", and the
  Out-of-Scope list confirms "Broad generated endpoint coverage beyond the first
  `RescheduleAppointment` sample" is out. The completion criterion should say "Endpoint and HTTP
  client sample" so a hand-written sample is unambiguously sufficient.
- 4.5 lists "one generated endpoint/client pair" as part of the first slice, while Task 10 allows
  sample. Reconcile to one phrasing.

This matters because the completion checklist is what unblocks a release; ambiguous criteria force
the implementer to either over-deliver or argue at PR time.

---

## 10. `Aiel.Authorization.Testing` has no creation task (low)

Listed in 4.2 and first **used** in Task 9 ("Use `Aiel.Authorization.Testing` for shared sample IDs
and fake services"), but no task creates the project. Either add a creation step inside Task 9 or
add a small Task 4.5.

---

## 11. Solution / virtual-folders / Directory.Packages.props updates unmentioned (low)

Adding twelve src projects and seven test projects requires updates to `Aiel.slnx`,
`virtual-folders.json`, and any central package version pins. The phase should at least mention this
once so the first PR scope is realistic and so the implementer doesn't merge a partially listed
solution.

---

## 12. Naming and packaging convention not pinned (low)

The phase introduces `Aiel.Authorization.Domain.Shared` as a sibling under the permission family.
Today there is no `Aiel.Domain.Shared` — the convention is being established here. Worth one
sentence noting that future feature families should follow the same `<Family>.Domain.Shared`,
`<Family>.Application.Contracts`, `<Family>.Application` pattern so it doesn't drift.

Likewise, the generator packaging note in Task 7 correctly points at the `Aiel.Results` /
`Aiel.StrongIds` pattern (sibling generator DLL under `analyzers/dotnet/cs`, `ProjectReference
OutputItemType="Analyzer"`). Good. Keep that explicit when 04b lands so the implementer doesn't
reinvent it.

---

## 13. Minor — Aviendha consumption is in Task 12 but not in completion criteria

Task 12 updates `Aviendha/docs/SAD.md`. None of the completion criteria reference Aviendha. That is
probably correct — Aviendha rollout is explicitly out of scope — but the SAD update is the only
externally visible artifact for Aviendha planning, and dropping it from the completion checklist
makes it the first thing skipped under deadline pressure.

Either pull the SAD update out of Phase 04 entirely (cleanest, given Aviendha rollout is out of
scope) or add it to the checklist.

---

## Summary

The action-centered direction is correct. The phase document needs three structural changes before
implementation begins:

1. Split the phase. Twelve packages plus generator plus analyzer plus EF migration DSL plus client
   helpers in one phase will either fail to ship or ship as stubs.
2. Fill the gaps. `Aiel.Authorization.Domain`, `IPermissionStore`, `IPermissionManager`, and the
   `Aiel.Authorization.Testing` creation step all need explicit tasks.
3. Decide what is currently deferred. The marker name (already decided in the planning doc), the
   CQRS compatibility strategy (Task 2), and the `IExecutionContext` shape (planning open question)
   should all be resolved in `Decisions` before Task 1 starts. The repository's `AA1` gate requires
   it for the first two.

After those changes, the implementation plan reads as a clean sequence that respects fail-closed
ordering and keeps the gate explicit.
