# Critical Review — Phase 04 Aiel Permission System (Rev 2)

Scope: critique of `Aiel/docs/phases/phase-04-aiel-permission-system.md` only.

Reference: `Aiel/docs/planning/permission-system.md` (accepted design), existing
`Aiel.Application` code, `Aviendha/docs/SAD.md` (authoritative but unstable), and the repository
compliance / architecture rules in `.github/copilot-instructions.md`.

**Operating context** (updated):

- Aiel and Aviendha are greenfield. Aiel has zero external consumers; any "compatibility shim"
  or "breaking migration" framing is moot.
- Existing Aviendha source is scratch for planning — ignore it. `Aviendha/docs/SAD.md` is the
  authoritative reference and is still unstable.
- Implementation will be carried out by the `.squad` of Copilot agents. Agent throughput is high;
  human review bandwidth (Doug) is the bottleneck.
- The repository "No Technical Debt" core policy applies. Under that policy, type-forwarding shims,
  duplicate-and-deprecate paths, and "kept temporarily" wrappers are themselves debt and are not
  acceptable.

What changed since Rev 1:

- **Finding #1 (scope)** softens substantially. Re-cast as a review-checkpoint concern, not a
  person-weeks concern.
- **Finding #7 (Task 2 public-API decision)** collapses. With zero consumers and Zero Tech Debt,
  the only acceptable strategy is "move and delete." Shims would violate policy.
- **Finding #8 (`IExecutionContext` shape)** softens but does not collapse. The decision still needs
  to be made before Task 1, but the cost of revisiting is zero.
- **New finding #14 (Aviendha SAD update scope)** added in light of the SAD being authoritative.
- **New finding #15 (under-specification with agent squad)** added. With a fast agent team, the
  larger risk is no longer "scope too big to finish" — it is "spec gaps get filled with arbitrary
  choices at scale, fast."

Findings are ordered by remaining severity after the revision.

---

## 1. `Aiel.Authorization.Domain` is listed in §4.2 but absent from every Task (blocker)

Unchanged from Rev 1.

