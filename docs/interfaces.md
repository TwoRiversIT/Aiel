# Aiel Public Interfaces

All `public interface` declarations in `Aiel.*` namespaces, grouped by namespace.

---

## `Aiel.Collections`

Package: `Aiel`

```csharp
public interface ITypeSet<in TBase> : ISet<Type>, IReadOnlyCollection<Type>, IReadOnlySet<Type>
    where TBase : class
{
    void Add<T>(T item) where T : TBase;
    Boolean Contains<T>(T item) where T : TBase;
    Boolean Remove<T>(T item) where T : TBase;
}
```

---

## `Aiel.Data`

Package: `Aiel`

```csharp
public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    Task<IDbConnection> CreateConnectionAsync();
}
```

---

## `Aiel.Framework`

Package: `Aiel`

```csharp
public interface IDependencyConfigurator
{
    ValueTask PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default);
    ValueTask ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default);
}

public interface IInitializer
{
    Task InitializeAsync(DependencyInitializationContext context, CancellationToken cancellationToken = default);
}

public interface IDependencyManager
{
    IReadOnlyCollection<DependencyDescriptor> Dependencies { get; }
    Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default);
    Task InitializeAsync(DependencyInitializationContext context, CancellationToken cancellationToken = default);
}
```

---

## `Aiel.UI`

Package: `Aiel`

```csharp
public interface IMarkdownRenderer
{
    String Render(String markdown);
}
```

---

## `Aiel.StrongIds`

Package: `Aiel.StrongIds`

```csharp
public interface IStrongId;

public interface IStrongId<TValue> : IStrongId
{
    TValue Value { get; }
}
```

---

## `Aiel.Actions`

Package: `Aiel.Application.Contracts`

```csharp
public interface IAction;
```

---

## `Aiel.Commands`

Packages: `Aiel.Application.Contracts`, `Aiel.Application`

```csharp
public interface ICommand : IAction;

public interface ICommandDispatcher
{
    Task<Result> DispatchAsync<TCommand>(
        TCommand command,
        IExecutionContext context,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand;
}

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result> HandleAsync(
        TCommand command,
        IExecutionContext context,
        CancellationToken cancellationToken = default);
}

public interface ICommandPipelineBehavior<TCommand>
    where TCommand : ICommand
{
    Task<Result> HandleAsync(
        TCommand command,
        IExecutionContext context,
        CommandPipelineHandlerDelegate next,
        CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

---

## `Aiel.Domain`

Packages: `Aiel.Application.Contracts`, `Aiel.Domain`

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
    String EventType { get; }
}

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, IExecutionContext context, CancellationToken cancellationToken = default);
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, IExecutionContext context, CancellationToken cancellationToken = default);
}

public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent, IExecutionContext context, CancellationToken cancellationToken = default);
}

public interface IAggregateRoot
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

public interface IAuditedEntity
{
    DateTimeOffset CreatedAt { get; set; }
    String CreatedBy { get; set; }
    DateTimeOffset ModifiedAt { get; set; }
    String ModifiedBy { get; set; }
}

public interface IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull, IStrongId
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}

public interface IAggregateRepository<TAggregate, TId> : IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull, IStrongId;
```

---

## `Aiel.EventSourcing`

Package: `Aiel.Domain`

```csharp
public interface IRehydrateFromHistory
{
    void RehydrateFromHistory(IEnumerable<IDomainEvent> history);
}
```

---

## `Aiel.Actions`

Package: `Aiel.Application.Contracts`

```csharp
public interface IActor
{
    String AuditIdentity => GetType().FullName ?? GetType().Name;
}

public interface IExecutionContext
{
    Guid OperationId { get; }
    IActor Actor { get; }
    Guid CorrelationId { get; }
    Guid? CausationId { get; }
    Guid? ClientInstanceId { get; }
    IDictionary<String, Object?> Properties { get; }
}

public interface IActionExecutionContext<out TAction> : IExecutionContext
    where TAction : IAction
{
    TAction Action { get; }
}
```

---

## `Aiel.Queries`

Package: `Aiel.Application.Contracts`

```csharp
public interface IQuery<TResult> : IAction;

public interface IQueryDispatcher
{
    Task<Result<TResult>> DispatchAsync<TQuery, TResult>(
        TQuery query,
        IExecutionContext context,
        CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
}

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(
        TQuery query,
        IExecutionContext context,
        CancellationToken cancellationToken = default);
}

public interface IQueryPipelineBehavior<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(
        TQuery query,
        IExecutionContext context,
        QueryPipelineHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default);
}
```

