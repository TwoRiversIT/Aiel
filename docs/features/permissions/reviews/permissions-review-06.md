# Permission System Design Review Six: MS Copilot Readiness Assessment

**Short verdict:** The Aiel design is *well scoped and actionable* for a Copilot implementation, but it needs tighter decisions and delivery artifacts (stable identity, migration tooling, runtime contracts, and client sync) before a reliable generator/assistant can be built.  

### Quick comparison (decision criteria)
| **Criterion** | **Current status** | **Risk** | **Recommended next step** |
|---|---:|---|---|
| **Action identity stability** | Convention + optional override | **High** (refactors change persisted keys) | Lock a canonical ID strategy and emit migration metadata in generator |
| **Fail‑closed enforcement** | Explicit (analyzer + checker model) | Medium (depends on analyzer coverage) | Implement analyzer CI gates and runtime guard rails |
| **Resource-aware checks** | Typed checkers + resource auth service | Low | Provide sample checkers and test harnesses for common patterns |
| **Client capability sync** | Snapshot API + generated helpers | Medium (staleness, scale) | Define snapshot TTL, invalidation hooks, and push/notify patterns |
| **Migration & persistence** | Migration DSL sketched | High | Prototype EF migrations and manifest-driven rename/dep migration flow |

---

### Strengths (what makes it suitable)
- **Action-first model** aligns permissions with intent and payloads, which simplifies resource-aware checks and client capability generation. **This reduces attribute ceremony and centralizes authorization at the application boundary.**   
- **Fail‑closed rule + analyzers** give a strong safety net: missing authorization stories are a build error rather than a runtime hole. That is essential for an automated Copilot that generates code and must not produce insecure defaults. 
- **Generator + manifest approach** provides the deterministic inputs Copilot needs to emit constants, client helpers, and migration hints.

