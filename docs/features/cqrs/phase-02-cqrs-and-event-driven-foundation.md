# Phase 02 — CQRS Execution Path and Event-Driven Foundation

## Status

**Complete** (Implementation code is now the authoritative reference; this document retained for reference and project history)

---

## Authority Notice

The Aiel codebase (contracts, implementations, and tests) is the authoritative specification for this phase. This document describes the planning context and historical rationale. Refer to the implementation in `Aiel/src/` and test coverage in `Aiel/tests/` as the definitive current behavior.

---

## Problem Statement

The framework has well-formed contracts for the command and query sides but no runnable execution path.
More importantly, those contracts define only two of the five segments of the full application flow.
A developer adopting Aiel today can declare a command and write a handler, but cannot dispatch the
command, cannot load or save an aggregate, cannot raise domain events, cannot update a read model,
and cannot serve a query result.

This phase defines — and where appropriate implements — every interface in the end-to-end flow:

```
UX → Command Endpoint → Dispatcher → Pipeline → Handler →
Aggregate → Domain Events → Event Handlers → Read Models →
Query Dispatcher → Query Pipeline → Query Handler → UX
```

Phase 02 establishes the contracts for all of those seams, implements what is independent of
persistence infrastructure, and defers what requires EF Core to Phase 03.

---

## What Is Being Developed

### 2.1  Execution context — mutable with a property bag

`IExecutionContext` gains a `Properties` dictionary that pipeline stages, handlers, and event
handlers can read and write throughout the lifetime of a single dispatch.
`DefaultExecutionContext` is converted from a `sealed record` to a `sealed class` to make its
mutable semantics explicit.

### 2.2  Pipeline behavior infrastructure

Two distinct pipeline interfaces — `ICommandPipelineBehavior<TCommand>` and
`IQueryPipelineBehavior<TQuery, TResult>` — wrap command and query dispatch respectively.
Each has a matching non-swappable delegate type (`CommandPipelineHandlerDelegate` and
`QueryPipelineHandlerDelegate<TResult>`) that captures input and context in the closure.
A behavior MUST NOT implement both interfaces; it MAY share implementation via a base class.

### 2.3  Command dispatcher implementation

`DefaultCommandDispatcher` resolves the registered `ICommandHandler<TCommand>` from the DI
container, builds and executes the behavior pipeline, and returns the final `Result`.

### 2.4  Query dispatcher implementation

`DefaultQueryDispatcher` follows the same pattern, resolving `IQueryHandler<TQuery, TResult>` and
executing the behavior pipeline, returning `Result<TResult>`.

### 2.5  Domain event infrastructure

`IDomainEventHandler<TDomainEvent>` and `IDomainEventDispatcher` are declared in `Aiel.Domain`.
`DefaultDomainEventDispatcher` is implemented in `Aiel.Application` — it resolves all registered
`IDomainEventHandler<TEvent>` implementations from DI and dispatches in registration order.

### 2.6  Aggregate repository contract

`IRepository<TAggregate, TId>` is declared in `Aiel.Domain` to define the write-side
persistence contract.  EF Core implementations are Phase 03.

### 2.7  Built-in logging behaviors

`CommandLoggingPipelineBehavior<TCommand>` and `QueryLoggingPipelineBehavior<TQuery, TResult>`
are the first concrete pipeline behaviors.  Both emit structured log entries before and after
every dispatch, including the input type name, correlation ID from the execution context, and
success/failure outcome.  Shared logic lives in an internal `PipelineLoggingHelper` to avoid
duplication across the two classes.

### 2.8  Handler and behavior registration

`AddAielCqrs(params Assembly[])` extension scans the provided assemblies and registers:
- All `ICommandHandler<TCommand>` implementations as scoped
- All `IQueryHandler<TQuery, TResult>` implementations as scoped
- All `IDomainEventHandler<TDomainEvent>` implementations as scoped
- `DefaultCommandDispatcher` as scoped `ICommandDispatcher`
- `DefaultQueryDispatcher` as scoped `IQueryDispatcher`
- `DefaultDomainEventDispatcher` as scoped `IDomainEventDispatcher`

Behaviors are *not* auto-registered; the developer registers them explicitly after calling
`AddAielCqrs`, which keeps ordering intentional and visible.

