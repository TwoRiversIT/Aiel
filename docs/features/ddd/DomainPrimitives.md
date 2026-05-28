# Domain Primitives Contract

These documents are the source-of-truth design note for the first set of framework primitives.

## Purpose

- Define the baseline domain contracts that belong to Aiel rather than Aviendha.
- Keep the model explicit, strongly typed, and ORM-friendly without runtime magic.
- Give Aviendha a stable contract surface for aggregates, repositories, auditing, and domain events.
- Make early domain-shaping decisions now when they reduce long-term ambiguity and technical debt.

## Placement

- Namespace: `Aiel.Domain`
- Target package: `Aiel.Domain`
- Strong ID runtime contracts are now owned by `Aiel.StrongIds` in the `Aiel.StrongIds` package.
- Domain primitives depend on `Aiel.StrongIds` for typed identifiers, but `Entity<TKey>`, `AggregateRoot<TKey>`, and `IRepository<TAggregate, TId>` remain in `Aiel.Domain`.

## Design Rules

- Strongly typed identifiers are first-class domain concepts, not wrappers added as an afterthought.
- Domain base types must not rely on Castle DynamicProxy, IL weaving, lazy-loading proxies, or hidden runtime interception.
- EF Core compatibility is achieved through normal constructors, protected setters or init accessors, and explicit value converters where needed.
- Equality must be predictable and visible in code.
- Repository contracts must not leak `IQueryable` into consuming application code.
- Persistence commits belong to `IUnitOfWork`, not repository methods.
- Framework primitives should guide consumers toward maintainable code by making good patterns straightforward and weak patterns difficult.
- When there is tension between a low-impact shortcut and a cleaner long-term contract, prefer the cleaner contract.

## Domain Primitives

- [Strong ID](DomainPrimitives-StrongID.md)
- [Entity](DomainPrimitives-Entity.md)
- [AggregateRoot](DomainPrimitives-AggregateRoot.md)
- [ValueObject](DomainPrimitives-ValueObject.md)
- [DomainEvent](DomainPrimitives-DomainEvent.md)

## Execution Context And Message Contexts

Aiel should treat correlation and client-instance IDs as first-class cross-cutting metadata, but that metadata should live in execution context and message contexts rather than inside every message payload.

Recommended context contract:

```csharp
namespace Aiel.Domain.Execution;

public interface IExecutionContext
{
    Guid OperationId { get; }

    Guid CorrelationId { get; }

    Guid? CausationId { get; }

    Guid? ClientInstanceId { get; }
}
```

Recommended default implementation:

```csharp
namespace Aiel.Domain.Execution;

public sealed record DefaultExecutionContext : IExecutionContext
{
    public Guid OperationId { get; }
    public Guid CorrelationId { get; }
    public Guid? CausationId { get; }
    public Guid? ClientInstanceId { get; }

    public static DefaultExecutionContext CreateRoot(
        Guid? correlationId = null,
        Guid? clientInstanceId = null);

    public static DefaultExecutionContext CreateChild(IExecutionContext parent);
}
```

Recommended message-context shape:

```csharp
using Aiel.Domain.Execution;

namespace Aiel.Domain.Messaging;

public sealed record MessageContext<TMessage>(TMessage Message, IExecutionContext Context)
    where TMessage : notnull;
```

Initial rules:

- Every command or query executes inside an `IExecutionContext`.
- `OperationId` uniquely identifies the current execution scope.
- `CorrelationId` stays stable across the full end-to-end flow.
- `CausationId` identifies the immediate parent operation that directly caused the current execution scope.
- `DefaultExecutionContext.CreateRoot()` creates a new root scope with a new `OperationId`, `CausationId = null`, and `CorrelationId = OperationId` unless an external correlation ID is supplied.
- `DefaultExecutionContext.CreateChild(parent)` creates a new child scope with a new `OperationId`, the same `CorrelationId` as the parent, and `CausationId = parent.OperationId`.
- Integration events, message contexts, outbox records, and structured log scopes should carry the same `CorrelationId`.
- Client-facing applications should also provide a `ClientInstanceId` when possible.
- `ClientInstanceId` is broader than `CorrelationId`: one client instance can produce many correlated operations.

Example flow:

- An HTTP request starts with `OperationId = A`, `CorrelationId = A`, and `CausationId = null`.
- That request publishes an integration event with a new `OperationId = B`, the same `CorrelationId = A`, and `CausationId = A`.
- A downstream consumer publishes another message with `OperationId = C`, the same `CorrelationId = A`, and `CausationId = B`.

That is the difference between correlation and causation:

- correlation answers "which whole flow is this part of?"
- causation answers "what directly triggered this specific step?"

Intended operational use:

- Search Seq or another structured log store by `CorrelationId` to see all activity for one use case execution.
- Search by `CausationId` to walk one step backward or forward through a distributed chain.
- Search by `OperationId` when you need the exact log scope for one handler or one request.
- Search by `ClientInstanceId` to see a wider client session and then narrow to individual correlation scopes.

Design notes:

- These identifiers are technical tracing metadata, not business data.
- They should not be used to drive domain decisions or business invariants.
- They belong in execution contexts, message contexts, logging scopes, outbox records, and integration boundaries.
- Domain events, commands, and queries should remain focused on business intent and business facts.
- `Result<T>` is already an outcome envelope and should remain focused on success/failure rather than tracing metadata.
- The first implementation may use `Guid` and `Guid?` directly for execution-context contracts.
- Aiel should prefer explicit named metadata like `OperationId`, `CorrelationId`, and `CausationId` over a catch-all dictionary in the core abstraction.
- Transport-specific headers or broker metadata should live in bus-specific envelopes or adapters rather than in the core domain/application contract.
- The interface is named `IExecutionContext` rather than `ExecutionContext` to avoid collision with the BCL `System.Threading.ExecutionContext` type while keeping the concept familiar.
- `DefaultExecutionContext` is the default concrete implementation because it is explicit, immutable, and usable without introducing a factory abstraction too early.

Why `MessageContext<TMessage>` instead of `MessageEnvelope<TMessage>`:

- Most message transports already have their own envelope abstraction.
- Those transport envelopes often include headers, delivery metadata, retry metadata, partition keys, and broker-specific concerns.
- Aiel's wrapper is narrower: it pairs a business message with the execution context that should travel with it.
- Naming it `MessageContext<TMessage>` reduces collision with transport terminology and keeps the abstraction focused.

## Command And Query Execution

These contracts belong in the application layer rather than the domain layer, but they should follow the same execution-context model from the start.

```csharp
using Aiel.Domain.Execution;
using Aiel.Results;

namespace Aiel.Application.Commands;

public interface ICommand;

public interface ICommand<TResult>;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result> HandleAsync(
        TCommand command,
        IExecutionContext context,
        CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<Result<TResult>> HandleAsync(
        TCommand command,
        IExecutionContext context,
        CancellationToken cancellationToken = default);
}

public interface ICommandDispatcher
{
    Task<Result> DispatchAsync<TCommand>(
        TCommand command,
        IExecutionContext context,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    Task<Result<TResult>> DispatchAsync<TCommand, TResult>(
        TCommand command,
        IExecutionContext context,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;
}
```

```csharp
using Aiel.Domain.Execution;
using Aiel.Results;

namespace Aiel.Application.Queries;

public interface IQuery<TResult>;

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(
        TQuery query,
        IExecutionContext context,
        CancellationToken cancellationToken = default);
}

public interface IQueryDispatcher
{
    Task<Result<TResult>> DispatchAsync<TQuery, TResult>(
        TQuery query,
        IExecutionContext context,
        CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
}
```

Semantics:

- Dispatchers and handlers should accept `IExecutionContext` explicitly rather than relying on ambient static state.
- Root entry points create the first execution context for the flow.
- Dispatchers preserve `CorrelationId` and set a new `OperationId` for each new dispatched message or externally visible boundary.
- When one message directly causes another, the child execution context sets `CausationId` to the parent `OperationId`.
- In-process handler pipelines may reuse the same context for a single logical operation when no new boundary is crossed.
- Application query messages use `Aiel.Application.Queries.IQuery<TResult>`.
- Domain/business-rule composition uses `Aiel.Application.Specifications.ISpecification<T>`.
- Read-side repository filtering uses `Aiel.Application.Specifications.IQuerySpecification<TReadModel>`.
- Paging uses a dedicated `PageRequest` and paged responses should return a `PagedResult<T>`.
- Sorting uses a dedicated `SortRequest` made up of one or more `SortField` values.
- Free-form search text should remain an application query concern or an infrastructure concern, not a universal core abstraction.

Recommended minimal read-side shaping types:

```csharp
namespace Aiel.Application.Queries;

public sealed record PageRequest(Int32 Number, Int32 Size)
{
    public Int32 Offset => (Number - 1) * Size;
}

public enum SortDirection
{
    Ascending,
    Descending
}

public sealed record SortField(String Name, SortDirection Direction = SortDirection.Ascending);

public sealed record SortRequest(IReadOnlyList<SortField> Fields);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    Int32 PageNumber,
    Int32 PageSize,
    Int32 TotalCount);
```