---

## `Aiel.Specifications`

Package: `Aiel.Application`

```csharp
public interface ISpecification<T>
{
    Boolean IsSatisfiedBy(T obj);
}

public interface IQuerySpecification<T> : ISpecification<T>
{
    Expression<Func<T, Boolean>> Predicate { get; }
}

public interface IReadRepository<TEntity> : IDisposable
    where TEntity : class
{
    IAsyncEnumerable<TEntity> FindAsync(IQuerySpecification<TEntity> specification, SortRequest? sort = null, PageRequest? page = null);
    Task<TEntity?> GetAsync(IQuerySpecification<TEntity> specification, SortRequest? sort = null, CancellationToken cancellationToken = default);
    Task<Boolean> AnyAsync(IQuerySpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<Boolean> AnyAsync(Expression<Func<TEntity, Boolean>> predicate, CancellationToken cancellationToken = default);
    Task<Int32> CountAsync(IQuerySpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<Int32> CountAsync(Expression<Func<TEntity, Boolean>> predicate, CancellationToken cancellationToken = default);
}

public interface IQuerySpecificationRepository<TEntity> : IReadRepository<TEntity>
    where TEntity : class;
```

---

## `Aiel.AspNetCore`

Package: `Aiel.AspNetCore`

```csharp
public interface ITenantResolutionFeature
{
    TenantResolution Resolution { get; }
}
```

---

## `Aiel.Authorization`

Packages: `Aiel.Authorization.Application.Contracts`, `Aiel.Authorization.Client`

```csharp
public interface IActionAuthorizationChecker<TAction>
    where TAction : IAction
{
    Task<Result> CheckPermissionAsync(IActionExecutionContext<TAction> context, CancellationToken cancellationToken = default);
}

public interface IActionCapabilityService
{
    ValueTask<Result<ActionCapabilitySnapshot>> GetSnapshotAsync(ActionCapabilityRequest request, CancellationToken cancellationToken = default);
}

public interface IActionCapabilitySnapshotCache
{
    ValueTask<Result<ActionCapabilitySnapshot>> GetSnapshotAsync(ActionCapabilityRequest request, CancellationToken cancellationToken = default);
    ValueTask<Result<ActionCapabilitySnapshot>> RefreshSnapshotAsync(ActionCapabilityRequest request, CancellationToken cancellationToken = default);
    ValueTask InvalidateAsync(ActionCapabilityRequest request);
    ValueTask<Result<ActionCapabilitySnapshot>> HandleAuthorizationFailureAsync(ActionCapabilityRequest request, Result actionResult, CancellationToken cancellationToken = default);
}

public interface IActionGate<TAction>
    where TAction : IAction
{
    Task<Result<IActionExecutionContext<TAction>>> AuthorizeAsync(
        IExecutionContext context,
        TAction action,
        CancellationToken cancellationToken = default);
}

public interface IActionValidator<TAction>
    where TAction : IAction
{
    Task<Result> ValidateAsync(IActionExecutionContext<TAction> context, CancellationToken cancellationToken = default);
}

public interface IAuthorizationDefinitionRegistry
{
    IReadOnlyList<AuthorizationDefinitionManifest> GetAll();
    Boolean TryGet(PermissionName permissionName, [NotNullWhen(true)] out AuthorizationDefinitionManifest manifest);
    Boolean TryGetForAction<TAction>([NotNullWhen(true)] out AuthorizationDefinitionManifest manifest) where TAction : IAction;
}

public interface IAuthorizationGrantEvaluator
{
    Task<Result<AuthorizationGrantDecision?>> EvaluateAsync(
        PermissionName permissionName,
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        CancellationToken cancellationToken = default);
}

public interface IAuthorizationGrantStore
{
    Task<Result<AuthorizationGrantId>> CreateGrantAsync(
        PermissionName permissionName,
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        AuthorizationGrantDecision decision,
        CancellationToken cancellationToken = default);
    Task<Result> RevokeGrantAsync(AuthorizationGrantId grantId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AuthorizationGrantSummary>>> GetGrantsForSubjectAsync(
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        CancellationToken cancellationToken = default);
}

public interface IAuthorizationManager
{
    Task<Result<AuthorizationGrantId>> GrantAsync(GrantPermissionRequest request, CancellationToken cancellationToken = default);
    Task<Result> RevokeAsync(RevokeAuthorizationRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AuthorizationGrantSummary>>> GetGrantsForSubjectAsync(
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        CancellationToken cancellationToken = default);
}

public interface IAuthorizationScopeResolver<TAction>
    where TAction : IAction
{
    Task<Result<AuthorizationScopeResolution>> ResolveAsync(
        IActionExecutionContext<TAction> context,
        CancellationToken cancellationToken = default);
}

public interface IAuthorizationSubjectResolver<TAction>
    where TAction : IAction
{
    AuthorizationSubjectKey ResolveSubjectKey(IActionExecutionContext<TAction> context);
}

public interface IResourceAuthorizationService
{
    Task<Result> AuthorizeAsync(
        IExecutionContext context,
        PermissionName permissionName,
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        CancellationToken cancellationToken = default);
}
```

