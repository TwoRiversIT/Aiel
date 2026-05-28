# Domain Primitives

## AggregateRoot

> **Goal:** Support both state-based and event-sourced aggregates with one consistent developer mental model, without pretending their persistence or query semantics are the same.

### Design decision

Aiel should define:

- one thin abstract base named `AggregateRoot<TKey>`
- one explicit state-based aggregate root named `StateBasedAggregateRoot<TKey>`
- one event-sourced aggregate root named `EventSourcedAggregateRoot<TKey>`
- one shared domain-event mechanism across both root types
- one unified version concept across both root types
- one explicit write-side repository abstraction
- one explicit read-side repository abstraction

This preserves the real storage differences while keeping the developer experience consistent and predictable.

### The core problem

State-based and event-sourced aggregates cannot be queried the same way.

- State-based aggregates are usually persisted in relational storage.
- Event-sourced aggregates are rehydrated from event streams.
- Arbitrary query predicates belong on read models, not on event streams.

So the right abstraction is not “one fake query model for both.”

The right abstraction is:

- one write-side model for aggregates
- one read-side model for read models and projections

That split should be explicit for both aggregate styles.

### Shared base contract

```csharp
namespace Aiel.Domain;

public abstract class AggregateRoot<TKey> : Entity<TKey>
    where TKey : notnull, IStrongId
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    protected AggregateRoot(TKey id)
        : base(id)
    {
    }

    protected AggregateRoot()
    {
    }

    protected void RaiseEvent(IDomainEvent domainEvent);

    protected virtual void OnRaiseEvent(IDomainEvent domainEvent);

    public void ClearDomainEvents();
}
```

Semantics:

- `AggregateRoot<TKey>` contains only behavior common to both aggregate styles.
- It owns the shared domain-event collection and exposure model.
- `RaiseEvent` is the one canonical method name for recording newly raised domain events.
- `RaiseEvent` should use a template-method style hook so concrete aggregate styles can customize semantics without method hiding.
- It does not know how aggregates are persisted.
- It does not know whether persistence is relational or event-sourced.
- It does not know about repositories, EF Core, or event stores.

### State-based aggregate root

```csharp
namespace Aiel.Domain;

public abstract class StateBasedAggregateRoot<TKey> : AggregateRoot<TKey>
    where TKey : notnull, IStrongId
{
    protected StateBasedAggregateRoot(TKey id)
        : base(id)
    {
    }

    protected StateBasedAggregateRoot()
    {
    }
}
```

Semantics:

- The state-based root enforces invariants by mutating internal state directly.
- It may raise domain events through the same event mechanism used by event-sourced roots.
- Persistence is typically a current-state snapshot in relational storage.

### Event-sourced aggregate root

```csharp
using Aiel.Domain.EventSourcing;

namespace Aiel.Domain;

public abstract class EventSourcedAggregateRoot<TKey> : AggregateRoot<TKey>, IRehydrateFromHistory
    where TKey : notnull, IStrongId
{
    protected EventSourcedAggregateRoot(TKey id)
        : base(id)
    {
    }

    protected EventSourcedAggregateRoot()
    {
    }

    void IRehydrateFromHistory.RehydrateFromHistory(IEnumerable<IDomainEvent> history);

    protected abstract void Apply(IDomainEvent domainEvent);

    protected override void OnRaiseEvent(IDomainEvent domainEvent);
}
```

```csharp
namespace Aiel.Domain.EventSourcing;

public interface IRehydrateFromHistory
{
    void RehydrateFromHistory(IEnumerable<IDomainEvent> history);
}
```

Semantics:

- The event-sourced root enforces invariants by deciding which events may be raised.
- `RaiseEvent` applies the event to current state and records it in the same shared domain-event collection model used by state-based aggregates.
- `RehydrateFromHistory` rehydrates state from historical events without re-recording them as new uncommitted events.
- `RehydrateFromHistory` should not be part of the normal domain programming surface.
- A good starting point is to hide `RehydrateFromHistory` behind explicit interface implementation so persistence infrastructure can access it deliberately while normal aggregate usage does not see it.
- `IRehydrateFromHistory` belongs in a narrower `Aiel.Domain.EventSourcing` namespace because it is an event-sourcing seam, not a general domain primitive.
- `Apply` is the dispatch entry point, but the actual event handling pattern should be strongly typed.
- `OnRaiseEvent` is the event-sourced hook where raising a new event also applies it to current state and advances `Version`.

### Shared domain-event mechanism

Both aggregate styles should use the same domain-event mechanism.

- Both expose `IReadOnlyList<IDomainEvent> DomainEvents`.
- Both use the same `RaiseEvent` method name on the base type to record newly raised events.
- Infrastructure clears recorded events after they are persisted to the outbox or event store.