Design notes:

- `ISpecification<T>` remains the pure business-rule abstraction.
- `IQuerySpecification<TReadModel>` is the read-side abstraction for provider-translatable filtering.
- `PageRequest` and `SortRequest` live at the application boundary, where transport input is interpreted and normalized.
- HTTP query strings like `pageNumber`, `pageSize`, and `sortBy` should be translated into these types at the presentation boundary.
- EF Core integration may translate `SortRequest` and `PageRequest` into `OrderBy`, `Skip`, and `Take` internally, but those `IQueryable` details should not become the public abstraction.
- Reusable specification contracts now belong in `Aiel.Application.Specifications`; the old `Aiel.Specifications` package has been retired.
- Offset pagination is the default initial model because it supports typical page-number navigation cleanly.
- Keyset pagination is still valuable, but it should be added later as a distinct model rather than being forced into the initial abstraction.

## Repository

### `IAggregateRepository<TAggregate, TKey>`

```csharp
using Aiel.Results;

namespace Aiel.Domain;

public interface IAggregateRepository<TAggregate, TKey>
    where TAggregate : AggregateRoot<TKey>
    where TKey : notnull, IStrongId
{
    Task<Result<TAggregate>> LoadAsync(TKey id, CancellationToken cancellationToken = default);

    Task<Result> SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}
```

Semantics:

- Repository methods return `Result` or `Result<T>` to align with Aiel's no-`null` public API rule.
- `LoadAsync` returns an explicit not-found failure rather than `null`.
- `SaveAsync` persists aggregate changes using the appropriate mechanism for the aggregate style.
- State-based and event-sourced aggregates share the same write-side repository abstraction.

Dependency note:

- This contract intentionally does not include query operations. Aggregate querying belongs to explicit read-side repositories.

### `IReadRepository<TReadModel>`

```csharp
using Aiel.Results;
using Aiel.Application.Specifications;

namespace Aiel.Domain;

public interface IReadRepository<TReadModel>
    where TReadModel : class
{
    Task<Result<IReadOnlyList<TReadModel>>> ListAsync(
        IQuerySpecification<TReadModel> specification,
        CancellationToken cancellationToken = default);

    Task<Result<TReadModel>> GetAsync(
        IQuerySpecification<TReadModel> specification,
        CancellationToken cancellationToken = default);
}
```

Semantics:

- Read-side repositories query read models, not aggregates.
- State-based systems may project from the same database used by the write model, but the query contract remains read-side.
- Event-sourced systems query projections or other read models.
- `IQuerySpecification<TReadModel>` provides the actual filtering semantics for read-model queries, while paging and sorting remain separate concerns.

### `IAuditedEntity`

```csharp
namespace Aiel.Domain;

public interface IAuditedEntity
{
    DateTimeOffset CreatedAt { get; }
    string CreatedBy { get; }
    DateTimeOffset? LastModifiedAt { get; }
    string? LastModifiedBy { get; }
}
```

Semantics:

- Creation metadata is required once the entity is first persisted.
- Last-modified metadata is optional until the first update occurs.
- Audit metadata is set by infrastructure based on `IAuditContext`, not by controllers or UI code.

### `IAuditContext`

```csharp
namespace Aiel.Domain;

public interface IAuditContext
{
    DateTimeOffset UtcNow { get; }
    string CurrentActor { get; }
}
```

Semantics:

- `UtcNow` is the authoritative timestamp for audit stamping within a unit of work.
- `CurrentActor` is the stable identifier for the current user, service principal, or system account.
- Infrastructure may adapt richer runtime context into this narrow contract.

## Multi-Tenancy Contracts

Placement:

- Namespace: `Aiel.MultiTenancy`
- Target package: `Aiel.MultiTenancy`
- EF integration hook point: `Aiel.EntityFrameworkCore.AielDbContext`

**See also:**
- [`multitenancy-system.md`](../multitenancy/multitenancy-system.md) — framework scope and extension points
- [`Aiel.MultiTenancy` README](../../src/Aiel.MultiTenancy/README.md) — API reference
- [`Aiel.AspNetCore` README](../../src/Aiel.AspNetCore/README.md) — HTTP pipeline usage
- [`Aiel.EntityFrameworkCore` README](../../src/Aiel.EntityFrameworkCore/README.md) — data access patterns

### `IMultiTenant`

```csharp
namespace Aiel.MultiTenancy;

public interface IMultiTenant
{
    Guid TenantId { get; set; }
}
```

Semantics:

- `IMultiTenant` marks persisted entities that must always be scoped to a tenant.
- `TenantId` is infrastructure-facing and intentionally scalar so middleware, EF filters, migrations, and HTTP boundaries can share the same contract.
- New tenant-scoped entities may be created with `Guid.Empty`; `AielDbContext.SaveChangesAsync` stamps the current tenant when a tenant context is available.
- Existing entities must keep their assigned tenant ID stable for the lifetime of the record.

### `TenantContext`

```csharp
namespace Aiel.MultiTenancy;

public sealed record TenantContext(Guid TenantId, string? DomainName = null);
```

Semantics:

- `TenantContext` is the resolved, scoped tenant identity and is **framework-owned**.
- `TenantId` is the authoritative identifier used for query filters and foreign-key stamping.
- `DomainName` is the human-facing routing key when the request was resolved by domain or host name.
- `TenantContext` intentionally carries **only tenant identity**, not storage-topology decisions, connection strings, or actor metadata.
- **Ownership seam:** Aiel owns tenant identity resolution and fail-closed enforcement. Aviendha owns storage binding, connection-string resolution, and actor-context enrichment.
- Storage-specific partitioning details such as schema names, database names, collection names, stream categories, or shard keys belong in provider-specific infrastructure packages or application-owned runtime contracts.

### `ITenantProvider`

```csharp
namespace Aiel.MultiTenancy;

public interface ITenantProvider
{
    ValueTask<TenantResolutionResult> GetCurrentTenantAsync(CancellationToken cancellationToken = default);
}

public abstract record TenantResolutionResult
{
    public sealed record Resolved(TenantContext TenantContext) : TenantResolutionResult;
    public sealed record None : TenantResolutionResult;
    public sealed record Error(string Reason) : TenantResolutionResult;
}
```

Semantics:

- `ITenantProvider` resolves the current tenant outcome for the active execution scope.
- `Resolved` carries the materialized `TenantContext`.
- `None` means the current operation is running outside a tenant scope or the authenticated actor does not have a valid active tenant for the route.
- `Error` means tenant resolution could not complete because the control plane, configuration, or conflict checks failed.
- Providers should cache the resolved outcome per request, message, or worker scope so EF query filters and save operations do not repeat lookups.

### Request Resolution Chain

The canonical tenant resolution chain is:

1. Authenticated principal claim `sub` (OIDC standard; Kratos populates this)
2. Aviendha resolves `ActorContext`
3. Aviendha validates exactly one active tenant for the request
4. Aiel materializes `TenantContext`
5. Aviendha resolves `TenantStoreBinding`

Supporting constants live in `TenantResolutionConstants`.

**Trust and hint semantics:**

- Host or subdomain can narrow lookup or brand the experience, but it is only a routing hint and never authorizes a tenant on its own.
- Tenant headers (`X-Tenant-ID`) are NOT public-edge claims; they are reserved for internal overrides on privileged endpoints only.
- No tenant claim should be trusted from an unauthenticated or untrusted source.
- `X-Tenant-ID` must be validated against the actor-resolved tenant unless the endpoint is an explicit operator-only override route.
- Conflict detection: If `X-Tenant-ID` conflicts with the actor-resolved tenant, the request MUST return 400 Bad Request.

Middleware registration pattern:

- Resolve once near the start of the HTTP pipeline, after authentication has populated the principal and before application handlers execute.
- Resolve actor context and active tenant before materializing `TenantContext`.
- Expose the explicit tenant-resolution outcome through a scoped `ITenantProvider` implementation.
- Fail closed when a tenant-scoped endpoint requires a tenant and the outcome is `None` or `Error`.

### EF Core Strategy For `IMultiTenant`

- `AielDbContext` applies global query filters to all mapped entity types implementing `IMultiTenant`.
- The filter uses the current DbContext instance's tenant state rather than repository-level caller code.
- `AielDbContext.SaveChangesAsync` stamps `TenantId` on newly added `IMultiTenant` entities when the current tenant is known and the entity still has `Guid.Empty`.
- The DbContext should establish its `TenantContext` before executing tenant-scoped queries. The base class supports this either by constructor injection of `TenantContext` or by resolving through `ITenantProvider` before save operations when the outcome is `Resolved`.

### Out-Of-The-Box Relational Partitioning