### 2.9  Test project

A new `Aiel.Application.UnitTests` project covers dispatchers, pipeline composition, behavior
ordering, and registration discovery.

---

## Decisions

### D1 — Mutable execution context: class vs. record

**Context.**
The user confirmed that the execution context should be mutable and modifiable by each pipeline
stage.  The current `DefaultExecutionContext` is a `sealed record`, which implies value semantics
and communicates immutability by convention.

**Options.**

| Option | Description |
|--------|-------------|
| A — `record` with mutable `Properties` | Keep `record`; rely on the fact that the `Properties` dict reference is stable even though its contents change. |
| B — `sealed class` | Change to a class; explicitly communicates mutable reference semantics. |

**Decision: B — sealed class.**

A `record` with a mutable dictionary is semantically contradictory.  Records signal immutability
and value-equality, which is the wrong contract for an object that pipelines will intentionally
mutate.  The existing factory methods (`CreateRoot`, `CreateChild`) continue to work unchanged;
only the declaration keyword changes.  Default reference equality is the correct equality for a
mutable context object.

---

### D2 — Properties bag value type: `object` vs. `object?`

**Context.**
The Properties dictionary allows pipeline stages to attach and retrieve typed values by key.

**Options.**

| Option | Description |
|--------|-------------|
| A — `IDictionary<string, object>` | Non-nullable values only. |
| B — `IDictionary<string, object?>` | Nullable values; consistent with the reality that typed extension methods will use `TryGetValue`. |

**Decision: B — `IDictionary<string, object?>`.**

Typed extension methods will always use `TryGetValue` and pattern-match the retrieved value.
Forcing non-null in the bag means callers have to store sentinel objects instead of `null`, which
is artificial.  Nullable values make the real contract explicit.

---

### D3 — Pipeline behavior interface shape

**Context.**
Commands produce `Task<Result>`.  Queries produce `Task<Result<TResult>>`.  Behaviors must
wrap both shapes.

**Options.**

| Option | Description |
|--------|-------------|
| A — Separate `ICommandPipelineBehavior<TCommand>` and `IQueryPipelineBehavior<TQuery, TResult>` | Explicit split; two separate registration paths. |
| B — Unified `IPipelineBehavior<TInput, TOutput>` | Single interface; open-generic DI handles registration for all combinations. |

**Decision: A — separate `ICommandPipelineBehavior<TCommand>` and `IQueryPipelineBehavior<TQuery, TResult>`.**

Separate interfaces provides clear separation of concerns and explicit registration paths. Constraints can be applied
appropriately, and the DI container respects those constraints during resolution. A behavior MUST NOT implement both interfaces
but MAY have a common base class. This should be enforced by an analyzer in a later phase.

---

### D4 — Pipeline delegate signature

**Context.**
The "next in chain" delegate that each behavior receives to call the downstream stage.

**Options.**

| Option | Description |
|--------|-------------|
| A — `delegate Task<TOutput> PipelineHandlerDelegate<TOutput>(TInput input, IExecutionContext context, CancellationToken ct)` | Allows behaviors to replace input or context before passing downstream. |
| B — `delegate Task<TOutput> PipelineHandlerDelegate<TOutput>(CancellationToken ct = default)` | Input and context are captured by the closure when the chain is constructed. |

**Decision: B — context-capturing delegate.**

Because `IExecutionContext` is mutable by reference, a behavior can write to `context.Properties`
and the downstream stage will see those writes without needing them passed through the delegate.
Passing input through the delegate would imply behaviors can swap the input object itself, which
is a footgun — if a behavior needs to transform input it should do so before calling `next`.
The simpler delegate signature results in cleaner behavior implementations.

---

### D5 — Execution context child propagation in the dispatcher

**Context.**
The dispatcher receives an `IExecutionContext` from the caller.  `DefaultExecutionContext.CreateChild`
already exists to establish a causal chain.

**Decision: yes — the dispatcher creates a child context for every dispatch.**

The child receives a new `OperationId`, inherits the `CorrelationId`, and records the caller's
`OperationId` as its `CausationId`.  This makes the entire operation tree reconstructible from
logs or telemetry.  The caller's context is not modified.  The child context is what the pipeline
and handler see.

---

### D6 — Domain event dispatch timing and durability

