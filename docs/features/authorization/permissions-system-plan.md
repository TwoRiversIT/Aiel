# Permission and Role System - Action-Centered RBAC Implementation Plan

## Status

Ready for implementation planning.

---

## Background

Aiel has always assumed that permissions would become a framework feature. The original draft used
ABP's permission model as the reference point: permission definitions, value providers, stores, and
ASP.NET Core policy integration. The review process exposed a better Aiel-native center of gravity:
permissions should attach to actions, not to loose strings or endpoint attributes.

In Aiel, commands and queries already represent user, system, message, or saga intent. They are not
just transport DTOs. They carry the parameters needed to perform work, including resource identifiers.
That makes them the natural object for validation, authorization, telemetry, auditing, and execution.

This draft explores an action-centered authorization model inspired by FluentValidation, CQRS,
application service boundaries, source generators, analyzers, and ASP.NET Core authorization, while
keeping Aiel's design principles intact:

- Explicit over implicit.
- No hidden runtime discovery.
- No exceptions for expected control flow.
- No magic strings.
- No technical debt in public models.
- Server-side enforcement is authoritative.
- Client-side affordances should prevent users from initiating actions they are not permitted to run.

---

## Design Thesis

Permissions are durable catalog entries for actions. Roles are durable bundles of authority that
collect those permissions and assign them to actors within scope. An action type is the source of
intent. A permission name is the stable key used for storage, policy interop, manifests, and client
capability snapshots.

The authorization system should therefore answer this question:

> Can this actor execute this action, in this scope, possibly against this resource?

In practice, the answer should usually come from RBAC evaluation:

> Which effective subjects apply to this actor in this scope, which roles do those subjects hold,
> and do those direct or role-derived grants authorize the action?

The command or query object is the runtime action instance. It already carries context such as
`AppointmentId`, `ClientId`, `TenantId`, `ClinicId`, dates, state transitions, or requested query
shape. The application service method should validate and authorize the action before it performs the
use case.

---

## Goals

1. Make actions the first-class unit of validation, authorization, telemetry, and auditing.
2. Remove routine `[RequirePermission]` decoration from commands and queries.
3. Derive default permission keys from a configured application/module root and relative action identity.
4. Keep generated permission constants to avoid magic strings.
5. Fail closed when an action has no authorization story.
6. Make roles first-class framework concepts rather than a naming convention over generic subjects.
7. Support typed, action-specific permission checkers for resource-aware authorization.
8. Evaluate permissions through effective subjects so actor authority can come from direct grants,
    role assignments, or both.
9. Keep ASP.NET Core authorization for authentication and transport-level policy, not as the
   authoritative permission boundary.
10. Provide client-side capability data so applications can hide actions a user cannot perform.
11. Keep server-side permission checks authoritative even when the client hides unavailable actions.
12. Preserve Clean Architecture boundaries across the permission and role package family.

---

## Non-Goals

1. Do not rely on client-side checks for security.
2. Do not require controllers or endpoints to repeat action-level permissions.
3. Do not replace ASP.NET Core authentication or authorization infrastructure.
4. Do not auto-discover permissions through runtime assembly scanning.
5. Do not store application user, tenant, clinic, or identity tables inside the authorization
    persistence package.
6. Do not make permission checks depend on `HttpContext`.
7. Do not preserve a permission-only evaluator surface when a cleaner RBAC-oriented contract is
    available. Greenfield posture allows breaking changes.

---

## Target Layer and Package Ownership

The permission feature should follow the same Clean Architecture boundaries expected of application
features. Some assemblies may start thin; that is acceptable if the dependency direction is correct.

| Package | Responsibility |
| --- | --- |
| `Aiel.Application.Contracts` | Core action abstractions: `IAction`, `ICommand`, `IQuery<TResult>`, execution context contracts, application service contracts |
| `Aiel.Permissions.Domain.Shared` | Permission and role value objects, strong IDs, lifecycle enums, scope type names, subject type names |
| `Aiel.Permissions.Domain` | Permission catalog, role catalog, grant aggregate, role assignment aggregate, and grant precedence rules |
| `Aiel.Permissions.Application.Contracts` | `IPermissionChecker`, `IPermissionManager`, `IRoleManager`, effective-subject resolution contracts, action authorization contracts, and the shared capability request/snapshot DTOs (`ActionCapabilityRequest`, `ActionCapabilitySnapshot`, `IActionCapabilityService`) |
| `Aiel.Permissions.Application` | Default permission checker, managers, action authorization gate services, scope orchestration, and effective-subject orchestration |
| `Aiel.Permissions.EntityFrameworkCore` | EF Core implementation of application persistence contracts and DbContext mappings for permission and role data |
| `Aiel.Permissions.EntityFrameworkCore.PostgreSql` | PostgreSQL-specific configuration, migrations, indexes, and provider extensions |
| `Aiel.Permissions.AspNetCore` | ASP.NET Core middleware, authorization integration, generated endpoint support, HTTP action adapters |
| `Aiel.Permissions.Client` | Client-side capability cache and refresh abstractions shared by UI frameworks |
| `Aiel.Permissions.Client.Blazor` | Blazor visibility helpers such as `CanExecute` and snapshot-aware action rendering helpers |
| `Aiel.Permissions.Testing` | Test doubles, fake stores, configurable context factories, always-grant and always-deny helpers |
| `Aiel.Permissions.Generators` | Source generators for action permission constants, manifests, default role manifests, definitions, and client helpers |
| `Aiel.Permissions.Analyzers` | Diagnostics for missing authorization, invalid lifecycle changes, unsafe strings, missing registration, and RBAC drift |

Domain grant and role-assignment entities belong in `Aiel.Permissions.Domain`. EF Core owns
mappings and persistence records only. If EF Core maps a domain aggregate directly, the aggregate
type still lives in the Domain package. If EF Core needs a separate persistence shape, that shape
should be named as a persistence record or EF entity, not a domain entity.

The target layer for action and application service contracts is `Aiel.Application.Contracts`
because those types define the public application boundary used by generated endpoints, generated
HTTP clients, Blazor Server applications, background workers, and tests. Permission and role domain
types stay inside the permission package family; application contracts should not depend on storage
or infrastructure packages.

---

## RBAC Upgrade Decision

The current implementation already models grants against a generic `PermissionSubjectTypeName` plus
`PermissionSubjectKey`, which is a good base. It does not yet model role definitions, role
assignments, or effective-subject expansion during evaluation. That means the current system is a
generic subject-grant system, not yet a full RBAC system.

Greenfield posture means Aiel should not preserve that narrower shape. The recommended direction is
to make a breaking change now and standardize on these rules:

1. Roles are first-class authorization concepts, not just a conventional subject type.
2. Direct grants remain supported, but roles become the primary assignment mechanism for human actors.
3. Grant evaluation resolves effective subjects for the actor and scope before looking up grants.
4. Role definitions and role assignments have stable identities, lifecycle, and migration stories just
    like permissions.
5. The default authorization story for application actors should be role-based, with direct grants
    used sparingly for exceptions, bootstrapping, or break-glass operations.

---

## Core Action Model

Commands and queries should share a common root interface.

```csharp
public interface IAction;

public interface ICommand : IAction;

public interface IQuery<TResult> : IAction;
```

This makes cross-cutting application concerns action-centric instead of command/query-specific.
Validation, authorization, telemetry, auditing, idempotency, and client capability generation can all
speak in terms of actions.

Example action:

```csharp
public sealed record ChangeAppointment(
    AppointmentId AppointmentId,
    DateTimeOffset? NewDateTime,
    LocationId? NewLocation) : ICommand;
```

Queries are actions too:

```csharp
public sealed record FindClient(ClientId ClientId) : IQuery<ClientDetails>;

public sealed record ListAppointments(
    ClinicId ClinicId,
    DateOnly From,
    DateOnly To) : IQuery<IReadOnlyCollection<AppointmentSummary>>;
```

Action type names should be intentful. `ChangeAppointment`, `ApproveRiskAssessment`, and
`ListAppointments` are useful action names. `UpdateClient` is too vague because it hides intent.

---

## Execution Context

The action should not replace trace identity. `OperationId` and `Action` are different concepts:

- `OperationId` identifies a specific execution for traceability.
- `Action` is the payload being executed.

The base execution context keeps trace and actor information. A typed action execution context adds
the current action.

```csharp
public interface IExecutionContext
{
    OperationId OperationId { get; }

    IActor Actor { get; }

    CorrelationId CorrelationId { get; }

    OperationId? CausationId { get; }

    ClientInstanceId? ClientInstanceId { get; }

    IDictionary<String, Object?> Properties { get; }
}

public interface IActionExecutionContext<out TAction> : IExecutionContext
    where TAction : IAction
{
    TAction Action { get; }
}
```

The application service creates a child action context before it validates and authorizes the action:

```csharp
var childContext = ActionExecutionContext<RescheduleAppointment>.CreateChild(
    parentContext,
    action);
```

That context becomes the shared envelope for validators, permission checkers, telemetry, auditing,
and the application use case.

Open design question: Aiel currently exposes `IExecutionContext` as an interface. Prior design notes
raised whether this should become a framework-owned sealed class to protect invariants. The action
context design should be evaluated together with that broader execution-context decision.

---

## Canonical Action Permission Identity

The default permission key should be derived from a configured application or module root plus the
relative action namespace and action type name.

Recommended default:

```text
{ApplicationOrModule}.{RelativeActionNamespace}.{ActionTypeName}
```

For example:

```csharp
namespace Aviendha.Scheduling.Appointments;

public sealed record ChangeAppointment(...) : ICommand;
```

Default permission key when the configured root is `Aviendha`:

```text
Aviendha.Scheduling.Appointments.ChangeAppointment
```

The configured root avoids leaking arbitrary source layout into persisted permission keys while still
keeping names human-readable. The full source type name remains manifest metadata, not the persisted
grant identity.

Once emitted in a committed manifest, a permission name is frozen. Namespace or type refactors can
become authorization data migrations, so the generator and analyzer must make action identity changes
visible. The generator must not silently replace an existing manifest item because a type moved.

The source generator emits constants from action types:

```csharp
public static partial class AviendhaPermissions
{
    public static partial class Scheduling
    {
        public const string ChangeAppointment =
            "Aviendha.Scheduling.Appointments.ChangeAppointment";
    }
}
```

Application code still avoids magic strings:

```csharp
var permissionName = AviendhaPermissions.Scheduling.ChangeAppointment;
```

If the default convention is not stable enough, the action can declare a stable permission name with
`ActionPermissionNameAttribute`. This should be rare and explicit. The attribute is the preferred
override because analyzers and generators can read it without executing user code.

```csharp
[ActionPermissionName("Aviendha.Scheduling.Appointments.ChangeAppointment")]
public sealed record ChangeAppointment(...) : ICommand;
```

---

## Generated Permission Definitions

The generator should emit deterministic, inspectable permission definitions from action types and
their metadata.

Generated provider example:

```csharp
public sealed class AviendhaActionPermissionDefinitionProvider : IPermissionDefinitionProvider
{
    public void Define(IPermissionDefinitionBuilder builder)
    {
        builder.Group("Aviendha.Scheduling.Appointments", "Scheduling appointments")
            .Permission(AviendhaPermissions.Scheduling.ChangeAppointment)
                .Action<ChangeAppointment>()
                .Scope(AviendhaPermissionScopes.Clinic)
                .Lifecycle(PermissionLifecycle.Active);
    }
}
```

The generator also emits a manifest used by analyzers, tests, EF migrations, and client capability
metadata:

```csharp
public sealed class AviendhaPermissionManifest : IPermissionManifest
{
    public PermissionManifestId Id => new("Aviendha");

    public ManifestHash VersionHash => new("sha256:...");

    public IReadOnlyCollection<ActionPermissionManifestItem> Permissions { get; } =
    [
        new ActionPermissionManifestItem(
            StableId: new PermissionStableId("perm_01jz9p58d6d8m8n7x3t9q2a4bc"),
            PermissionName: new PermissionName(
                "Aviendha.Scheduling.Appointments.ChangeAppointment"),
            ActionTypeName: new ActionTypeName(
                "Aviendha.Scheduling.Appointments.ChangeAppointment"),
            Lifecycle: PermissionLifecycle.Active,
            ScopeType: AviendhaPermissionScopes.Clinic.Name,
            PreviousNames: [])
    ];
}
```

`StableId` is generated when the permission is first published and then preserved through renames.
`PermissionName` remains the current human-readable key. `PreviousNames` gives analyzers, tests, and
migration tooling a deterministic way to connect old grants to the current permission.

The repository should commit a generated permission manifest snapshot beside migrations or generated
artifacts. Future generator runs compare the previous snapshot to the current manifest and require an
explicit permission migration when a stable ID changes name, scope, or lifecycle.

The generator may emit a fluent registration extension, but the consuming dependency module must call
it explicitly. Runtime service registration must not be hidden behind a module initializer.