- Aiel should support two relational tenant-partitioning strategies out of the box.
- **Strategy 1 (Discriminator):** Shared database with `IMultiTenant` columns enforced through EF Core global query filters and save-time tenant stamping.
- **Strategy 2 (Database-Per-Tenant):** Tenant-specific database, where application-layer infrastructure resolves the connection string, endpoint, or database name before the unit of work starts.
- These strategies may coexist in one system. Tenant-owned write data may use database-per-tenant while shared administrative or reference data still uses `IMultiTenant` query filters.
- The core multi-tenancy package (Aiel.MultiTenancy) remains responsible only for **tenant identity and tenant resolution**, not for choosing one persistence partitioning strategy or for storing connection metadata.
- **Storage binding is an application concern.** Aviendha owns the decision to map tenants to specific database endpoints, schema names, or data-store locations. Aiel provides only identity.

### Schema-Per-Tenant Mechanics

- Schema-per-tenant is an optional relational refinement, not a core multi-tenancy contract.
- **Application-owned infrastructure** (not Aiel) should define storage-partition resolvers when needed.
- Example: A PostgreSQL integration in Aviendha may implement schema-per-tenant by resolving schema names from a runtime catalog and calling `modelBuilder.HasDefaultSchema(...)` when the model is intentionally schema-scoped.
- Example: A document-database integration may resolve database, collection, or partition-key names from `TenantContext` or from a separate `TenantStoreBinding` contract.
- Example: An event-store integration may resolve stream prefixes, categories, or tenant-specific stores from tenant identity without involving relational schemas.
- **Critical:** Because EF Core caches models by DbContext type, any provider that varies model shape by tenant storage partition must use a partition-aware model cache key.
- Shared reference data versus tenant-owned data remains an **application-specific storage-layout decision**, not a framework contract.
- **Data ownership split (framework definition, application implementation):**
  - **Platform/Catalog Database:** Tenants, domains, memberships, storage bindings, encryption keys, subscription billing, feature flags, operator audit.
  - **Per-Tenant Databases:** Clients, clinical data, appointments, session notes, tenant-local invoices, tenant-owned audit trail.
- Operational migration guidance for discriminator, schema-per-tenant, and database-per-tenant deployments lives in [phase-03-aiel-aspnet-operational-plan.md](phases/phase-03-aiel-aspnet-operational-plan.md).

## Unit Of Work Contract

Placement:

- Namespace: `Aiel.Commands`
- Contract package: `Aiel.Application`
- Default implementation package: `Aiel.EntityFrameworkCore`

### `IUnitOfWork`

```csharp
namespace Aiel.Commands;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

Semantics:

- One command handler execution maps to one unit-of-work scope by default.
- Repositories do not commit; they attach or mutate aggregates and leave persistence boundaries to the unit of work.
- EF-backed implementations should use `DbContext.SaveChangesAsync` as the commit boundary.

### Aggregate Tracking And Outbox Flow

- EF-backed repositories do not manually register tracked aggregates in a separate registry.
- `AielDbContext` treats the EF `ChangeTracker` as the source of truth for tracked aggregates by reading entities that implement `IAggregateRoot`.
- Domain events are collected from aggregate `DomainEvents` before the underlying `DbContext.SaveChangesAsync` call.
- The outbox implementation point is `AielDbContext.PersistDomainEventsAsync(...)`; concrete infrastructure should translate domain events into outbox rows inside the same DbContext and transaction.
- Aggregate domain events are cleared only after `DbContext.SaveChangesAsync` completes successfully.
- If save fails, domain events remain on the aggregate so the caller can decide whether to retry, inspect, or discard the unit of work.

## Implementation Notes

- `Entity<TKey>` and `AggregateRoot<TKey>` should support protected parameterless constructors for EF Core materialization.
- `StateBasedAggregateRoot<TKey>` exists as an intentional, visible reminder that state-based and event-sourced aggregates are distinct choices and should be selected deliberately.
- `Entity<TKey>` intentionally depends only on a strong-ID marker interface, not on the strong ID's underlying primitive type.
- Strong ID value converters should live in EF Core integration packages, not in domain primitives.
- Domain events should be serialized from the aggregate's `DomainEvents` collection into the outbox before `ClearDomainEvents()` is called.
- `IAggregateRoot` exists as a non-generic inspection contract so infrastructure can discover tracked aggregates without knowing their strong-ID type.
- A typed domain event base record may be added later for convenience, but `IDomainEvent` remains the required contract.
- Operation, correlation, causation, and client-instance IDs should be propagated through execution contexts, message contexts, integration events, outbox records, and structured logging scopes.
- Read-side filtering should converge on `IQuerySpecification<TReadModel>`, while paging and sorting remain separate request-shaping concerns.

## Related Documents

- [AggregateRootDiscussion.md](./AggregateRootDiscussion.md)
- [Architecture.md](../../ArchitectureOverview.md)
- [Framework.md](../../Framework.md)