---

## `Aiel.Authorization.EntityFrameworkCore`

Package: `Aiel.Authorization.EntityFrameworkCore`

```csharp
public interface IPermissionMigrationOperation;
```

---

## `Aiel.Emailing`

Package: `Aiel.Emailing`

```csharp
public interface IEmailSender
{
    Task SendEmailAsync(String email, String subject, String htmlMessage);
}

public interface IEmailValidator
{
    Boolean IsValid(String email);
    Boolean IsValid(EmailAddress emailAddress);
}

public interface IRequireEmailSender
{
    void SetSender(IEmailSender sender);
}
```

---

## `Aiel.EntityFrameworkCore.Migrations`

Package: `Aiel.EntityFrameworkCore`

```csharp
public interface IDatabaseMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken = default);
}

public interface ITargetedDatabaseMigrator
{
    Task MigrateAsync(ITenantMigrationTarget target, CancellationToken cancellationToken = default);
}

public interface ITenantMigrationTarget
{
    TenantMigrationKey Key { get; }
    TenantMigrationLabel Label { get; }
}

public interface ITenantMigrationTargetSource
{
    IAsyncEnumerable<ITenantMigrationTarget> GetTargetsAsync(CancellationToken cancellationToken = default);
}

public interface ITenantMigrationRunner
{
    Task<TenantMigrationResult> ResumeAsync(MigrationCheckpoint checkpoint, CancellationToken cancellationToken = default);
}

public interface IMigrationReadinessContributor
{
    Task<Boolean> IsReadyAsync(CancellationToken cancellationToken = default);
}

public interface IMigrationTelemetryHook
{
    void OnStarted(ITenantMigrationTarget target);
    void OnCompleted(ITenantMigrationTarget target);
    void OnFailed(ITenantMigrationTarget target, MigrationFailedTarget failure);
}
```

---

## `Aiel.Gps.HP`

Package: `Aiel.Gps.HP`

```csharp
public interface INmeaParser
{
    ReadOnlySpan<Byte> Identifier { get; }
}

public interface INmeaParser<TMessage> : INmeaParser
    where TMessage : struct
{
    void Parse(ref Lexer lexer, out TMessage message);
}

public interface ICustomNmeaParser
{
    ReadOnlySpan<Byte> Identifier { get; }
    Object Parse(ref Lexer lexer);
}
```

---

## `Aiel.Gps.Parsing`

Package: `Aiel.Gps`

```csharp
public interface ILexer
{
    Boolean EOL { get; }
    Char NextChar();
    String NextString();
    void SkipString();
    ReadOnlySequence<Byte> NextStringSlice();
    Double NextDouble();
    Int32 NextInteger();
    Int32 NextHexadecimal();
    Double NextLatitude();
    Double NextLongitude();
    DateOnly NextDate();
    TimeOnly NextTime();
    DateTime NextDateTime();
    Int32 NextChecksum();
}
```

---

## `Aiel.IdGeneration`

Package: `Aiel.IdGeneration`

```csharp
public interface IIdGenerator
{
    String NextId();
}

public interface IGuidGenerator
{
    Guid NewGuid() => Guid.NewGuid();
}

public interface IKeyGenerator
{
    String Generate(Int32 keyLength);
}
```

---

## `Aiel.Mediator`

Package: `Aiel.Mediator.Abstractions`

```csharp
public interface INotification;

public interface IActionHandler<in TAction>
    where TAction : IAction
{
    ValueTask<Result> HandleAsync(TAction action, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand> : IActionHandler<TCommand>
    where TCommand : ICommand;

public interface IQueryHandler<in TQuery, TDto> : IActionHandler<TQuery>
    where TQuery : IQuery<TDto>;

public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    ValueTask HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}

public interface ISender
{
    ValueTask<Result> ExecuteAsync(ICommand action, CancellationToken cancellationToken = default);
    ValueTask<Result<TDto>> QueryAsync<TDto>(IQuery<TDto> action, CancellationToken cancellationToken = default);
}

public interface IPublisher
{
    ValueTask PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}

public interface IPipelineBehavior<in TAction>
    where TAction : IAction
{
    ValueTask<Result> HandleAsync(TAction request, ActionHandlerDelegate next, CancellationToken cancellationToken = default);
}
```