```csharp
public sealed class AviendhaApplicationContracts : AielDependency
{
    public override Task PreConfigureAsync(
        DependencyConfigurationContext context,
        CancellationToken cancellationToken = default)
    {
        context.Services.ConfigurePermissionDefinitions(permissions =>
            permissions.AddAviendhaActionPermissions(options =>
            {
                options.UsePostgreSql();
                options.ConfigureScopeResolver<AviendhaPermissionScopeResolver>();
            }));

        return Task.CompletedTask;
    }
}
```

The analyzer should warn locally and fail in CI if generated permission registration is not reachable
from a `AielDependency`.

---

## Authorization Stories and Fail-Closed Rules

Every action must have an authorization story. Valid states are:

1. A concrete `IActionPermissionChecker<TAction>` exists.
2. A generated convention permission exists and the default action permission checker can evaluate it.
3. The action is explicitly marked as permission-free.
4. The analyzer fails the build.

There must be no "no checker found, allow" path.

Explicit permission-free actions should be rare and visible:

```csharp
[DoesNotRespectAuthority(Reason = "Public health endpoint")]
public sealed record GetHealthStatus : IQuery<HealthStatus>;
```

`DoesNotRespectAuthority` is the selected marker name because it is direct, searchable, and
uncomfortable enough to stand out in review.

---

## Permission Checker Model

The framework should expose a typed action permission checker similar in spirit to
`FluentValidation` validators.

```csharp
public interface IActionPermissionChecker<TAction>
    where TAction : IAction
{
    ValueTask<Result> CheckAsync(
        TAction action,
        IActionExecutionContext<TAction> context,
        CancellationToken cancellationToken = default);
}
```

The application service action gate can resolve the typed checker for the current action. If no
concrete checker is registered, it uses the generated default checker for the action permission
definition.

```csharp
public sealed class DefaultActionPermissionChecker<TAction>(
    IPermissionGrantEvaluator grants,
    IPermissionDefinitionRegistry definitions)
    : IActionPermissionChecker<TAction>
    where TAction : IAction
{
    public ValueTask<Result> CheckAsync(
        TAction action,
        IActionExecutionContext<TAction> context,
        CancellationToken cancellationToken = default)
    {
        var permission = definitions.GetByAction<TAction>();
        return grants.EvaluateAsync(permission.Name, context, cancellationToken);
    }
}
```

Resource-aware actions use concrete checkers:

```csharp
public sealed class ChangeAppointmentPermissionChecker(
    IPermissionGrantEvaluator grants,
    IResourceAuthorizationService resources)
    : AbstractPermissionChecker<ChangeAppointment>
{
    public override async ValueTask<Result> CheckAsync(
        ChangeAppointment action,
        IActionExecutionContext<ChangeAppointment> context,
        CancellationToken cancellationToken = default)
    {
        var permission = await grants.EvaluateAsync(
            AviendhaPermissions.Scheduling.ChangeAppointment,
            context,
            cancellationToken);

        if (!permission.IsSuccess)
        {
            return permission;
        }

        return await resources.AuthorizeAsync(
            new ResourceAuthorizationRequest<AppointmentId>(
                Resource: action.AppointmentId,
                Action: new PermissionAction(
                    new PermissionName(AviendhaPermissions.Scheduling.ChangeAppointment)),
                ExecutionContext: context),
            cancellationToken);
    }
}
```

The permission checker should return `Result` failures for expected authorization denial. It should
not throw for expected control flow.

---

## Validation and Authorization Application Service Boundary

The application service boundary is authoritative for both validation and authorization. Actions may
originate from HTTP, background services, message queues, sagas, tests, or internal calls. The same
application service method must protect all origins.

Recommended application service order:

1. Create action execution context.
2. Run structural action validation.
3. Run action permission check.
4. Run the application use case.
5. Dispatch events and downstream effects.

Structural validation runs before permission checks because permission checkers often need valid IDs
and non-empty action inputs. Business validation that depends on protected data may still belong in
the application use case or domain model after authorization.

Application service methods should not be thin facades over `ICommandHandler<TCommand>` when the
application service is the public architectural boundary. The action remains the permission, audit,
and capability unit, but the service method is the command handler in the architectural sense.

Aiel should provide a small reusable action gate helper for the shared validation and authorization
steps. The gate is explicitly called by application services; it is not a hidden dispatcher pipeline
and must not support arbitrary DI-ordered behavior chains.

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

The gate performs only these shared steps:

1. Create the typed action execution context.
2. Run `IActionValidator<TAction>`.
3. Resolve and run `IActionPermissionChecker<TAction>` or the generated default checker.
4. Return a `Result` failure if the action has no authorization story.

It must not load aggregates, save data, dispatch domain events, or call arbitrary ordered behaviors.
Those remain application service or domain responsibilities.

```csharp
public sealed record RescheduleAppointment(
    AppointmentId AppointmentId,
    Instant StartsAt) : ICommand;

public interface IAppointmentApplicationService
{
    Task<Result> RescheduleAsync(
        RescheduleAppointment action,
        CancellationToken cancellationToken = default);
}
```

The implementation keeps the gate and the use case together, which reduces navigation between action
types and detached handlers while remaining testable through injected validators, permission checkers,
repositories, and unit-of-work abstractions.

