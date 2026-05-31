# Permission System Design Review Four

## Doug's thoughts on the `[RequirePermission]` attribute

Using the attribute on Commands and Queries feels like unnecessary ceremony. I think we should consider using a convention based approach where the namespace + type of the command or query is the permission key. Further, if we include the command or query in the execution context, it will bring the resource identifier along with it.

Essentially, if we follow the convention strictly, application services would not need to be decorated with the `[RequirePermission]` attribute because they will never be called if the permission check fails in the pipeline.

We have already established that permission definitions should be based on actions and not CRUD operations, and since a command is an `object` with the parameters necessary to complete the action, it already has the majority of the context required for the permission check, including resource identifiers.

Queries are also essentially actions. `FindClient`, `ListAppointments`, etc. If we introduce a common base `IAction` interface we open up a world of opportunity.

Replace `IExecutionContext.OperationId` with an `IExecutionContext.Action`, we now have all the context required for the entire pipeline.

Note: We know that actions can come from origins other than HTTP Controllers, like a message queue or a Saga, so we must have validation and permission checking as part of the pipeline. That makes permission checking at the controller redundant. Same with validation.

If we take some inspiration from `FluentValidation`, we end up with something like this:

```csharp
public interface IAction;

public interface ICommand : IAction;

public interface IQuery : IAction;

public interface IExecutionContext
{
    IAction Action { get; }

    IActor Actor { get; }

    Guid CorrelationId { get; }

    Guid? CausationId { get; }

    Guid? ClientInstanceId { get; }

    IDictionary<String, Object?> Properties { get; }
}

public interface IPermissionChecker
{
   ValueTask<Result> CheckAsync(
      IExecutionContext executionContext,
      CancellationToken cancellationToken = default);
}

public class ChangeAppointment : ICommand
{
    public AppointmentId AppointmentId { get; init;}
    public DateTimeOffset? NewDateTime { get; init;}
    public LocationId? NewLocation {get; init; }
    ...
}

public class ChangeAppointmentValidator : AbstractValidator<ChangeAppointment>
{
    public ChangeAppointmentValidator()
    {
        RuleFor(a => a.AppointmentId).NotNull().NotEmpty();
        ...
    }
}

public class ChangeAppointmentPermissionChecker : AbstractPermissionChecker<ChangeAppointment>
{
    public override async ValueTask<Result> CheckAsync(
        IExecutionContext context,
        CancellationToken cancellationToken = default);
    {
        // 
    }
}
```

At this point, the only reason to keep the '[RequirePermission]' attribute would be to allow customizing a fallback permission checker when there is no concrete `PermissionChecker<IAction>`. For example, declaring the `PermissionScopeType`

I know this is not a complete solution as it is only concerned with the server side pipeline, and I am not suggesting we throw away the current design. I just think it is worth exploring.

---

## Copilot's Response

That is a genuinely interesting turn. I’ll think through it as a design proposal first, not jump straight into editing, because this touches the CQRS model, execution context, validation, authorization, and permission lifecycle all at once.

I think you are heading in a very good direction. Not “throw away the current design,” but “this may be the missing abstraction that makes the design feel native instead of decorated.”

The key move is this: **permission is not really attached to a string; permission is attached to an action.** A permission name is the durable catalog key for that action. The command/query object is the runtime instance of the action, with resource identifiers and intent already inside it.

That is strong.

**What I Would Keep**
I would absolutely explore:

```csharp
public interface IAction;

public interface ICommand : IAction;

public interface IQuery<TResult> : IAction;
```

Then make validation, authorization, telemetry, auditing, and dispatch all understand “the current action.”

I would also seriously consider type-based permission checkers:

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

That shape is very Aiel: explicit, typed, testable, pipeline-native, no magic strings in handler code.

It also aligns with prior art that has worked well: `FluentValidation` validators, MediatR pipeline behaviors, and request-specific authorization behaviors. This pattern has not been “tried and abandoned” as a category. The brittle versions are the ones that rely on hidden convention with no analyzer/generator support.

**What I Would Change**
I would not replace `OperationId` with `Action`.

`OperationId` is trace identity. `Action` is the payload being executed. They are different things, and we need both.

I would instead evolve toward something like:

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

The dispatcher creates the action context:

```csharp
var childContext = ActionExecutionContext<TCommand>.CreateChild(parentContext, command);
```

That keeps the existing trace model intact while making the current command/query first-class.