---

## `Aiel.MessageBus`

Package: `Aiel.MessageBus.Abstractions`

```csharp
public interface IIntegrationMessage;

public interface IMessageEnvelopeFactory
{
    MessageEnvelope<TMessage> Create<TMessage>(TMessage message, IExecutionContext executionContext, TenantIdentity? tenant = null)
        where TMessage : IIntegrationMessage;
}

public interface IMessagePublisher
{
    ValueTask PublishAsync<TMessage>(MessageEnvelope<TMessage> envelope, CancellationToken cancellationToken = default)
        where TMessage : IIntegrationMessage;
    ValueTask PublishAsync<TMessage>(TMessage message, IExecutionContext executionContext, TenantIdentity? tenant = null, CancellationToken cancellationToken = default)
        where TMessage : IIntegrationMessage;
}

public interface IMessageHandler<TMessage>
    where TMessage : IIntegrationMessage
{
    ValueTask HandleAsync(InboundMessageContext<TMessage> context, CancellationToken cancellationToken = default);
}

public interface IMessageTypeRegistry
{
    MessageTypeName GetName<TMessage>() where TMessage : IIntegrationMessage;
    Type Resolve(MessageTypeName messageTypeName);
}

public interface IMessageSerializer
{
    SerializedMessage Serialize<TMessage>(MessageEnvelope<TMessage> envelope) where TMessage : IIntegrationMessage;
    MessageEnvelope<TMessage> Deserialize<TMessage>(SerializedMessage message) where TMessage : IIntegrationMessage;
}

// vNext seams — defined but not registered in v1

public interface IInboundMessageContext
{
    MessageMetadata Metadata { get; }
    IExecutionContext ExecutionContext { get; }
    TransportContext Transport { get; }
}

public interface IMessageConsumptionMiddleware
{
    ValueTask InvokeAsync(IInboundMessageContext context, Func<CancellationToken, ValueTask> next, CancellationToken cancellationToken = default);
}

public interface IOutboxWriter
{
    ValueTask WriteAsync(SerializedMessage message, CancellationToken cancellationToken = default);
}

public interface IOutboxDispatcher
{
    ValueTask DispatchPendingAsync(CancellationToken cancellationToken = default);
}
```

---

## `Aiel.MessageBus.Sagas`

Package: `Aiel.MessageBus.Sagas`

```csharp
// Lifecycle markers — applied to the saga orchestrator class

public interface IAmStartedByMessage<TMessage>
    where TMessage : IIntegrationMessage;

public interface IHandleSagaMessage<TMessage>
    where TMessage : IIntegrationMessage;

// Correlation — implemented on the orchestrator for every handled message type

public interface ICorrelateMessage<TSagaState, TMessage>
    where TSagaState : SagaState
    where TMessage : IIntegrationMessage
{
    SagaId GetSagaId(TMessage message, MessageMetadata metadata);
}

// Handler — the orchestrator's execution contract

public interface ISagaMessageHandler<TSagaState, TMessage>
    where TSagaState : SagaState
    where TMessage : IIntegrationMessage
{
    ValueTask HandleAsync(SagaHandlingContext<TSagaState, TMessage> context, CancellationToken cancellationToken = default);
}

// Persistence seam — implemented by infrastructure; no default provided

public interface ISagaRepository<TSagaState>
    where TSagaState : SagaState, new()
{
    ValueTask<TSagaState?> FindAsync(SagaId sagaId, CancellationToken cancellationToken = default);
    ValueTask SaveAsync(TSagaState state, CancellationToken cancellationToken = default);
    ValueTask DeleteAsync(SagaId sagaId, CancellationToken cancellationToken = default);
}
```

---

## `Aiel.MultiTenancy`

Package: `Aiel.MultiTenancy`

```csharp
public interface IMultiTenant
{
    TenantId TenantId { get; set; }
}

public interface ICurrentTenant
{
    TenantIdentity? Current { get; }
    IDisposable Change(TenantIdentity? tenant);
}

public interface ITenantResolver
{
    ValueTask<TenantResolution> ResolveAsync(CancellationToken cancellationToken = default);
}

public interface ITenantAccessor
{
    ValueTask<TenantIdentity> GetCurrentTenantAsync(CancellationToken cancellationToken = default);
}
```