**Context.**
Domain events must be dispatched after the aggregate's state changes are committed.  Full
durability requires an outbox (Phase 03).

**Decision: Phase 02 provides synchronous in-process dispatch only.**

`DefaultDomainEventDispatcher` dispatches events synchronously within the same thread/task after
the handler explicitly calls it.  It is designed to be replaced by an outbox-backed implementation
in Phase 03 without any change to the `IDomainEventDispatcher` contract or to any handler code.

The recommended command handler pattern is:
```csharp
var aggregate = await _repository.GetByIdAsync(id, ct);
aggregate.DoSomething();
await _unitOfWork.SaveChangesAsync(ct);
await _eventDispatcher.DispatchAsync(aggregate.DomainEvents, _context, ct);
aggregate.ClearDomainEvents();
```

This ordering is important: persist first, dispatch second.  If dispatch fails, events can be
replayed from the persisted aggregate state.  The outbox in Phase 03 will make this automatic and
transactional.

---

### D7 — Domain event handler return type

**Context.**
Command handlers return `Result` to communicate success/failure back to the dispatcher.  Domain
event handlers are side-effect processors (read model updates, integration events, etc.) — they do
not report results back to their callers.

**Options.**

| Option | Description |
|--------|-------------|
| A — `Task HandleAsync(...)` | Handlers throw on failure; dispatcher propagates the exception. |
| B — `Task<Result> HandleAsync(...)` | Handlers return a Result; dispatcher aggregates them. |

**Decision: A — `Task HandleAsync(...)`, no Result.**

Domain event handlers represent a commitment: once an event is raised and persisted, the system
MUST eventually process it.  Wrapping the handler in a Result pattern suggests that "I chose not
to process this event" is a valid outcome, which it is not.  A handler that cannot complete should
throw, triggering retry at the dispatch layer (via the outbox in Phase 03).  Keeping the return
type as `Task` also makes projection handlers simpler to write.

---

### D8 — Repository interface location

**Decision: `IRepository<TAggregate, TId>` lives in `Aiel.Domain`.**

Aggregates are domain objects.  The repository is the domain's abstraction over aggregate
persistence.  Applications reference `Aiel.Domain` to write command handlers; having the
repository contract there removes the need for an additional package reference just to declare
a handler that loads an aggregate.

`IDomainEventDispatcher` and `IDomainEventHandler<TDomainEvent>` also live in `Aiel.Domain`
for the same reason.

---

### D9 — Built-in behaviors for Phase 02

**Decision: logging only.**

`CommandLoggingPipelineBehavior<TCommand>` and `QueryLoggingPipelineBehavior<TQuery, TResult>` are the only built-in behaviors in this phase, sharing the internal `PipelineLoggingHelper` for structured log formatting.

| Behavior | Phase |
|----------|-------|
| Logging (`CommandLoggingPipelineBehavior` + `QueryLoggingPipelineBehavior`) | 02 (this phase) |
| Unit-of-work auto-save | 03 (requires repository + EF Core wiring) |
| Validation | 04 (requires a validation abstraction) |
| Authorization | 05 (requires the current-user abstraction) |

---

### D10 — `MessageContext<TMessage>` and domain event handlers

**Context.**
`Aiel.Domain` already contains `MessageContext<TMessage>` which wraps a message and an
`IExecutionContext` together.

**Decision: `MessageContext` is reserved for the messaging/outbox layer; domain event handlers
use the explicit parameter form for consistency with command and query handlers.**

The explicit form:
```csharp
Task HandleAsync(TDomainEvent domainEvent, IExecutionContext context, CancellationToken ct = default);
```

mirrors `ICommandHandler<TCommand>` and `IQueryHandler<TQuery, TResult>` exactly.
`MessageContext` will become relevant in Phase 03 when messages arrive from an external bus with
their own correlation headers.

---

## Interface Catalog

The following table lists every interface touched or created in this phase. Note: implementation placed these contracts in `Aiel.Application.Contracts` rather than in `Aiel.Domain` and `Aiel.Application` as originally drafted; the namespace hierarchy remains as planned, but the project boundaries reflect the evolved architecture.