```csharp
public sealed class AppointmentApplicationService(
    IExecutionContextAccessor executionContextAccessor,
    IActionGate<RescheduleAppointment> actionGate,
    IAppointmentRepository appointments,
    IUnitOfWork unitOfWork)
    : IAppointmentApplicationService
{
    public async Task<Result> RescheduleAsync(
        RescheduleAppointment action,
        CancellationToken cancellationToken = default)
    {
        var executionContext = executionContextAccessor.Current;
        var gate = await actionGate.ValidateAndAuthorizeAsync(
            action,
            executionContext,
            cancellationToken);

        if (!gate.IsSuccess)
        {
            return gate.ToResult();
        }

        var actionContext = gate.Value;

        var appointment = await appointments.GetAsync(
            action.AppointmentId,
            cancellationToken);

        if (!appointment.IsSuccess)
        {
            return appointment.ToResult();
        }

        var rescheduled = appointment.Value.Reschedule(
            action.StartsAt,
            actionContext.Actor);

        if (!rescheduled.IsSuccess)
        {
            return rescheduled;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

`ICommandHandler<TCommand>` and dispatchers may remain as lower-level or compatibility abstractions,
but generated application services should not be implemented as facades over per-action handlers by
default. If a use case is complex enough to need collaborators, those collaborators should be named
for the domain work they perform rather than being mandatory one-to-one command handlers.

Concrete error contracts should be available before generator work begins:

| Error | Meaning |
| --- | --- |
| `PermissionErrors.MissingAuthorizationStory` | No checker, generated definition, or explicit permission-free marker exists |
| `PermissionErrors.PermissionDenied` | Grant evaluation or resource authorization denied the action |
| `PermissionErrors.PermissionDefinitionNotFound` | A generated/default checker could not resolve the action permission definition |
| `PermissionErrors.InvalidPermissionScope` | The action was evaluated in an unsupported scope |
| `PermissionErrors.ClientCapabilityStale` | A client-presented capability version is no longer current |

These errors are `Result` failures, not exceptions for expected control flow.

Validation follows a `FluentValidation`-like model:

```csharp
public interface IActionValidator<TAction>
    where TAction : IAction
{
    ValueTask<Result> ValidateAsync(
        TAction action,
        IActionExecutionContext<TAction> context,
        CancellationToken cancellationToken = default);
}
```

Example:

```csharp
public sealed class ChangeAppointmentValidator : AbstractActionValidator<ChangeAppointment>
{
    public ChangeAppointmentValidator()
    {
        RuleFor(action => action.AppointmentId).NotDefault();
        RuleFor(action => action)
            .Must(HaveAtLeastOneRequestedChange)
            .WithError(AppointmentErrors.NoChangeRequested);
    }
}
```

Open design question: decide whether Aiel should wrap `FluentValidation` directly, define a small
native validation abstraction, or support both. Client reuse may push toward a native abstraction with
optional FluentValidation adapters.

---

## ASP.NET Core Integration

The intention is to leverage ASP.NET Core authorization, not replace it.

Controllers and Minimal API endpoints should use ASP.NET Core `[Authorize]` for authentication and
transport-level policy. They should not repeat action permissions.

```csharp
[Authorize]
public sealed class AppointmentsController(
    IAppointmentApplicationService applicationService)
    : AielControllerBase
{
    [HttpPost("appointments/{appointmentId}/reschedule")]
    public async Task<IActionResult> Reschedule(
        AppointmentId appointmentId,
        RescheduleAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var action = new RescheduleAppointment(
            appointmentId,
            request.StartsAt);

        var result = await applicationService.RescheduleAsync(
            action,
            cancellationToken);

        return ToActionResult(result);
    }
}
```

No `[RequirePermission]` appears on the controller action. The application service performs
validation and permission checks before the use case runs.

Generated HTTP clients can implement the same application service contract for client-side consumers.
They remain transport adapters and should use `ResultHttpClientExtensions` or equivalent
`Result`-aware helpers rather than replacing the contract with `ProblemDetails`.

```csharp
public sealed class AppointmentApplicationHttpClient(HttpClient http)
    : IAppointmentApplicationService
{
    public Task<Result> RescheduleAsync(
        RescheduleAppointment action,
        CancellationToken cancellationToken = default)
    {
        var request = new RescheduleAppointmentRequest(action.StartsAt);

        return http.PostResultAsync(
            AppointmentRoutes.Reschedule(action.AppointmentId),
            request,
            cancellationToken);
    }
}
```

The ASP.NET Core package likely needs:

1. Middleware that creates or resolves the current `IExecutionContext` for the request.
2. A scoped accessor for the current execution context.
3. A controller base or generated endpoint layer that creates actions and calls application services.

Possible middleware order:

```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAielExecutionContext();
app.UseAuthorization();
app.MapControllers();
```

The exact order must be validated against ASP.NET Core behavior. The important invariant is that the
same actor and correlation data flows through ASP.NET Core authorization, application services,
telemetry, and auditing.

Open design question: generated controllers or Minimal API endpoint classes may be a natural later
extension. They should call application service contracts rather than a hidden command dispatcher.

---

## Scope and Subject Model

Subject answers: who or what receives a grant?

Scope answers: where is the grant valid?

Scope is not included because ABP has it. Scope is included because a grant without a boundary of
authority is ambiguous.

Examples:

- A platform support actor may inspect operational metadata across tenants.
- A tenant counsellor may have authority in one tenant but no authority in another.
- A clinic administrator may manage scheduling actions in one clinic but not another.
- A client application may have a machine grant for integration actions in one scope.

`Platform`, `Host`, and `Tenant` are built-in scope types, not a closed enum. Applications can add
scope types such as `Clinic`.

```csharp
public sealed record PermissionScopeType(PermissionScopeTypeName Name);

public sealed record PermissionScope(
    PermissionScopeType Type,
    PermissionScopeKey Key);

public sealed record PermissionSubject(
    PermissionSubjectType Type,
    PermissionSubjectKey Key);
```

No public model should expose raw string keys. Scope and subject keys are value objects from the
first implementation.

The scope chain should be resolved by an application-aware service:

```csharp
public interface IPermissionScopeResolver
{
    ValueTask<Result<IReadOnlyCollection<PermissionScope>>> ResolveAsync(
        IExecutionContext context,
        CancellationToken cancellationToken = default);
}
```

For Aviendha, a clinic-aware resolver might return:

1. Platform scope.
2. Host scope.
3. Tenant scope.
4. Clinic scope.

Whether `Clinic` should be a permission scope or a resource authorization fact depends on whether
clinic-level grants are assignable. If clinic administrators can grant permissions at the clinic
level, clinic is a scope. If clinic only constrains access to a specific record, clinic may belong in
resource authorization.

---

## Role Model

Roles answer a distinct question from permissions:

- Permissions answer what action is being authorized.
- Roles answer which reusable authority bundle is being assigned.

The framework should introduce first-class role concepts instead of treating roles as an
application-only convention.

```csharp
public sealed record RoleName
{
    public RoleName(string value)
    {
        Value = RoleNameValidator.Validate(value);
    }

    public string Value { get; }
}

[StrongId<Guid>(DisallowDefault = true)]
public readonly partial record struct RoleDefinitionId : IStrongId<Guid>;