**The Permission Key Question**
I like the convention idea, but I would be careful with:

> namespace + type name is the permission key

As a default, yes. As the persisted key, maybe not blindly.

The risk is that namespace/type refactors become authorization data migrations. That is manageable only if the generator emits a manifest and the analyzer forces an explicit migration when the action identity changes.

So I would frame it like this:

- Action type is the source of permission intent.
- Generator derives the default permission name from the action type.
- Generator emits constants, definitions, and manifest.
- Analyzer detects renamed/moved action types that would change persisted permission names.
- EF migration DSL handles the rename when it is intentional.

Example:

```csharp
public sealed record ChangeAppointment(
    AppointmentId AppointmentId,
    DateTimeOffset? NewDateTime,
    LocationId? NewLocation) : ICommand;
```

Generated:

```csharp
public static partial class AviendhaPermissions
{
    public static partial class Scheduling
    {
        public const string ChangeAppointment =
            "Aviendha.Scheduling.ChangeAppointment";
    }
}
```

Generated definition:

```csharp
builder.Group("Aviendha.Scheduling", "Scheduling")
    .Permission(AviendhaPermissions.Scheduling.ChangeAppointment)
        .Action<ChangeAppointment>()
        .Scope(AviendhaPermissionScopes.Clinic)
        .Lifecycle(PermissionLifecycle.Active);
```

That preserves the no-magic-string defense while letting the action drive the catalog.

**Fallback And Fail-Closed**
The critical rule: every action must have an authorization story.

Possible valid states:

- A concrete `IActionPermissionChecker<TAction>` exists.
- A generated convention permission exists and default checker can evaluate it.
- The action is explicitly marked as not requiring permission.
- The analyzer fails the build.

I would not allow “no checker found” to mean “allowed.”

Something like:

```csharp
public interface IActionAuthorizationStrategy<TAction>
    where TAction : IAction
{
    ValueTask<Result> AuthorizeAsync(
        TAction action,
        IActionExecutionContext<TAction> context,
        CancellationToken cancellationToken = default);
}
```

The default strategy can do catalog/store permission checks. A concrete checker handles resource-specific cases.

**Controller Authorization**
I agree with your instinct that controller permission checks become redundant if the pipeline is authoritative.

I would still keep ASP.NET Core authorization for authentication and transport-level policy:

```csharp
[Authorize]
public sealed class AppointmentsController : AielControllerBase
{
    public Task<IActionResult> Change(ChangeAppointment request)
        => DispatchAsync(request);
}
```

No `[RequirePermission]` on the controller. No permission policy needed on the action unless we later generate endpoint metadata for docs or early rejection. The command pipeline is the source of truth.

**Where Resource Authorization Fits**
Your `ChangeAppointment` example is exactly why action-first is attractive. The action contains `AppointmentId`, so a checker can do:

```csharp
public sealed class ChangeAppointmentPermissionChecker
    : AbstractPermissionChecker<ChangeAppointment>
{
    public override async ValueTask<Result> CheckAsync(
        ChangeAppointment action,
        IActionExecutionContext<ChangeAppointment> context,
        CancellationToken cancellationToken = default)
    {
        var permission = await Permissions.CheckAsync(
            AviendhaPermissions.Scheduling.ChangeAppointment,
            context,
            cancellationToken);

        if (!permission.IsSuccess)
        {
            return permission;
        }

        return await Resources.AuthorizeAsync(
            action.AppointmentId,
            AppointmentActions.Change,
            context,
            cancellationToken);
    }
}
```

That gives us a clean composition:

1. Can the actor perform the action generally?
2. Can the actor perform it on this resource?
3. If yes, the handler runs.

**My Verdict**
This is worth exploring. I think it may be better than the attribute-first design.

The refined direction I would propose is:

- Introduce `IAction` as the root abstraction.
- Make commands and queries actions.
- Add typed action execution contexts, but keep `OperationId`.
- Generate permission definitions from actions by convention.
- Keep generated permission constants.
- Use analyzers to require every action to have an authorization strategy.
- Use concrete `IActionPermissionChecker<TAction>` for non-trivial/resource-aware checks.
- Keep ASP.NET Core `[Authorize]` for authentication/transport, not action permission semantics.
- Treat the command/query pipeline as the authoritative validation and permission boundary.

This feels very Aiel: action-centered, strongly typed, convention guided by generators/analyzers, and explicit where the design matters.