| Interface | Project | New / Modified | Notes |
|-----------|---------|---------------|-------|
| `IExecutionContext` | `Aiel.Application.Contracts` | Modified | Add `IActor Actor`, `IDictionary<string, object?> Properties` |
| `DefaultExecutionContext` | `Aiel.Application.Contracts` | Modified | `record` → `class`; add `Properties` |
| `ICommandPipelineBehavior<TCommand>` | `Aiel.Application.Contracts` | New | Command pipeline behavior contract |
| `IQueryPipelineBehavior<TQuery, TResult>` | `Aiel.Application.Contracts` | New | Query pipeline behavior contract |
| `CommandPipelineHandlerDelegate` | `Aiel.Application.Contracts` | New | `delegate Task<Result>(CancellationToken)` — next stage for commands |
| `QueryPipelineHandlerDelegate<TResult>` | `Aiel.Application.Contracts` | New | `delegate Task<Result<TResult>>(CancellationToken)` — next stage for queries |
| `ICommandDispatcher` | `Aiel.Application.Contracts` | Existing | No changes; already cleaned up |
| `DefaultCommandDispatcher` | `Aiel.Application` | New | DI-based, pipeline-aware implementation |
| `IQueryDispatcher` | `Aiel.Application.Contracts` | Existing | No changes |
| `DefaultQueryDispatcher` | `Aiel.Application` | New | DI-based, pipeline-aware implementation |
| `IRepository<TAggregate, TId>` | `Aiel.Domain` | New | Write-side repository contract |
| `IDomainEventHandler<TDomainEvent>` | `Aiel.Application.Contracts` | New | Per-event-type handler contract (namespace: `Aiel.Domain`) |
| `IDomainEventDispatcher` | `Aiel.Application.Contracts` | New | Dispatches a collection of domain events (namespace: `Aiel.Domain`) |
| `DefaultDomainEventDispatcher` | `Aiel.Application` | New | In-process synchronous implementation |
| `CommandLoggingPipelineBehavior<TCommand>` | `Aiel.Application` | New | Built-in command logging; shares `PipelineLoggingHelper` |
| `QueryLoggingPipelineBehavior<TQuery, TResult>` | `Aiel.Application` | New | Built-in query logging; shares `PipelineLoggingHelper` |
| `AielCqrsServiceCollectionExtensions` | `Aiel.Application` | New | `AddAielCqrs(params Assembly[])` |

---

## Contract Sketches

The following represent the implemented signatures. **Note:** application-layer contracts are implemented in `Aiel.Application.Contracts` project, while implementations live in `Aiel.Application`.

```csharp
// Aiel.Application.Contracts — IExecutionContext
public interface IExecutionContext
{
    Guid OperationId { get; }
    IActor Actor { get; }
    Guid CorrelationId { get; }
    Guid? CausationId { get; }
    Guid? ClientInstanceId { get; }
    IDictionary<string, object?> Properties { get; }  // implemented
}

// Aiel.Domain — IRepository<TAggregate, TId>
public interface IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull, IStrongId
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}

// Aiel.Application.Contracts — IDomainEventHandler<TDomainEvent> (namespace: Aiel.Domain)
public interface IDomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent, IExecutionContext context,
        CancellationToken cancellationToken = default);
}

// Aiel.Application.Contracts — IDomainEventDispatcher (namespace: Aiel.Domain)
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, IExecutionContext context,
        CancellationToken cancellationToken = default);
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, IExecutionContext context,
        CancellationToken cancellationToken = default);
}

// Aiel.Application.Contracts — ICommandPipelineBehavior<TCommand>
public interface ICommandPipelineBehavior<TCommand>
    where TCommand : ICommand
{
    Task<Result> HandleAsync(TCommand command, IExecutionContext context,
        CommandPipelineHandlerDelegate next,
        CancellationToken cancellationToken = default);
}

// Aiel.Application.Contracts — CommandPipelineHandlerDelegate
public delegate Task<Result> CommandPipelineHandlerDelegate(
    CancellationToken cancellationToken = default);

// Aiel.Application.Contracts — IQueryPipelineBehavior<TQuery, TResult>
public interface IQueryPipelineBehavior<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, IExecutionContext context,
        QueryPipelineHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default);
}

// Aiel.Application.Contracts — QueryPipelineHandlerDelegate<TResult>
public delegate Task<Result<TResult>> QueryPipelineHandlerDelegate<TResult>(
    CancellationToken cancellationToken = default);
```