[StrongId<Guid>(DisallowDefault = true)]
public readonly partial record struct RoleAssignmentId : IStrongId<Guid>;
```

A role definition should include:

1. Stable identity.
2. Published role name.
3. Display metadata for admin and client UX.
4. Allowed assignment scope types.
5. Allowed assignee subject types.
6. Lifecycle state and optional replacement metadata.

```csharp
public sealed class RoleDefinition
{
    public RoleDefinitionId Id { get; init; }
    public RoleName Name { get; init; }
    public string DisplayName { get; init; }
    public string? Description { get; init; }
    public IReadOnlyCollection<PermissionScopeTypeName> AllowedScopeTypes { get; init; }
    public IReadOnlyCollection<PermissionSubjectTypeName> AllowedAssigneeTypes { get; init; }
    public RoleLifecycle Lifecycle { get; init; }
}
```

A role assignment should bind an assignee subject to a role within scope:

```csharp
public sealed class RoleAssignment
{
    public RoleAssignmentId Id { get; init; }
    public RoleDefinitionId RoleId { get; init; }
    public PermissionScope Scope { get; init; }
    public PermissionSubject Assignee { get; init; }
    public RoleAssignmentOrigin Origin { get; init; }
}
```

The first RBAC milestone should not include nested roles. Role-to-role inheritance can be a later
extension if real application scenarios require it. V1 should support:

1. Permission grants to role subjects.
2. Role assignments from actor subjects to role definitions.
3. Effective evaluation of direct subject grants plus role-derived grants.

That is sufficient to make the system real RBAC without introducing hierarchy complexity on day one.

---

## Effective Subject Resolution

The current evaluator shape takes a single `subjectType` and `subjectKey`, which is too narrow for
RBAC because an actor's authority may come from multiple effective subjects:

1. The actor directly.
2. One or more assigned roles.
3. Later, possibly system or derived subjects.

This should become a breaking contract change. Instead of making callers expand subjects manually,
the framework should resolve effective subjects before evaluating grants.

```csharp
public interface IEffectivePermissionSubjectResolver
{
    ValueTask<Result<IReadOnlyList<EffectivePermissionSubject>>> ResolveAsync(
        IExecutionContext context,
        PermissionScope scope,
        CancellationToken cancellationToken = default);
}

public sealed record EffectivePermissionSubject(
    PermissionSubject Subject,
    EffectivePermissionSubjectKind Kind,
    Int32 Precedence);
```

`IPermissionGrantEvaluator` should then evaluate across effective subjects, not a single explicit
subject:

```csharp
public interface IPermissionGrantEvaluator
{
    Task<Result<PermissionGrantEvaluation>> EvaluateAsync(
        PermissionName permissionName,
        PermissionScope scope,
        IExecutionContext context,
        CancellationToken cancellationToken = default);
}

public sealed record PermissionGrantEvaluation(
    PermissionGrantDecision? Decision,
    PermissionSubject? WinningSubject,
    RoleDefinitionId? WinningRoleId,
    PermissionGrantSource Source);
```

Recommended precedence for v1:

1. Prohibit beats grant.
2. Direct subject decisions beat role-derived decisions.
3. More specific scope beats less specific scope when a scope chain is involved.
4. If no direct or role-derived decision exists, the result is no decision and the action checker
   fails closed.

This evaluation result should be audit-friendly. The framework should preserve which effective
subject produced the decision so telemetry and admin tooling can answer why authorization succeeded
or failed.

---

## Role Definitions, Assignment, and Mutation APIs

`IPermissionManager` should remain focused on direct grant mutation. RBAC needs separate role
contracts so the framework does not force applications to fake role management through generic
subject writes.

```csharp
public interface IRoleManager
{
    ValueTask<Result<RoleDefinitionId>> CreateOrUpdateRoleAsync(
        UpsertRoleDefinitionRequest request,
        CancellationToken cancellationToken = default);

    ValueTask<Result<RoleAssignmentId>> AssignRoleAsync(
        AssignRoleRequest request,
        CancellationToken cancellationToken = default);

    ValueTask<Result> UnassignRoleAsync(
        UnassignRoleRequest request,
        CancellationToken cancellationToken = default);
}
```

The manager validates:

- The role exists and is active.
- The assignee subject type is allowed for the role.
- The assignment scope type is allowed for the role.
- The assignee subject reference is valid according to registered subject validators.
- Duplicate or contradictory assignments are rejected.

Default roles can still be generated from application code and committed manifests, but runtime
assignments are operational data. The plan should support both:

1. Code-defined default roles with stable IDs and migration support.
2. Runtime role assignments in persistence.
3. Optional runtime-created custom roles later, if applications need them.

For the first RBAC slice, Aiel should implement code-defined roles plus runtime role assignments.
That keeps the migration story deterministic while still delivering a real RBAC model.

---

## Strong IDs and Value Objects

Aiel is greenfield and has a zero-technical-debt posture. Permission public models should use
strong IDs and value objects from the first implementation.

```csharp
[StrongId<Guid>(DisallowDefault = true)]
public readonly partial record struct PermissionGrantId : IStrongId<Guid>;

[StrongId<Guid>(DisallowDefault = true)]
public readonly partial record struct TenantId : IStrongId<Guid>;

public sealed record PermissionName
{
    public PermissionName(string value)
    {
        Value = PermissionNameValidator.Validate(value);
    }

    public string Value { get; }
}
```

`PermissionName` is a value object rather than a GUID-backed strong ID because it is the durable,
human-readable policy key used in source code, manifests, storage, and client capability metadata.

Prerequisite decision: promote StrongId to a first-class module before permissions are implemented,
or allow the permission packages to depend on the existing StrongId primitive in `Aiel.Domain` until
that promotion happens.

---

## Permission Persistence and Lifecycle

Permission names are persisted grant keys. Renames, removals, and scope changes are data migrations.

When the EF Core package is used, permission lifecycle changes should be expressed through EF Core
migrations using a permission migration DSL.

Permission migrations should be generated from a manifest-diff workflow modeled after EF Core's model
snapshot pattern. The migration package keeps the previous committed permission manifest snapshot next
to EF migrations. A tool compares that snapshot with the current generated manifest and classifies
changes before producing a migration.

| Manifest change | Required migration action |
| --- | --- |
| New stable ID | Add catalog row |
| Same stable ID, different permission name | Rename grants and catalog identity |
| Same stable ID, different scope type | Move scope or require an explicit migration block |
| Same stable ID, lifecycle changed to deprecated | Deprecate with optional replacement |
| Stable ID removed | Remove only after a deprecation window |

```csharp
public partial class RenameAppointmentPermission : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Permissions()
            .Rename(
                from: "Aviendha.Scheduling.ChangeAppointment",
                to: AviendhaPermissions.Scheduling.ChangeAppointment)
            .Deprecate(
                permission: "Aviendha.Scheduling.ChangeAppointment",
                replacedBy: AviendhaPermissions.Scheduling.ChangeAppointment);
    }
}
```

Potential DSL operations:

| Operation | Purpose |
| --- | --- |
| `Add` | Add a permission catalog row |
| `Deprecate` | Mark a permission as deprecated and optionally point to a replacement |
| `Rename` | Move grants and catalog identity from one permission name to another |
| `Remove` | Mark a permission removed after a deprecation window |
| `Restore` | Reverse a deprecation or removal in migration rollback |
| `MoveScope` | Migrate grants when a permission changes allowed scope |

The generator emits manifests so analyzers can detect unmanaged changes. Applying changes should be
migration-driven, not a hidden startup mutation.

The first executable prototype should prove one scenario end to end: rename `ChangeAppointment` to
`RescheduleAppointment`, preserve existing grants, and emit a manifest snapshot showing the previous
name.

Role lifecycle should follow the same discipline. Default roles are persisted authorization catalog
data, not incidental UI labels. Renames and removals need stable IDs and migrations.

Role manifest diff rules should mirror permission rules:

| Role manifest change | Required migration action |
| --- | --- |
| New stable ID | Add role definition row |
| Same stable ID, different role name | Rename role definition and preserve assignments |
| Same stable ID, different allowed scope types | Require explicit migration or assignment validation backfill |
| Same stable ID, lifecycle changed to deprecated | Deprecate with optional replacement |
| Stable ID removed | Remove only after a deprecation window |

The first RBAC persistence prototype should prove one scenario end to end: rename a default role,
preserve existing assignments, and show a manifest snapshot that links the previous role name to the
current stable ID.

---

## Store, Manager, and Decorators

`IPermissionManager` is the consumer-facing API for grant mutation. `IPermissionStore` is an
infrastructure contract behind the manager and evaluator.

```csharp
public interface IPermissionManager
{
    ValueTask<Result> GrantAsync(
        PermissionGrantRequest request,
        CancellationToken cancellationToken = default);

