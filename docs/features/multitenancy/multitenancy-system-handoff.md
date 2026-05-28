# Plan: Aiel Multitenancy Handoff

**Status (as of issue #20):** All major steps completed in issues #16–#19. See updated [`multitenancy-system.md`](./multitenancy-system.md) and [`phase-03-aiel-aspnet-operational-plan.md`](./phase-03-aiel-aspnet-operational-plan.md) for landed contracts and application responsibilities.

---

Aiel should receive the reusable framework-level multitenancy work first, with Aviendha acting as the first validation app rather than the authority for Aiel API shape. Existing multitenancy code was created to facilitate discussion and may be kept, replaced, or removed. The recommended approach is to reconcile the current docs, redesign the public multitenancy vocabulary around clear framework names, harden the contracts into explicit non-null outcomes, add ASP.NET Core fail-closed resolution primitives, then extend EF Core migration/data-access support for discriminator and database-per-tenant models behind reusable abstractions. Aviendha adoption should follow as a downstream proof, not as framework source-of-truth.

**Steps**

1. Alignment, naming, and doc reconciliation — Moiraine primary, Loial secondary, Verin review.
   - Confirm the authority order: Aiel framework design decisions are reusable and open-source-oriented; Aviendha requirements are design inputs and validation scenarios.
   - Reconcile `d:/source/TwoRivers/Aiel/docs/planning/multitenancy-system.md` with `d:/source/TwoRivers/Aiel/docs/DomainPrimitives.md` and `d:/source/TwoRivers/Aiel/docs/phases/phase-03-aiel-aspnet-operational-plan.md`.
   - Treat existing multitenancy code as provisional. Keep only names and shapes that still read well for a reusable open-source framework.
   - Naming recommendation: reserve `TenantId` for the identifier value object/scalar. Replace `TenantContext` if the wrapper carries more than the raw id; preferred candidate is `TenantIdentity` for resolved tenant identity plus optional routing/display hints, with `CurrentTenant`/`TenantScope`/`TenantResolution` used for scoped access and outcome concepts as appropriate.
   - Rename `TrDbContext` to `AielDbContext` as part of the same public vocabulary cleanup, with compatibility only if Moiraine decides it is worth keeping during greenfield development.
   - Mark older assumptions for removal: tuple-like resolver results, nullable `ITenantProvider`, public `tenantId` claim trust, public tenant headers, storage details inside tenant identity, schema-per-tenant as first-class default.
   - Deliverable: short ADR or decision note in `.squad/decisions/inbox/moiraine-aiel-multitenancy-contract.md` before coding starts.
   - Blocks steps 2-5.

2. Contract and naming tests first — Verin primary, Perrin secondary.
   - Add/adjust tests that fail against current `Aiel.MultiTenancy` because `ITenantProvider.GetCurrentTenantAsync` returns `TenantContext?`, tenant outcomes are not explicit, and provisional names do not represent the intended public vocabulary.
   - Test the desired public contract: `Resolved`, `None/Missing/NotApplicable`, `Forbidden`, `Ambiguous`, and `Error`/control-plane-failure outcomes, with no nullable public return values.
   - Test that identifier, identity, scoped accessor, and resolution outcome concepts are separate in the API surface: `TenantId` should not mean the same thing as the resolved current tenant.
   - Test constants policy: `sub` as the stable external subject claim, `X-Tenant-ID` reserved for internal privileged overrides, host/domain as hint only.
   - Keep these as contract/unit tests, not ASP.NET integration tests yet.
   - Parallel with step 3 after step 1.

3. Redesign `Aiel.MultiTenancy` reusable contracts — Perrin primary, Moiraine review.
   - Replace nullable tenant resolution with explicit outcome types, likely `TenantResolution`/`TenantResolutionResult` plus a typed outcome, consistent with existing Aiel result/error patterns.
   - Introduce or confirm a `TenantId` identifier type. If the strong-id generator is ready, prefer a framework-level strong `TenantId`; otherwise use scalar `Guid` temporarily and document the intended upgrade path.
   - Replace `TenantContext` with a clearer name unless Moiraine decides to keep it. Recommended split: `TenantId` for the identifier, `TenantIdentity` for resolved tenant identity plus optional domain/routing hint, `CurrentTenant` or `ITenantAccessor` for request/job-scoped access, and `TenantScope` for an explicit ambient binding if background jobs need one.
   - Keep tenant identity free of connection strings, storage model, actor metadata, secret references, and Aviendha-specific concepts.
   - Add XML docs to public packable types.
   - Preserve package independence and avoid references from `Aiel.MultiTenancy` to Aviendha or infrastructure packages.
   - Depends on step 1; uses tests from step 2.

4. ASP.NET Core tenant pipeline design and tests — Verin primary for tests, Perrin primary for implementation, Moiraine review.
   - Add tenant-required/tenant-optional endpoint metadata abstractions in `Aiel.AspNetCore`.
   - Add scoped tenant resolution accessors/middleware that run after authentication and before tenant-scoped handlers.
   - The framework middleware should enforce explicit outcomes and fail closed on tenant-required endpoints; host/domain hints and internal headers are inputs to host-provided resolvers, not authorization decisions by themselves.
   - Provide host extension points so any application, including Aviendha, can resolve actor and active tenant policy outside Aiel.
   - Integration tests should cover resolved tenant, missing tenant on required endpoint, optional endpoint, error outcome, and conflict/forbidden outcome mapping.
   - Depends on steps 2-3.

5. EF Core base-context and discriminator safety pass — Verin primary for isolation tests, Perrin primary for code.
   - Rename `TrDbContext` to `AielDbContext` and update docs/tests accordingly; because this is greenfield, prefer the clean public name over compatibility shims unless Moiraine asks for them.
   - Update the renamed base context to consume the new non-null tenant-resolution contract from the scoped tenant accessor/provider.
   - Preserve existing discriminator behavior: `IMultiTenant` query filters and save-time tenant stamping, unless the contract redesign replaces `IMultiTenant` with a clearer marker name.
   - Add tests for provider-returned no-tenant/error outcomes, tenant-scoped query behavior, save stamping, and no cross-tenant leakage.
   - Decide whether tenant-required DbContexts should fail closed when the provider returns no tenant, while allowing explicitly non-tenant/control-plane DbContexts to opt out.
   - Depends on step 3; can run partly parallel with step 4 after contract shape is stable.

6. EF Core database-per-tenant and migration primitives — Perrin primary, Nynaeve secondary, Verin review.
   - Define reusable abstractions for tenant migration targets, target version/readiness, migration ledger records, rollout locks/leases, and per-target migrators without imposing Aviendha's catalog schema.
   - Keep production bulk migration execution out of web startup; allow startup migration only for local development, tests, and explicit single-tenant provisioning.
   - Add tests for target selection, checkpoint/resume behavior, failed-target handling, and no fan-out during normal app startup.
   - Provide telemetry hooks and structured result outcomes without logging secrets or connection strings.
   - Depends on step 5 for EF contract compatibility; Nynaeve can draft operational acceptance criteria in parallel.

7. Documentation and operator guidance — Loial primary, Nynaeve secondary, Moiraine review.
   - Update Aiel docs to describe the framework contract, supported patterns, extension points, and what Aiel deliberately does not own.
   - Include a developer-facing guide for discriminator and database-per-tenant usage.
   - Include production migration guidance: out-of-band runner, expand/contract migrations, per-target checkpoints, and startup restrictions.
   - Include an Aviendha validation note, clearly labeled as an example/reference implementation, not the normative framework design.
   - Depends on steps 3-6 but can start draft outlines after step 1.

8. Aviendha validation planning — Elayne primary, Perrin secondary, Verin review.
   - Treat this as downstream validation after Aiel contracts land.
   - Map Aviendha's `sub -> ActorContext -> active tenant -> TenantContext -> TenantStoreBinding` flow onto the Aiel extension points.
   - Keep `TenantStoreBinding`, control-plane catalog schema, membership policy, tenant switching UX, and per-tenant database binding in Aviendha.
   - Add integration acceptance tests for tenant-user, Client Area, bootstrap/recovery separation, and internal operator override semantics.
   - Depends on step 4; detailed implementation should be separate issues/PRs from Aiel framework work.

9. Squad issue breakdown and review gates — coordinator/Moiraine.
   - Create one GitHub issue per phase or per independently reviewable package slice.
   - Label initial architecture issue with `squad` for Moiraine triage; implementation issues should route to `squad:perrin`, test strategy to `squad:verin`, ops to `squad:nynaeve`, docs to `squad:loial`, Aviendha adoption to `squad:elayne`.
   - Every implementation issue must state target layer, public API impact, files, tests-first gate, verification commands, and review owner.
   - Do not hand work to Doug for QA until a PR has clean build and passing tests.

**Squad issue sequence**

1. `ADR: Aiel multitenancy vocabulary and boundaries` — Moiraine primary, Loial secondary, Verin review.
   - Decide final names for tenant identifier, resolved tenant identity, tenant resolution outcome, scoped tenant accessor, tenant binding/scope, and EF base context.
   - Recommended defaults: `TenantId`, `TenantIdentity`, `TenantResolution`, `ITenantAccessor` or `ICurrentTenant`, `TenantScope`, and `AielDbContext`.
   - Explicitly state that current `TenantContext`, `ITenantProvider`, and `TrDbContext` are provisional and may be replaced.
   - Acceptance gate: ADR is merged into `.squad/decisions.md` or placed in `.squad/decisions/inbox/` with Moiraine approval before implementation begins.

2. `Aiel.MultiTenancy: explicit tenant identity contract` — Perrin primary, Verin tests, Moiraine review.
   - Add failing tests for explicit resolution outcomes and naming separation.
   - Implement the selected tenant identity/resolution/accessor contracts.
   - Remove nullable public tenant resolution returns.
   - Acceptance gate: focused unit tests fail first, then pass; no Aviendha references; public XML docs are present.

3. `Aiel.EntityFrameworkCore: rename base context and align tenant contract` — Perrin primary, Verin tests.
   - Rename `TrDbContext` to `AielDbContext`.
   - Update discriminator query filters and tenant stamping to the selected tenant identity contract.
   - Decide whether compatibility type aliases/shims are unnecessary because the project is greenfield.
   - Acceptance gate: EF integration tests cover tenant stamping, query isolation, no-tenant/error outcomes, and no cross-tenant leakage.

4. `Aiel.AspNetCore: tenant-required endpoint pipeline` — Perrin primary, Verin tests, Moiraine review.
   - Add endpoint metadata and middleware for tenant-required and tenant-optional routes.
   - Enforce explicit tenant outcomes fail-closed.
   - Keep actor resolution and storage binding host-owned through extension points.
   - Acceptance gate: ASP.NET integration tests cover resolved, missing, optional, forbidden/conflict, and error outcomes.

5. `Aiel.EntityFrameworkCore: per-tenant migration primitives` — Perrin primary, Nynaeve secondary, Verin review.
   - Define target-scoped migration abstractions, ledger/checkpoint contracts, rollout locks/leases, readiness contributors, and telemetry hooks.
   - Keep production fan-out migrations out of normal web startup.
   - Acceptance gate: tests prove startup does not enumerate tenant targets and out-of-band runner checkpoint/resume behavior is explicit.

6. `Aiel docs: reusable multitenancy guide` — Loial primary, Moiraine review.
   - Update planning and developer docs to reflect final names, boundaries, discriminator usage, database-per-tenant patterns, and migration safety.
   - Label Aviendha as a reference implementation / validation app, not the normative design authority.
   - Acceptance gate: docs match implemented contracts and do not promise admin UI/CLI before those are separately planned.

7. `Aviendha validation plan for Aiel tenancy contracts` — Elayne primary, Perrin secondary, Verin review.
   - Map Aviendha's actor, active tenant, and store-binding flow onto the Aiel extension points after framework contracts land.
   - Keep Aviendha-specific catalog, membership, and tenant database binding out of Aiel.
   - Acceptance gate: app-level acceptance tests are planned separately from Aiel framework PRs.

**Relevant files**

- `d:/source/TwoRivers/Aiel/docs/planning/multitenancy-system.md` — current high-level multitenancy draft to reconcile into an implementation-ready Aiel plan.
- `d:/source/TwoRivers/Aiel/docs/DomainPrimitives.md` — current source-of-truth language for identity-only `TenantContext`, explicit tenant outcomes, and EF strategy.
- `d:/source/TwoRivers/Aiel/docs/phases/phase-03-aiel-aspnet-operational-plan.md` — ASP.NET and migration operating model to preserve.
- `d:/source/TwoRivers/Aiel/src/Aiel.MultiTenancy/Aiel/MultiTenancy/TenantContext.cs` — provisional identity wrapper; likely replace with `TenantId` plus `TenantIdentity`/scoped-accessor vocabulary.
- `d:/source/TwoRivers/Aiel/src/Aiel.MultiTenancy/Aiel/MultiTenancy/ITenantProvider.cs` — provisional nullable contract to replace with explicit outcomes and clearer resolver/accessor naming.
- `d:/source/TwoRivers/Aiel/src/Aiel.MultiTenancy/Aiel/MultiTenancy/IMultiTenant.cs` — discriminator marker used by EF query filters and tenant stamping; keep or rename only after the core naming ADR.
- `d:/source/TwoRivers/Aiel/src/Aiel.MultiTenancy/Aiel/MultiTenancy/TenantResolutionConstants.cs` — constants that must align with trust-boundary decisions.
- `d:/source/TwoRivers/Aiel/src/Aiel.EntityFrameworkCore/Aiel/EntityFrameworkCore/TrDbContext.cs` — provisional EF base context; rename to `AielDbContext`.
- `d:/source/TwoRivers/Aiel/src/Aiel.EntityFrameworkCore/Aiel/EntityFrameworkCore/ModelBuilderExtensions.cs` — existing global query-filter implementation.
- `d:/source/TwoRivers/Aiel/tests/Aiel.EntityFrameworkCore.IntegrationTests/Aiel/EntityFrameworkCore/TrDbContextTests.cs` — current discriminator tests to extend.
- `d:/source/TwoRivers/Aiel/src/Aiel.AspNetCore/Aiel/AspNetCore/AielAspNetCore.cs` — currently marker-only ASP.NET package; primary home for HTTP middleware/metadata.
- `d:/source/TwoRivers/.squad/decisions.md` — existing squad decisions, including May 10 tenant lifecycle and migration decisions.
- `d:/source/TwoRivers/.squad/routing.md` — owner routing for phase/issues.
- `d:/source/TwoRivers/.squad/skills/tenant-contract-boundaries/SKILL.md` — useful boundary rules, but subordinate to Aiel framework architecture decisions.
- `d:/source/TwoRivers/Aviendha/docs/planning/MULTITENANCY.md` — downstream reference implementation pressure test, not normative for Aiel.

**Verification**

1. Before implementation: confirm a Moiraine ADR/decision exists and resolves the public API direction for tenant identifier, tenant identity wrapper, scoped tenant accessor/resolver, resolution outcomes, constants, `AielDbContext`, and storage ownership.
2. For each implementation PR: require the new or changed tests to fail before implementation, then pass after implementation.
3. Run `Set-Location d:/source/TwoRivers/Aiel; dotnet build Aiel.slnx -c Debug -warnaserror`.
4. Run `Set-Location d:/source/TwoRivers/Aiel; dotnet test Aiel.slnx -c Debug --no-build`.
5. For ASP.NET middleware work: add integration tests using a test host/web application covering required, optional, missing, forbidden/conflict, and error outcomes.
6. For EF discriminator work: run `Aiel.EntityFrameworkCore.IntegrationTests` and verify no cross-tenant leakage and no nullable public tenant outcomes remain.
7. For migration primitives: add tests showing production startup does not enumerate/fan out tenant targets and that out-of-band runner checkpoints/resumes correctly.
8. For documentation: ensure Aiel docs distinguish reusable framework contracts from Aviendha-specific adoption and do not promise admin UI/CLI behavior before contracts exist.

**Decisions**

- Aiel is reusable framework code and eventual open-source API; Aviendha is a first reference/commercial implementation and design influence, not an authority over Aiel.
- Do not call the whole request-scoped concept `TenantId`. `TenantId` should name the identifier only; use a separate name such as `TenantIdentity`, `CurrentTenant`, `TenantScope`, or `TenantResolution` for richer concepts.
- Storage topology, connection strings, secret references, actor context, membership policy, tenant switching UX, and catalog schema are application-owned or provider-owned concerns.
- Support discriminator and database-per-tenant as first-class Aiel patterns through reusable abstractions and EF helpers; do not bake Aviendha's control-plane table names or onboarding saga into Aiel.
- Use explicit resolution outcomes instead of nullable public return values.
- Production migrations are out-of-band deploy/provisioning work, not normal web startup fan-out.
- Schema-per-tenant should remain optional/provider-extensible rather than a recommended first-class default unless Moiraine reopens that decision.

**Further Considerations**

1. Public API break: changing `ITenantProvider`, replacing `TenantContext`, or renaming `TrDbContext` is technically breaking, but both projects are greenfield. The implementation issue should call this out and get Moiraine approval before code changes.
2. Strong `TenantId`: Aiel has strong-id design/generator work in flight, while current `IMultiTenant` uses scalar `Guid` for EF and infrastructure compatibility. Prefer a strong framework-level `TenantId` if the generator is ready; otherwise use scalar `Guid` temporarily and document the migration path.
3. Admin tooling scope: the attached draft includes admin UI/CLI. Treat that as post-contract/tooling work unless a separate planning pass decides which parts belong in Aiel versus Aviendha.