---

## Completion Criteria

- [x] `IExecutionContext.Properties` exists; `DefaultExecutionContext` is a class with a live `Properties` dictionary; all existing tests still pass.
- [x] `DefaultCommandDispatcher` implements `ICommandDispatcher`; dispatches the handler; runs the pipeline in registration order (outermost registered = outermost wrapper).
- [x] `DefaultQueryDispatcher` implements `IQueryDispatcher`; same pipeline semantics.
- [x] Each dispatcher creates a child `DefaultExecutionContext` per dispatch; child's `CausationId` equals the parent's `OperationId`.
- [x] Zero-behavior pipeline dispatches directly to the handler without error.
- [x] Multiple-behavior pipeline executes in registration order; order is validated by a test.
- [x] `IDomainEventHandler<TDomainEvent>` and `IDomainEventDispatcher` declared in `Aiel.Domain`; `DefaultDomainEventDispatcher` in `Aiel.Application` resolves handlers from DI and invokes them in registration order.
- [x] `IRepository<TAggregate, TId>` declared in `Aiel.Domain`.
- [x] `CommandLoggingPipelineBehavior<TCommand>` and `QueryLoggingPipelineBehavior<TQuery, TResult>` each write a structured entry before and after dispatch including input type name, correlation ID, and success/failure.
- [x] `AddAielCqrs(params Assembly[])` registers the three handler types (`ICommandHandler<>`, `IQueryHandler<,>`, `IDomainEventHandler<>`) plus the three dispatchers; duplicate registrations do not cause errors.
- [x] `Aiel.Application.UnitTests` project exists; all new behavior is covered by unit tests.
- [x] 701 baseline tests continue to pass.
- [x] Build is clean with zero warnings.

---

## Implementation Plan

| Task | Description | Gate |
|------|-------------|------|
| ✅ 1 | Modify `IExecutionContext`; convert `DefaultExecutionContext` to class; update all affected stubs; build green | Build |
| ✅ 2 | Declare `IRepository<TAggregate, TId>`, `IDomainEventHandler<TDomainEvent>`, `IDomainEventDispatcher` in `Aiel.Domain`; build green | Build |
| ✅ 3 | Declare `ICommandPipelineBehavior<TCommand>` + `CommandPipelineHandlerDelegate`, `IQueryPipelineBehavior<TQuery, TResult>` + `QueryPipelineHandlerDelegate<TResult>` in `Aiel.Application`; build green | Build |
| ✅ 4 | Create `Aiel.Application.UnitTests` project; write failing tests for `DefaultCommandDispatcher`, `DefaultQueryDispatcher`, pipeline ordering, child context propagation, and registration | G1 |
| ✅ 5 | Implement `DefaultCommandDispatcher` and `DefaultQueryDispatcher`; run tests | G2 partial |
| ✅ 6 | Write failing tests for `DefaultDomainEventDispatcher`; implement it | G2 partial |
| ✅ 7 | Write failing tests for `CommandLoggingPipelineBehavior` and `QueryLoggingPipelineBehavior`; implement both sharing `PipelineLoggingHelper` | G2 partial |
| ✅ 8 | Write failing tests for `AddAielCqrs`; implement the registration extension | G2 full |
| ✅ 9 | Run full suite; confirm 0 regressions | G2 |
| ✅ 10 | Build clean | G3 |
| 11 | Update `Framework.md` with CQRS execution path documentation | Docs |

---

## Deferred to Later Phases

| Item | Phase |
|------|-------|
| EF Core `IRepository<TAggregate, TId>` implementation | 03 |
| Wire `PersistDomainEventsAsync` in `AielDbContext` after `SaveChangesAsync` | 03 |
| Outbox-backed `IDomainEventDispatcher` | 03 |
| `UnitOfWorkPipelineBehavior` | 03 |
| Projection checkpoint infrastructure | 03 |
| `ValidationPipelineBehavior` (FluentValidation or custom) | 04 |
| `ICurrentUser` abstraction and `AuthorizationPipelineBehavior` | 05 |
| HTTP → `IExecutionContext` resolution middleware | 05 |
| Integration event publishing (external bus) | 06 |