    ValueTask<Result> ProhibitAsync(
        PermissionGrantRequest request,
        CancellationToken cancellationToken = default);

    ValueTask<Result> RevokeAsync(
        PermissionGrantId grantId,
        CancellationToken cancellationToken = default);
}
```

The manager validates:

- The permission exists.
- The permission is active.
- The requested scope type is valid for the permission.
- The subject type is valid.
- The subject reference is valid according to registered subject validators.
- Grant, prohibit, and revoke semantics are consistent.

The store should start simple and be decorator-friendly. Batching and caching should be decorators,
not premature complexity in the base contract.

```csharp
public interface IPermissionStore
{
    ValueTask<Result<PermissionGrantDecision>> GetGrantAsync(
        PermissionName permissionName,
        PermissionScope scope,
        PermissionSubject subject,
        CancellationToken cancellationToken = default);

    ValueTask<Result> SetGrantAsync(
        PermissionGrant grant,
        CancellationToken cancellationToken = default);

    ValueTask<Result> DeleteGrantAsync(
        PermissionGrantId grantId,
        CancellationToken cancellationToken = default);
}
```

Decorator candidates:

| Decorator | Purpose |
| --- | --- |
| `CachingPermissionStore` | Cache grant lookups by permission, scope, and subject |
| `BatchingPermissionStore` | Coalesce multiple grant lookups within a single evaluation |
| `AuditingPermissionStore` | Record grant writes and lifecycle changes |

This depends on a broader Aiel decorator system and should be tracked separately.

RBAC adds separate persistence seams for role definition and role assignment data:

```csharp
public interface IRoleAssignmentStore
{
    ValueTask<Result<IReadOnlyCollection<RoleAssignment>>> GetAssignmentsAsync(
        PermissionSubject assignee,
        PermissionScope scope,
        CancellationToken cancellationToken = default);

    ValueTask<Result<RoleAssignmentId>> CreateAssignmentAsync(
        RoleAssignment assignment,
        CancellationToken cancellationToken = default);

    ValueTask<Result> DeleteAssignmentAsync(
        RoleAssignmentId assignmentId,
        CancellationToken cancellationToken = default);
}
```

Decorator candidates expand accordingly:

| Decorator | Purpose |
| --- | --- |
| `CachingRoleAssignmentStore` | Cache role membership lookups by assignee and scope |
| `BatchingRoleAssignmentStore` | Coalesce assignment lookups during capability or grant evaluation |
| `AuditingRoleAssignmentStore` | Record assignment writes and lifecycle changes |

---

## EF Core Persistence Boundary

`Aiel.Permissions.EntityFrameworkCore` should store permission catalog rows, grant rows, role
definition rows, and role assignment rows. It should not own application user, tenant, clinic, or
client-application tables.

Reasons:

1. Identity providers differ across applications.
2. Application identity semantics still differ across applications.
3. Tenant and scope topology differ across applications.
4. Owning those tables would force permissions infrastructure to depend outward on application
   identity and tenancy concerns.

The EF package stores validated subject references and framework-owned RBAC records:

```csharp
public sealed class PermissionGrant
{
    public PermissionGrantId Id { get; init; }
    public PermissionName PermissionName { get; init; }
    public PermissionScope Scope { get; init; }
    public PermissionSubject Subject { get; init; }
    public PermissionGrantDecision Decision { get; init; }
}
```

Applications provide subject validators:

```csharp
public interface IPermissionSubjectValidator
{
    ValueTask<Result> ValidateAsync(
        PermissionSubject subject,
        CancellationToken cancellationToken = default);
}
```

Applications still own the authoritative user and tenant records. The authorization package owns
role definitions and role assignments because those are framework-level authorization data.

---

## Default Roles and Application Composition

Permissions are too granular to be the default admin surface. Applications need stable, named roles
that bundle action permissions in language the business recognizes.

Examples for Aviendha could include:

- `TenantOwner`
- `ClinicAdministrator`
- `Counsellor`
- `Scheduler`
- `ClientAreaUser`

Default role definitions should be generated or registered alongside permission definitions:

```csharp
public sealed class AviendhaRoleDefinitionProvider : IRoleDefinitionProvider
{
    public void Define(IRoleDefinitionBuilder builder)
    {
        builder.Role(AviendhaRoles.ClinicAdministrator)
            .DisplayName("Clinic administrator")
            .AssignableTo("User")
            .InScopes("Clinic")
            .Grants(
                AviendhaPermissions.Scheduling.RescheduleAppointment,
                AviendhaPermissions.Scheduling.CreateAppointment);
    }
}
```

This gives the framework a clean split:

1. Actions define permissions.
2. Applications define default roles as named bundles of those permissions.
3. Runtime operations assign subjects to those roles inside scope.

The design should not require every application to expose direct grant management to business
administrators. In most cases, the main operational surface should be role assignment.

---

## Resource Authorization

Flat permission checks answer:

> Can this actor execute this action in this scope?

Resource authorization answers:

> Can this actor execute this action on this specific resource?

The action-first model makes resource authorization more natural because the action instance already
carries resource identifiers.

```csharp
public interface IResourceAuthorizationService
{
    ValueTask<Result<ResourceAuthorizationDecision>> AuthorizeAsync<TResource>(
        ResourceAuthorizationRequest<TResource> request,
        CancellationToken cancellationToken = default);
}

public sealed record ResourceAuthorizationRequest<TResource>(
    TResource Resource,
    PermissionAction Action,
    IExecutionContext ExecutionContext);