This is intentionally different from some existing systems, but there is no architectural downside as long as the semantics are documented clearly.

The benefit is consistency:

- developers work with domain events the same way regardless of aggregate style
- logging and correlation flow the same way
- the eventual consistency model stays uniform

### Event dispatch pattern

Aiel should start with a typed dispatch pattern for event-sourced aggregates.

The framework should prefer explicit typed handlers over reflection, naming conventions, or hidden registration.

Recommended shape:

```csharp
protected override void Apply(IDomainEvent domainEvent)
    => domainEvent switch
    {
        OrderCreated e => Apply(e),
        OrderLineAdded e => Apply(e),
        _ => throw new InvalidOperationException($"Unsupported event type: {domainEvent.GetType().Name}")
    };

protected abstract void Apply(OrderCreated domainEvent);

protected abstract void Apply(OrderLineAdded domainEvent);
```

Guidance:

- The non-generic `Apply(IDomainEvent)` method remains the single dispatch entry point.
- Concrete aggregates should implement strongly typed `Apply` overloads for each supported event.
- Adding a new event should require an explicit update to the dispatch switch and a new typed handler.
- That extra ceremony is acceptable in v1 because it keeps behavior visible and easy to debug.

This should be treated as the starting point, not an irreversible decision.

If experience shows the pattern becomes too repetitive, Aiel can refine it later with generator support, but should not begin with magic.

### Unified version semantics

Aiel should use one unified version concept across both aggregate styles.

- `Version` is monotonic.
- `Version` is part of concurrency control, not identity.
- `Version` is not part of equality.

Interpretation by aggregate type:

- For state-based aggregates, `Version` is the persisted concurrency version for the current state.
- For event-sourced aggregates, `Version` is the stream revision or number of applied events.

This is a good unification, not a compromise.

It gives developers one concurrency concept instead of two nearly identical ones with different names.

### Write-side repository

```csharp
using Aiel.Results;

namespace Aiel.Domain;

public interface IAggregateRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull, IStrongId
{
    Task<Result<TAggregate>> LoadAsync(TId id, CancellationToken ct = default);

    Task<Result> SaveAsync(TAggregate aggregate, CancellationToken ct = default);
}
```

Semantics:

- `LoadAsync` is the write-side way to obtain an aggregate for command handling.
- `SaveAsync` persists aggregate changes using the appropriate persistence mechanism.
- The interface is the same for both aggregate styles.
- The implementation differs by persistence model.

Implementations:

- State-based aggregates: `LoadAsync` and `SaveAsync` use relational persistence.
- Event-sourced aggregates: `LoadAsync` rehydrates from an event stream and `SaveAsync` appends new events.

### Read-side repository

```csharp
using Aiel.Results;
using Aiel.Application.Specifications;

namespace Aiel.Domain;

public interface IReadRepository<TReadModel>
    where TReadModel : class
{
    Task<Result<IReadOnlyList<TReadModel>>> ListAsync(
        IQuerySpecification<TReadModel> specification,
        CancellationToken ct = default);

    Task<Result<TReadModel>> GetAsync(
        IQuerySpecification<TReadModel> specification,
        CancellationToken ct = default);
}
```

Semantics:

- Read-side querying is always against read models, not aggregates.
- For state-based systems, the read model may come from the same relational store but should still be treated as a read-side type.
- For event-sourced systems, the read model is typically a projection.
- Filtering semantics live in read-side query specifications, not in fake aggregate querying over heterogeneous stores.
- Sorting and paging are separate read-request concerns, not part of the core specification abstraction.

### Why this is the right split

- Developers always know where command-side aggregate loading belongs.
- Developers always know where query-side projection loading belongs.
- Event-sourced projections become first-class citizens.
- CQRS is explicit rather than accidental.
- No fake `IQueryable` abstraction is needed.
- No persistence concerns leak into aggregate behavior.

### Unified usage model

Write side:

```csharp
var order = await repository.LoadAsync(orderId, ct);
if (order.IsFailure)
    return order.Error;

order.Value.AddLine(...);
return await repository.SaveAsync(order.Value, ct);
```

Read side:

```csharp
var orders = await readRepository.ListAsync(new OrdersByCustomerSpecification(customerId), ct);
```

Application query handlers can compose specifications, sorting, and paging without collapsing those concerns into one type:

```csharp
var specification = new OrdersByCustomerSpecification(query.CustomerId);
var sort = query.Sort ?? SortRequest.Empty;
var page = query.Page ?? PageRequest.Default;
```

The usage pattern is the same whether the write model is state-based or event-sourced.

### Naming convention

Typical naming should remain boring and obvious:

```csharp
IOrderRepository : IAggregateRepository<Order, OrderId>
IOrderReadRepository : IReadRepository<OrderReadModel>
```

That provides:

- one naming convention
- one usage pattern
- one mental model