The package family table in 4.2 includes `Aiel.Authorization.Domain` ("Permission catalog and grant
domain model"), but no task in the Implementation Plan creates that project. Tasks 3–11 cover
`Domain.Shared`, `Application.Contracts`, `Application`, `Analyzers`, `Generators`,
`EntityFrameworkCore`, `EntityFrameworkCore.PostgreSql`, `AspNetCore`, `Client`, `Client.Blazor`, and
`Testing`. Where does the `PermissionGrant` aggregate referenced in the design draft live?

Task 8 implements EF Core mappings and a rename test that must operate on a grant aggregate. Without
`Aiel.Authorization.Domain`, either the aggregate is conflated with the EF Core persistence record
(violates the planning doc's explicit guidance) or it is being put in `Domain.Shared` (wrong layer
for an aggregate, violates `A2`).

Recommendation: add Task 3.5 — create `Aiel.Authorization.Domain` with the grant aggregate, catalog
entry, and lifecycle invariants, before Task 8 maps anything.

---

## 2. `IPermissionStore` and `IPermissionManager` are missing from the Tasks (blocker)

Unchanged from Rev 1.

The design draft treats `IPermissionManager` as the consumer-facing API for grant mutation and
`IPermissionStore` as the infrastructure contract behind it. Task 4 only lists
`IPermissionGrantEvaluator` (read-side) and Task 8 implements the EF Core store, but no task ever
defines the manager or store contracts.

Concrete problem in Task 8's test gate: "Rename from `ChangeAppointment` to `RescheduleAppointment`
preserves existing grants" requires a way to create those grants first. Today that path does not
exist anywhere in the plan.

Recommendation: in Task 4, add `IPermissionStore` and `IPermissionManager` with `PermissionErrors.*`
failures. Add a manager implementation task before Task 8.

---

## 3. Task 8 sequencing — rename test depends on actions defined in Task 9 (high)

Unchanged from Rev 1.

Task 8 requires a test that renames `ChangeAppointment` to `RescheduleAppointment` and proves grants
survive. Task 9 is the task that defines `RescheduleAppointment`.

Pick one explicitly:

- Task 8 uses **inline test fixture action types** with their own lifecycle, fully independent of
  the Task 9 reference slice. Note this in the task body.
- Move the Task 9 action declarations forward into a smaller "fixture only" Task 8.5.

As written, the order reads as a circular dependency. An agent assigned Task 8 will either invent
the action types and trigger a merge conflict with Task 9, or block waiting for Task 9.

---

## 4. Task 6 — the analyzer's third valid state is not testable until Task 7 (high — bumped from medium)

Task 6 accepts three states for the missing-authorization analyzer: concrete checker, generated
permission definition, explicit permission-free marker. The test gate covers only the first and
third. The second cannot be exercised until generators exist (Task 7).

Bumped to high because with an agent squad, an unspecified-but-implied test case is exactly the kind
of gap that gets either silently skipped (gap ships) or faked with a hand-rolled stub that imitates
generator output (the stub then ships as "the generator contract" and constrains Task 7).

Resolution: explicitly state in Task 6 that the "generated permission accepted" analyzer test is
deferred and add it as an explicit test gate to Task 7. Do not let it remain implicit.

---

## 5. `IExecutionContext` shape decision still needs to land before Task 1 (medium — softened)

The planning draft flags an open question: should `IExecutionContext` become a framework-owned
sealed class? With zero consumers, the cost of revisiting later is zero, so this is no longer a
blocker.

It is still worth picking before Task 1 because Task 5's `IActionGate<TAction>` implementation needs
a `CreateChild`-style signature, and `ActionExecutionContext<TAction>` in Task 1 has to either be an
interface implementation or a sealed-class derivative.

Recommendation: pick the simpler option (interface today) in a new D6 and note that hardening to a
sealed class is an allowed Phase 04b/04c refactor. With Zero Tech Debt and no consumers, the right
answer is the one that lets the slice ship.

---

## 6. Task 2 is now "move and delete" — Zero Tech Debt forbids shims (medium — re-scoped)

Rev 1 flagged Task 2 as deferring a public-API decision. With zero consumers, there is no API
decision. The Zero Tech Debt policy makes the answer concrete:

- Move `Aiel.Commands.ICommand`, `Aiel.Queries.IQuery<T>`, `IExecutionContext`,
  `ICommandHandler<T>`, `IQueryHandler<T>`, dispatcher interfaces, and pipeline behavior interfaces
  to `Aiel.Application.Contracts`.
- Delete the old locations in the same PR.
- Update every reference in `Aiel.Application` and tests in the same PR.

No type-forwarding. No `[Obsolete]`. No "kept temporarily." Those would be debt.

The phase doc should rewrite Task 2 to state this directly: "Move the contracts. Delete the old
locations. Update consumers in the same change." Drop the "decide in the PR" language from D2.

---

## 7. Completion criteria overstate what some tasks deliver (medium)

Unchanged from Rev 1.

- "Generated endpoint/client sample calls the application service contract" — Task 10 explicitly
  permits "Generated **or** sample endpoint code" and "Generated **or** sample client code". The
  out-of-scope list confirms broad generation is out. The criterion should read "Endpoint and HTTP
  client sample" so a hand-written sample is unambiguously sufficient.
- §4.5 lists "one generated endpoint/client pair" as part of the first slice, while Task 10 allows
  sample. Reconcile to one phrasing.

Bumped relevance: with an agent squad, completion checklists drive behavior. An agent assigned to
"complete the phase" will read the criterion literally and either build a generator that's out of
scope or loop arguing with the task body.

---

## 8. Phase scope — split for review gates, not for person-weeks (low — soft recommendation)

Substantially softened from Rev 1.

Original concern was that twelve packages plus generators plus analyzers plus EF migration DSL plus
ASP.NET Core plus Blazor in one phase was too much typing for a normal cadence. With an agent squad
that is no longer true; mechanical scaffolding is cheap, and independent packages can be built in
parallel.

The remaining argument for splitting is **review surface area**. Human review (Doug) is the
bottleneck, and a twelve-package phase produces either one un-reviewable mega-PR or a serial PR
train whose acceptance ordering matters. The split also gives each layer a chance to be accepted
before the next layer commits to its shape.

Recommended split if it is wanted — purely for review checkpoints, not for effort:

- **04a — Contracts and gate.** `Aiel.Application.Contracts`, `Aiel.Authorization.Domain.Shared`,
  `Aiel.Authorization.Domain`, `Aiel.Authorization.Application.Contracts`,
  `Aiel.Authorization.Application`, `Aiel.Authorization.Analyzers`, `Aiel.Authorization.Testing`.
  Reference slice lives in tests.
- **04b — Identity and persistence.** `Aiel.Authorization.Generators`,
  `Aiel.Authorization.EntityFrameworkCore`, `Aiel.Authorization.EntityFrameworkCore.PostgreSql`,
  manifest snapshot + rename migration test.
- **04c — Edges.** `Aiel.Authorization.AspNetCore`, `Aiel.Authorization.Client`,
  `Aiel.Authorization.Client.Blazor`, Aviendha SAD update.

If the agent squad is willing to land 04a in many small PRs (one per package boundary), keeping it
as a single phase is also defensible. The decision is about how Doug wants to review, not about how
fast the team can build.

---

## 9. Task 0 / D3 — marker name is already resolved in the planning doc (low)

A grep of `Aiel/docs/planning/permission-system.md` shows only `DoesNotRespectAuthority` in both
prose and code. The stale `[AllowWithoutPermission]` references live in worktree copies and old
review files only.

Recommendation: replace Task 0 with a one-line verification step. Update D3 to state the marker name
is selected: `DoesNotRespectAuthority`, with a non-empty `Reason` required by the analyzer.

---

## 10. `Aiel.Authorization.Testing` has no creation task (low)

Listed in §4.2 and first **used** in Task 9, but no task creates the project. Either add a creation
step inside Task 9 or add a small Task 4.5. With an agent squad, an unowned package will either get
silently created with default settings by whichever agent touches Task 9 first, or get skipped.

---

## 11. Solution / virtual-folders / Directory.Packages.props updates unmentioned (low)

Adding ~12 src projects and ~7 test projects requires updates to `Aiel.slnx`,
`virtual-folders.json`, and any central package version pins. Worth mentioning once so the first PR
scope is realistic and so the implementer doesn't merge a partially listed solution.

Agents will do this work, but they need to know it is in scope.

---

## 12. Naming and packaging convention not pinned (low)

The phase introduces `Aiel.Authorization.Domain.Shared` as a sibling under the permission family,
establishing a convention. Worth one sentence: "Future feature families follow
`<Family>.Domain.Shared`, `<Family>.Application.Contracts`, `<Family>.Application`."

Likewise, Task 7 should keep the explicit pointer at the `Aiel.Results` / `Aiel.StrongIds`
generator-packaging pattern (sibling generator DLL under `analyzers/dotnet/cs`, `ProjectReference
OutputItemType="Analyzer"`).

---

## 13. Aviendha SAD update — scope vs. timing (low — revised)

`Aviendha/docs/SAD.md` is authoritative but unstable. Task 12 currently asks for a SAD update
covering "the action-based permission matrix guidance Aviendha should follow during adoption."

Two concerns:

- Updating an unstable authoritative doc inside an out-of-scope rollout phase is a recipe for the
  SAD change either drifting from the eventual Aviendha rollout or pre-committing the SAD to
  decisions that haven't been validated yet.
- The phase already lists "Full Aviendha production permission rollout" as out of scope.

Recommendation: narrow Task 12's SAD edit to a **pointer**, not a guidance section. Something like:
"§5 Identity & Authorization now references `Aiel.Authorization.*` as the chosen permission
mechanism; the action-based permission matrix and Safety Plan permission boundaries will be
specified when Aviendha permission adoption is scoped." Leave the actual guidance to whichever phase
owns Aviendha permission rollout.

---

## 14. Under-specification is now the dominant risk (high — new)

This is the cross-cutting finding the rest of the review points at.

With the Copilot agent squad, ambiguity in the phase doc gets filled with arbitrary choices at the
speed the agents work. Twelve packages of arbitrary choices is a lot of decisions to walk back. The
phase document needs to be tighter than it would be for a human team, in three specific places:

- **Every "decide in the PR" must become a decision.** Today: Task 2 (collapses per finding #6),
  Task 1's "or factory, depending on the final execution-context decision" (resolve per finding
  #5), Task 10's "Generated or sample" (pick one per finding #7).
- **Every implied-but-untested behavior must become an explicit test gate** or be explicitly
  deferred. Today: Task 6's third analyzer state (finding #4), Task 8's rename test fixture origin
  (finding #3), the "permission manager / store" path needed to make Task 8 testable (finding #2).
- **Every entity in §4.2 must have a creation task.** Today: `Aiel.Authorization.Domain` (finding
  #1), `Aiel.Authorization.Testing` (finding #10).

The Zero Tech Debt policy depends on this. Agents asked to "complete the task with no warnings" can
satisfy that by deleting the warning rather than fixing the cause. The phase doc is the only place
to forbid that path in advance.

---

## Summary

The action-centered direction is correct. Two findings became less severe once the consumer story
and team makeup were clarified. The remaining blockers are about **specification completeness**, not
scope or sequencing:

1. **Spec gaps.** `Aiel.Authorization.Domain`, `IPermissionStore`, `IPermissionManager`, and
   `Aiel.Authorization.Testing` all need explicit tasks before any agent picks up Task 4 or Task 8.
2. **Sequencing.** Task 8 must explicitly own its rename-test fixture (independent of Task 9). Task
   6 must explicitly defer the generated-permission analyzer case to Task 7.
3. **Decisions, not deferrals.** With an agent squad and Zero Tech Debt, "decide in the PR" is the
   wrong default. The marker name, the CQRS contract move (now: move and delete, no shims), the
   execution-context shape, and the generated-vs-sample endpoint choice all need to be settled in
   `Decisions` before Task 1 starts.

If the phase doc is tightened on those three axes, splitting into 04a/04b/04c becomes a question of
review preference, not a question of feasibility.