```

`PermissionAction` should be a value object tied to a generated permission name, not a raw string.

```csharp
public sealed record PermissionAction(PermissionName PermissionName);
```

Resource authorization remains separate from data encryption. Application-specific encryption, such
as Aviendha's POCO document encryption model, remains an application concern unless a reusable
framework-level encryption abstraction emerges later.

The first resource-aware checker sample should use `RescheduleAppointment` and verify both grant and
resource decisions:

1. Structural validation runs before permission checks.
2. A missing or default appointment ID never reaches the permission checker.
3. A denied grant returns `PermissionErrors.PermissionDenied` and never loads the aggregate.
4. A grant success still requires appointment-level authorization.
5. A successful check allows the application service to run the domain use case.

---

## Client-Side Action Availability

The client philosophy is:

> Do not show a user an action that they are not permitted to perform.

The server remains authoritative. Client-side visibility is an affordance, not a security boundary.
The goal is to prevent users from initiating actions that the server will reject, while still
handling stale capability data safely.

### Client-side goals

1. Hide buttons, menu items, links, and commands when the current actor cannot execute the action.
2. Avoid duplicating permission strings in UI code.
3. Reuse generated action metadata from shared application contracts.
4. Support list/detail screens where action availability may depend on a specific resource.
5. Treat server rejection as authoritative when client data is stale.

### Capability snapshots

The server can expose an action capability snapshot for the current actor and scope.

Capability snapshots are cacheable read models with explicit freshness semantics. Global navigation
screens can request all available actions for a scope. Detail screens should request only the selected
actions they need for the current resource or view.

**Implemented in Phase 04 Task 13:** the shared capability contract surface now lives in `Aiel.Permissions.Application.Contracts` as `CapabilityContinuationToken`, `ActionCapabilityRequestMode`, `ActionCapabilityRequest`, `ActionCapabilitySnapshot`, and `IActionCapabilityService`. The client-side cache and refresh behavior lives in `Aiel.Permissions.Client` as `IActionCapabilitySnapshotCache` / `ActionCapabilitySnapshotCache`. Blazor visibility helpers live in `Aiel.Permissions.Client.Blazor` as `ActionCapabilityVisibility` and `CanExecute`.

```csharp
public interface IActionCapabilityService
{
    ValueTask<Result<ActionCapabilitySnapshot>> GetSnapshotAsync(
        ActionCapabilityRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record ActionCapabilityRequest(
    PermissionScope Scope,
    ActionCapabilityRequestMode Mode,
    IReadOnlyCollection<PermissionName> RequestedPermissions,
    ContinuationToken ContinuationToken);

public sealed record ActionCapabilitySnapshot(
    ActorId ActorId,
    PermissionScope Scope,
    CapabilitySnapshotVersion Version,
    IReadOnlyCollection<ActionCapability> Capabilities);

public sealed record ActionCapability(
    PermissionName PermissionName,
    ActionAvailability Availability);
```

`ActionCapabilityRequestMode` should distinguish all available actions from selected actions so an
empty `RequestedPermissions` collection is not ambiguous. `ContinuationToken` should use an explicit
empty value rather than `null` for the first page. Resource-specific capability checks should use a
separate request type rather than overloading the global snapshot request.

For global navigation and dashboards, a snapshot can answer coarse action availability:

- Can create appointment?
- Can view billing?
- Can manage clinic users?

For resource-specific actions, the client should request availability for specific action/resource
pairs or receive availability embedded in server view models.

```csharp
public sealed record ResourceActionCapabilityRequest(
    PermissionName PermissionName,
    ResourceReference Resource);
```

### Blazor component shape

Generated client helpers should keep UI code strongly typed.

```razor
<CanExecute Action="AviendhaActions.Scheduling.ChangeAppointment"
            Resource="appointment.Reference">
    <button @onclick="ChangeAppointment">Change appointment</button>
</CanExecute>
```

Or a generated action-specific component:

```razor
<CanChangeAppointment AppointmentId="appointment.Id">
    <button @onclick="ChangeAppointment">Change appointment</button>
</CanChangeAppointment>
```

The default behavior should be to hide unavailable actions. Disabling may be appropriate for transient
state such as incomplete form input, loading, or optimistic concurrency, but permission denial should
normally hide the affordance.

### Client-side validators versus permission checkers

FluentValidation-style client reuse may work for structural validation, but it likely breaks down for
permission checkers because permission decisions often require server-only data:

- Current grants and prohibitions.
- Scope chain resolution.
- Role and subject resolution.
- Resource ownership.
- Audit-sensitive facts.
- Secrets or data that must never be shipped to the client.

Recommended approach:

- Share generated action metadata with the client.
- Share structural validators only when they are deterministic and do not require protected data.
- Do not run full permission checkers on the client by default.
- Use a server capability endpoint or server-computed view model fields for permission visibility.

If a permission checker is pure and explicitly marked as client-safe, a later milestone can explore
client execution. The default assumption should be that permission checkers are server-side only.

### Staleness and race conditions

Client capability data can become stale when grants, roles, tenant membership, or resource state
changes. The server-side application service must still reject unauthorized actions.

The client should treat capability snapshots as cacheable but invalidatable:

- Include actor ID, scope, and snapshot version.
- Refresh after login, tenant switch, role change notifications, and failed authorization responses.
- Refresh after grant changes when the application can observe them.
- Prefer short-lived cache entries for resource-specific availability.
- Support selected-permission requests so large applications do not need to fetch the entire catalog
    for every screen.
- Never infer new permissions from hidden/visible UI state alone.
- Treat server rejection as authoritative and trigger a snapshot refresh when authorization fails.
- Consider push invalidation later, but do not require it for the first milestone.

The current implementation follows that rule by invalidating the cached request key and immediately refreshing the matching snapshot when an application-service call returns `PermissionDeniedError`.

RBAC adds one more client requirement: applications need role-aware invalidation semantics even when
the permission catalog does not change. A role assignment or role definition change can alter many
action capabilities at once. Capability snapshots should therefore be invalidated by an
authorization-state version, not only by permission snapshot version.

Manifest-driven permission rename migration remains owned by `Aiel.Permissions.EntityFrameworkCore` and `Aiel.Permissions.EntityFrameworkCore.PostgreSql`. The client packages consume stable permission names and snapshot versions only; they do not introduce a separate migration surface.

---

## Analyzer Requirements

Analyzers are what make convention safe. The action-centered model should include diagnostics for:

| Diagnostic | Condition | Severity |
| --- | --- | --- |
| Missing authorization story | Action has no concrete checker, generated permission, or explicit allow-without-permission marker | Error |
| Unstable permission rename | Action namespace/type change would alter a persisted permission name without migration metadata | Error |
| Raw permission string | Application code uses a raw permission name instead of generated constants/value objects | Warning or Error |
| Missing registration | Generated permission provider is not registered through a reachable dependency module | Error in CI |
| Missing role registration | Generated role definition provider is not registered through a reachable dependency module | Error in CI |
| Controller permission attribute | Controller or endpoint uses Aiel action permission attribute instead of ASP.NET Core authorization | Warning |
| Client unsafe checker | Permission checker is marked client-safe while depending on server-only services | Error |
| Missing client metadata | Action intended for UI use has no client display or grouping metadata | Warning |
| Discouraged action verb | Action type starts with a vague or reserved verb such as `Update`, `Modify`, `Manage`, `Process`, `Handle`, `Do`, `Run`, `Execute`, `Perform`, `Save`, `Set`, or `Edit` | Warning by default; configurable as Error |
| Role manifest drift | A default role changed stable identity, scope allowance, or permission membership without an explicit manifest migration | Error |
| Invalid role assignment metadata | A generated default role has no allowed assignee type or no allowed scope type | Error |

### Action verb diagnostics

Action names become permission names, audit language, generated constants, and client capability names.
That makes action verbs part of the long-term permission catalog, not just naming style.

The analyzer should work from a default list of discouraged verbs rather than a closed list of approved
verbs. This keeps module and application language expressive while still blocking low-information action
names.

Examples:

```csharp
// Discouraged.
public sealed record UpdateAppointment(AppointmentId AppointmentId) : ICommand;

// Better.
public sealed record RescheduleAppointment(AppointmentId AppointmentId, Instant StartsAt) : ICommand;
```

Applications and modules can extend or override the vocabulary explicitly:

```csharp
[DisallowedActionVerb("Update")]
[DisallowedActionVerb("Manage")]
[AllowedActionVerb("Triage")]
[AllowedActionVerb("Refer")]
[AllowedActionVerb("Enroll")]
public sealed partial class AviendhaActionVocabulary;
```

The analyzer should respect standard Roslyn suppression mechanisms. The preferred local exception is
`SuppressMessage` on the action type with a justification that explains why the discouraged verb is the
domain term of art.

```csharp
[SuppressMessage(
    "Aiel.ActionNames",
    "RHUARC1001:Avoid discouraged action verbs",
    Justification = "Submit is the domain term for sending an intake packet into review.")]
public sealed record SubmitIntakePacket(IntakePacketId PacketId) : ICommand;
```

Aiel should not invent a custom suppression attribute for this rule. A companion analyzer may report a
separate diagnostic when a Aiel analyzer suppression is missing a non-empty justification, especially
for permission-affecting diagnostics.

---

## Updated Implementation Milestones

| Milestone | Work |
| --- | --- |
| 1 | Introduce `Aiel.Application.Contracts` for `IAction`, `ICommand`, `IQuery<TResult>`, execution context contracts, and application service contracts |
| 2 | Freeze canonical permission identity: configured root, relative action namespace, action type name, `ActionPermissionNameAttribute`, stable ID, and manifest snapshot fields |
| 3 | Add structural action validation contracts and `IActionGate<TAction>` as an explicit application service helper |
| 4 | Add action permission checker contracts, default checker behavior, runtime `PermissionErrors`, and fail-closed analyzer rules |
| 5 | Generate permission constants, definitions, manifests, registration helpers, and committed manifest snapshots from action types |
| 6 | Introduce first-class role contracts: `RoleName`, `RoleDefinitionId`, `RoleAssignmentId`, `IRoleManager`, `IRoleDefinitionRegistry`, and `IRoleAssignmentStore` |
| 7 | Generate default role constants, role definitions, role manifests, registration helpers, and committed manifest snapshots for code-defined roles |
| 8 | Replace single-subject grant evaluation with `IEffectivePermissionSubjectResolver` and an RBAC-aware `IPermissionGrantEvaluator` that evaluates direct and role-derived grants |
| 9 | Implement `IPermissionManager`, `IRoleManager`, scope resolver, subject validators, and simple permission and role stores |
| 10 | Implement EF Core stores, permission and role migration DSL, and manifest-driven rename migration tests for both permissions and roles |
| 11 | Implement ASP.NET Core execution-context middleware, generated endpoint support, and HTTP client adapters that call application service contracts |
| 12 | Add resource authorization contracts and a `RescheduleAppointment` resource-aware checker sample that exercises role-derived authorization |
| 13 | Add client capability contracts, authorization-state invalidation, selected-permission snapshot requests, Blazor helpers, and server capability endpoint |
| 14 | Update Aviendha SAD.md with an action-based RBAC matrix, default role catalog, and client visibility guidance |

The first RBAC implementation slice should be deliberately narrow: `RescheduleAppointment`, one
validator, one resource-aware checker, one application service, one generated endpoint/client pair,
one default role such as `ClinicScheduler`, one role assignment path, one analyzer failure for a
missing authorization story, and one rename migration test for both `ChangeAppointment` to
`RescheduleAppointment` and the default role manifest that grants it.

---

## Current Open Questions

1. Should Aiel wrap `FluentValidation`, define a native validator abstraction, or support both?
2. Should client capability data be exposed as a general endpoint, embedded in generated API clients,
    or both?
3. Should generated Blazor helpers be component-based, service-based, or both?
4. Which action permission checkers, if any, can safely run on the client?
5. Should a concrete action checker replace the default grant check or compose with it by default?
6. Should direct user grants remain a first-class public admin feature or be positioned as an
    exception path behind role assignment?
7. Should default roles be code-defined only in v1, or should runtime-created custom roles ship in
    the first milestone?
8. Should nested roles ever be supported, or should Aiel keep role graphs flat by design?
9. Should generic command/query dispatchers remain as compatibility abstractions, lower-level
    primitives, or be deprecated after application service contracts become first-class?
10. How should action context flow through sagas, queues, and background workers?
11. What cache lifetime and invalidation trigger should be the default for global and resource-specific capability snapshots?

---

## Working Verdict

The action-centered model is still worth pursuing, but it should be completed as RBAC rather than
left as a permission-only system. The current generic subject model is a useful substrate, not the
finished design.

The main design risks are now twofold:

1. Convention drift for published permission and role identities.
2. Hidden evaluator complexity if effective subject resolution is bolted onto the current direct
    subject contracts instead of replacing them cleanly.

The design should resolve both by freezing published permission and role identities in committed
manifests, preserving stable IDs across renames, requiring migration DSL operations for identity,
scope, lifecycle, and role membership changes, and replacing the single-subject evaluator surface
with explicit RBAC-aware effective subject resolution. With that tooling in place, the action-
centered approach is more aligned with Aiel than an attribute-first permission model or an
application-specific role layer bolted on afterward.