### Gaps that block a production Copilot implementation
1. **Stable canonical permission identity.** The draft leaves namespace vs. configured root undecided; Copilot must know the canonical key to generate code and migrations reliably. (Action: finalize the ID algorithm and an immutable manifest field.)  
2. **Migration story and tooling.** Conventions will change; Copilot must produce EF migrations and manifest updates that preserve grants. The DSL is only sketched — it must be executable and testable.  
3. **Runtime enforcement contracts.** The design relies on analyzers and DI resolution; Copilot needs *concrete* runtime interfaces, error codes, and a small reference implementation to generate callers and tests.  
4. **Client snapshot semantics.** The snapshot API needs explicit versioning, cache invalidation, and scale guidance (pagination, partial snapshots) before Copilot can scaffold UI components safely. Best practices for least‑privilege and incremental consent should be followed.   [Microsoft Learn](https://learn.microsoft.com/en-us/security/zero-trust/develop/developer-strategy-authorization-best-practices)

### Actionable roadmap to make it Copilot‑ready
- **Decide and freeze**: permission name canonicalization, attribute vs. generator override, and the explicit name for permission‑free marker.  
- **Prototype**: a minimal EF-backed store + migration DSL and a manifest-driven rename test.  
- **Deliverables for Copilot**: (1) canonical manifest schema, (2) generator output samples, (3) analyzer rule set and CI config, (4) runtime NuGet with interfaces and a reference implementation, (5) client snapshot API contract and sample Blazor components. These map directly to generator templates Copilot can use.   [Medium](https://medium.com/@yashamansrivastava1710/building-scalable-and-maintainable-permission-systems-beyond-role-based-access-control-4f6560a3e696)  [community.trustcloud.ai](https://community.trustcloud.ai/docs/grc-launchpad/grc-101/governance/designing-an-effective-access-control-policy-best-practices-and-key-considerations/)

**Conclusion:** The design is *sufficiently mature* to draft a Copilot implementation plan **if** the team resolves the identity/migration decisions and produces the small set of concrete artifacts above. Two sentences from the draft:  
> *"The action-centered model is worth pursuing. It reduces attribute ceremony, keeps authorization at the application service boundary, gives resource authorization access to action payloads, and creates a coherent path for client-side action visibility."* 

> *"The main design risk is convention drift: type names and namespaces are easier to refactor than persisted permission keys."* 

---

## GitHub Copilot Response

I agree with the readiness assessment. The current design is conceptually strong enough to begin
implementation planning, especially after moving validation and authorization to the application
service boundary. The remaining risks are not philosophical problems with the model. They are missing
contract decisions and executable artifacts.

My recommendation is to resolve the concerns by turning them into a small set of hard design decisions
and acceptance tests before building the generator. The generator should be the last consumer of these
decisions, not the mechanism that discovers them.

### Opinion On The Main Risks

The highest risk is still permission identity drift. The action-first model is right, but an action
type is not by itself a stable persisted identity. Namespaces, folders, and type names are ordinary
refactoring targets. Permission names are not. Aiel should therefore treat the generated permission
name as a published contract once it appears in a manifest.

The second highest risk is accidentally recreating an invisible pipeline. The latest application
service pattern is the right correction: generated endpoints and clients call application service
contracts, and the service method explicitly validates, authorizes, and performs the use case. A small
reusable action gate is acceptable only if it is explicitly invoked by the application service and has
no DI-order behavior chain.

The third risk is client freshness. Client capability data is a usability optimization, not a security
boundary. The server-side application service must remain the final authority even when a capability
snapshot says an action is available.

### How I Would Resolve The Concerns

#### 1. Stable canonical permission identity

Decision: use a configured application or module root plus the relative action namespace and action
type name as the default permission name.

```text
{ApplicationOrModule}.{RelativeActionNamespace}.{ActionTypeName}
```

Example:

```text
Aviendha.Scheduling.Appointments.RescheduleAppointment
```

The configured root avoids leaking arbitrary source layout into persisted permission keys while still
keeping names human-readable. The full source type name remains manifest metadata, not the persisted
grant identity.

Once emitted, a permission name is frozen. A rename, move, scope change, or lifecycle change must be
represented by explicit migration metadata. The generator should not silently replace an existing
manifest item because a type moved.

Recommended source-level escape hatch:

```csharp
[ActionPermissionName("Aviendha.Scheduling.Appointments.RescheduleAppointment")]
public sealed record RescheduleAppointment(
    AppointmentId AppointmentId,
    Instant StartsAt) : ICommand;
```

Recommended manifest fields:

```csharp
public sealed record ActionPermissionManifestItem(
    PermissionStableId StableId,
    PermissionName PermissionName,
    ActionTypeName ActionTypeName,
    PermissionLifecycle Lifecycle,
    PermissionScopeTypeName ScopeType,
    IReadOnlyCollection<PermissionName> PreviousNames);
```

`StableId` should be generated when the permission is first published and then preserved through
renames. `PermissionName` remains the current human-readable key. `PreviousNames` gives analyzers,
tests, and migration tooling a deterministic way to connect old grants to the current permission.

#### 2. Migration story and tooling

Decision: implement permission migrations as a manifest-diff workflow, modeled after EF Core's model
snapshot pattern.

The migration package should keep a permission manifest snapshot next to EF migrations. A tool compares
the previous snapshot to the current generated manifest and classifies changes as:

| Change | Required action |
|---|---|
| New permission | `Add` catalog row |
| Name changed with same stable ID | `Rename` grants and catalog identity |
| Scope type changed | `MoveScope` or explicit migration block |
| Lifecycle changed to deprecated | `Deprecate` with optional replacement |
| Permission removed | `Remove` only after a deprecation window |

The first executable prototype should prove one scenario end to end: rename
`ChangeAppointment` to `RescheduleAppointment`, preserve existing grants, and emit a manifest snapshot
showing the previous name.

#### 3. Runtime enforcement contracts

Decision: add a small explicit action gate contract, but keep application services as the boundary.

```csharp
public interface IActionGate<TAction>
    where TAction : IAction
{
    ValueTask<Result<IActionExecutionContext<TAction>>> ValidateAndAuthorizeAsync(
        TAction action,
        IExecutionContext executionContext,
        CancellationToken cancellationToken = default);
}
```

Application service methods call the gate deliberately:

```csharp
var gate = await actionGate.ValidateAndAuthorizeAsync(
    action,
    executionContext,
    cancellationToken);

if (!gate.IsSuccess)
{
    return gate.ToResult();
}
```

The gate performs only the shared steps:

1. Create the typed action execution context.
2. Run `IActionValidator<TAction>`.
3. Resolve and run `IActionPermissionChecker<TAction>` or the generated default checker.
4. Return a `Result` failure if the action has no authorization story.

It must not load aggregates, save data, dispatch domain events, or call arbitrary ordered behaviors.
That keeps the application service readable and testable while avoiding the old hidden pipeline problem.

Concrete error contracts should be added before generator work begins:

| Error | Meaning |
|---|---|
| `PermissionErrors.MissingAuthorizationStory` | No checker, generated definition, or explicit permission-free marker exists |
| `PermissionErrors.PermissionDenied` | Grant evaluation or resource authorization denied the action |
| `PermissionErrors.PermissionDefinitionNotFound` | A generated/default checker could not resolve the action permission definition |
| `PermissionErrors.InvalidPermissionScope` | The action was evaluated in an unsupported scope |
| `PermissionErrors.ClientCapabilityStale` | A client-presented capability version is no longer current |

These remain `Result` failures, not exceptions for expected control flow.

#### 4. Resource-aware checks

Decision: make the first sample checker resource-aware and test it with both grant denial and resource
denial cases.

The `RescheduleAppointment` sample should verify:

1. Structural validation runs before permission checks.
2. A missing or default appointment ID never reaches the permission checker.
3. A denied grant returns `PermissionDenied` and never loads the aggregate.
4. A grant success still requires appointment-level authorization.
5. A successful check allows the application service to run the domain use case.

This gives Copilot a concrete pattern to reproduce safely.

#### 5. Client capability sync

Decision: define capability snapshots as cacheable read models with explicit freshness semantics.

Recommended contract shape:

```csharp
public sealed record ActionCapabilityRequest(
    PermissionScope Scope,
    ActionCapabilityRequestMode Mode,
    IReadOnlyCollection<PermissionName> RequestedPermissions,
    ContinuationToken ContinuationToken);
```

`Mode` should distinguish all available actions from selected actions so an empty collection is not an
ambiguous public API. Resource-specific capability checks should use a separate request type rather
than overloading the global snapshot request.

Recommended default behavior:

- Refresh snapshots after login, tenant or scope switch, role membership changes, grant changes, and
  authorization failures.
- Keep global navigation snapshots short-lived and resource-specific snapshots shorter-lived.
- Support selected-permission requests so large applications do not need to fetch the entire catalog
  for every screen.
- Treat server rejection as authoritative and trigger a refresh when authorization fails.
- Consider push invalidation later, but do not require it for the first milestone.

#### 6. Copilot-ready delivery artifacts

I would make these the readiness checklist before asking Copilot or a generator to scaffold real
application permissions:

| Artifact | Minimum acceptance test |
|---|---|
| Canonical manifest schema | `RescheduleAppointment` emits stable name, stable ID, scope, lifecycle, and source type metadata |
| Analyzer rule set | Missing authorization story fails the build in a sample project |
| Runtime action gate | Validation, authorization, and missing-story failures return typed `Result` errors |
| EF migration DSL prototype | Rename preserves grants and updates the manifest snapshot |
| Generated endpoint/client sample | HTTP adapters call `IAppointmentApplicationService` and preserve `Result` contracts |
| Client capability sample | Blazor hides unavailable `RescheduleAppointment` action and refreshes after denial |

### Recommended Next Change To The Main Design Draft

The main draft should graduate several open questions into decisions:

1. Put `IAction`, `ICommand`, `IQuery<TResult>`, application service contracts, and execution context
   contracts in `Aiel.Application.Contracts`.
2. Use `[ActionPermissionName]` as the explicit override because analyzers and generators can read it
   without executing user code.
3. Use `AllowWithoutPermission` as the permission-free marker because it is direct and uncomfortable
   enough to stand out in review.
4. Add `IActionGate<TAction>` as an explicit helper called by application services, not as a pipeline.
5. Add a committed permission manifest snapshot and migration DSL prototype before implementing the
   production generator.

With those decisions made, I would consider the design ready for a first implementation slice. The
first slice should be narrow: `RescheduleAppointment`, one validator, one resource-aware checker, one
application service, one generated endpoint/client pair, one analyzer failure, and one rename
migration test.

