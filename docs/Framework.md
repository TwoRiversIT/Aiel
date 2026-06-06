# Aiel

## Design Notes

- [Conceptual Overview](./ConceptualOverview.md)
- [Architectural Overview](./ArchitectureOverview.md)
- [Domain Primitives](./features/ddd/DomainPrimitives.md)

## Dependencies

The Aiel dependency system is a structured, compile-time-verified mechanism for organising how assemblies participate in application startup. It replaces ad-hoc `Program.cs` wiring with a graph of typed modules that configure DI services and run post-build initialisation in declared order.

There are two complementary execution paths:

| Path | How graph is built | When to use |
|---|---|---|
| **Source-generated** (preferred) | `DependencyGraphSourceGenerator` emits `AielDependencyGraph.g.cs` at compile time | New projects; gets compile-time safety and zero reflection at startup |
| **Reflection-based** (fallback) | `DependencyDiscoveryExtensions.BuildDependencyTree<T>()` walks `[DependsOn]` at runtime | Legacy code or projects that cannot use the generator |

Both paths respect the same dependency ordering: a dependency is always configured **before** the types that depend on it.

---

### Module Requirement (Critical for all Aiel Assemblies)

Every assembly in the Aiel codebase — **including test assemblies** — that is not an analyzer or source generator MUST declare **exactly one** public, `sealed` class with a public parameterless constructor that inherits from `AielDependencyConfigurator` (or `AielApplication` for the host). This class must include `[DependsOn(...)]` attributes that establish a direct or transitive path back to `AielFramework`.

This requirement ensures:
- **Compile-time verifiability** of the module graph
- **Deterministic startup ordering** across the entire codebase
- **Explicit clarity** on what each assembly depends on and provides
- **Future maintainability** when modules are added or moved

The `AssemblyAnalyzer` enforces this rule at compile time and reports diagnostic `AIEL00001` (error) if the constraint is violated.

### Declarative Dependencies

Every participating assembly MUST declare **exactly one** public, `sealed` class with a public parameterless constructor that inherits from either `AielDependencyConfigurator` (libraries) or `AielApplication` (the host application).

#### `AielDependencyConfigurator`

`AielDependencyConfigurator` is the abstract base class for every module in the dependency graph. Subclass it once per assembly to register services and declare upstream dependencies.

```csharp
// MyLibrary/MyLibraryDependency.cs
using Aiel.Dependencies;

[DependsOn(typeof(AielFramework))]   // declare upstream dependencies here
public sealed class MyLibraryDependency : AielDependencyConfigurator
{
    public override Task ConfigureAsync(
        DependencyConfigurationContext context,
        CancellationToken cancellationToken = default)
    {
        context.Services.AddScoped<IMyService, MyService>();
        return Task.CompletedTask;
    }
}
```

`AielDependencyConfigurator` implements `IDependencyConfigurator`, which defines two methods: `PreConfigureAsync` (phase 1) and `ConfigureAsync` (phase 2). See [Configuration](#configuration) below for the two-phase lifecycle.

#### `AielApplication`

`AielApplication` extends `AielDependencyConfigurator` and represents the **composition root** — the topmost node of the dependency graph. Define exactly one per application project.

```csharp
// MyApp/MyApplication.cs
using Aiel.Dependencies;

[DependsOn(typeof(MyLibraryDependency))]
[DependsOn(typeof(AielFramework))]
public sealed class MyApplication : AielApplication
{
    public override String ApplicationName    => ThisAssembly.AssemblyName;
    public override String ApplicationVersion => ThisAssembly.AssemblyInformationalVersion;

    public override Task ConfigureAsync(
        DependencyConfigurationContext context,
        CancellationToken cancellationToken = default)
    {
        // Application-level service registration
        context.Services.AddSingleton<IMyAppService, MyAppService>();
        return Task.CompletedTask;
    }
}
```

`ApplicationName` and `ApplicationVersion` are consumed by `AielEnvironment`, which is registered as a singleton and available throughout the application.

#### `[DependsOn]`

`DependsOnAttribute` is a repeatable class-level attribute that declares that one `AielDependencyConfigurator` depends on another. The graph engine (both paths) reads these attributes to determine configuration order.

```csharp
[DependsOn(typeof(DependencyA))]
[DependsOn(typeof(DependencyB))]
public sealed class MyDependency : AielDependencyConfigurator { ... }
```

Rules:
- The target type MUST be a concrete, public `AielDependencyConfigurator` subclass with a public parameterless constructor.
- Circular dependencies are detected at startup and throw `CircularDependencyException`.
- A type that appears in multiple branches of the graph is configured **once** (first encounter in reflection traversal order is reused; the generated graph also deduplicates).
- Repeated `[DependsOn]` attributes that reference the same dependency type are deduplicated for execution.

#### `AielFramework`

The Aiel library itself is registered as a dependency via `AielFramework : AielDependencyConfigurator`. Applications that use any Aiel feature should transitively depend on it (most built-in `AielDependencyConfigurator` types already do).

#### Tooling

##### Source Generator — `DependencyGraphSourceGenerator`

`Aiel.Generators` ships an incremental Roslyn source generator. When the project contains a concrete `AielApplication` subclass the generator:

1. Walks the `[DependsOn]` graph transitively across all referenced assemblies.
2. Emits `AielDependencyGraph.g.cs` into the `Microsoft.Extensions.DependencyInjection` namespace.
3. Emits an `AddApplicationAsync()` extension method on the appropriate builder type (detected from project references):

   | Project type | Builder type |
   |---|---|
   | Worker / Generic Host | `HostApplicationBuilder` |
   | ASP.NET Core | `WebApplicationBuilder` |
   | Blazor WebAssembly | `WebAssemblyHostBuilder` |

The generated file contains a `AielDependencyGraph.Dependencies` property (`IReadOnlyCollection<DependencyDescriptor>`) and the `AddApplicationAsync()` extension. The extension calls `builder.RegisterDependenciesAsync(AielDependencyGraph.Dependencies)`, which creates a `DependencyManager`, registers it as `IDependencyManager`, and runs all configurators.

Each generated `DependencyDescriptor` includes the dependency type itself in its `Configurators` list (since every `AielDependencyConfigurator` subclass implements `IDependencyConfigurator`). If the type also implements `IDependencyInitializer`, it is included in the `Initializers` list as well.

The generator requires that the `AielApplication` subclass be `sealed` and non-abstract; open types are ignored.

##### Analyzer — `AssemblyAnalyzer` (`AIEL00001`)

`Aiel.Analyzers` ships a Roslyn diagnostic analyzer. It reports `AIEL00001` (error) when a compilation that references Aiel contains zero or more than one public, sealed, parameterless-constructor `AielDependencyConfigurator` or `AielApplication` subclass. Add it as an analyzer-only project reference:

```xml
<ProjectReference Include="..\path\to\Aiel.Analyzers.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

---

### Configuration

Configuration is the first phase of application startup. Every dependency in the graph gets to register services, configure options, and modify `IConfiguration` before the DI container is built.

#### Lifecycle

Configuration runs in two sequential phases. Every module's `PreConfigureAsync` completes across the entire graph before any module's `ConfigureAsync` begins. Both phases execute in topological order — deepest dependencies first.

```
builder.AddApplicationAsync()            ← source-generated path
  │
  └─► DependencyManager.ConfigureAsync()
        │
        ├─► Phase 1 — PreConfigureAsync (all modules, topological order)
        │     ├─► Dependency D  ← pre-configured first
        │     ├─► Dependency B
        │     ├─► Dependency C
        │     └─► Application A ← pre-configured last
        │
        └─► Phase 2 — ConfigureAsync (all modules, topological order)
              ├─► Dependency D  ← configured first
              ├─► Dependency B
              ├─► Dependency C
              └─► Application A ← configured last
```

The ordering within each phase is a topological sort (post-order DFS): every dependency runs **before** the types that depend on it. Within a depth tier, order is not guaranteed.

#### `DependencyConfigurationContext`

Passed to every `PreConfigureAsync` and `ConfigureAsync` call. Inherits from `DependencyContext`.

| Member | Type | Notes |
|---|---|---|
| `Services` | `IServiceCollection` | Wrapped as `ObservableServiceCollection` |
| `Configuration` | `IConfiguration` | The application configuration |
| `Environment` | `AielEnvironment` | App name, version, environment name, instance GUID |
| `IsDevelopment` | `bool` | `Environment.EnvironmentName == "Development"` |
| `IsProduction` | `bool` | |
| `IsStaging` | `bool` | |
| `IsTesting` | `bool` | |
| `IsEnvironment(name)` | `bool` | Case-insensitive compare |

Extension method on the context (from `AielConfigurationExtensions`):

```csharp
string connStr = context.GetConnectionStringOrDefault("MyDatabase");
// Falls back to "Default" then "DefaultConnection" before throwing.
```

#### `PreConfigureAsync`

`AielDependencyConfigurator` exposes a `PreConfigureAsync` virtual method as part of the `IDependencyConfigurator` contract. It is called during **Phase 1** — a complete pass over the entire dependency graph that finishes before **Phase 2** (`ConfigureAsync`) begins for any module.

Use `PreConfigureAsync` to establish shared state that other modules will read during their own `ConfigureAsync`: registering options builders, setting global switches, configuring integration extension points, etc.

```csharp
public sealed class MyLibraryDependency : AielDependencyConfigurator
{
    public override Task PreConfigureAsync(
        DependencyConfigurationContext context,
        CancellationToken cancellationToken = default)
    {
        // Register an options builder that other modules can populate
        // during their ConfigureAsync phase.
        context.Services.AddOptions<MyOptions>();
        return Task.CompletedTask;
    }

    public override Task ConfigureAsync(
        DependencyConfigurationContext context,
        CancellationToken cancellationToken = default)
    {
        // By the time this runs, every module's PreConfigureAsync has
        // already completed — options registered above are fully populated.
        context.Services.AddScoped<IMyService, MyService>();
        return Task.CompletedTask;
    }
}
```

#### `ObservableServiceCollection`

`DependencyConfigurationContext.Services` is backed by an `ObservableServiceCollection`. This allows a dependency to register a callback that is invoked each time a `ServiceDescriptor` is added — useful for cross-cutting concerns like automatic decorator registration:

```csharp
context.Services.OnAdding(descriptor =>
{
    // Inspect or react to every service registered after this point
});
```

`OnAdding` requires an `ObservableServiceCollection` and throws `InvalidOperationException` if the underlying collection is a plain `IServiceCollection`.

#### `IDependencyConfigurator`

`AielDependencyConfigurator` implements this interface, exposing both `PreConfigureAsync` and `ConfigureAsync`. You can also provide **separate configurator classes** and reference them via `DependencyDescriptor.Configurators` when building descriptors manually. The `DependencyManager` instantiates each configurator type (via `Activator.CreateInstance`), calls both phase methods in order, then disposes the instance. Configurators are called in the same topological order as the graph.

---

### Initialization

Initialisation is the second phase of application startup and runs **after** the DI container is built. Use it for work that requires resolved services — database migrations, cache warm-up, seeding, connectivity checks, etc.

#### Lifecycle

```
host.InitializeApplicationAsync()
  │
  ├─► Uses IDependencyManager if registered  (source-generated path)
  │     └─► DependencyManager.InitializeAsync()
  │           ├─► Dependency D  ← initialized first
  │           ├─► Dependency B
  │           ├─► Dependency C
  │           └─► Application A ← initialized last
  │
  └─► Falls back to walking DependencyRoot    (reflection path)
        └─► Post-order DFS, calls InitializeAsync on each IDependencyInitializer
```

#### `IDependencyInitializer`

Implement this interface on a `AielDependencyConfigurator` subclass to participate in the initialisation phase.

Using a separate initializer class is supported only when `DependencyDescriptor.Initializers` is explicitly populated (manual descriptor registration). The default reflection and source-generated discovery paths do not scan for standalone `IDependencyInitializer` types.

```csharp
public sealed class MyLibraryDependency : AielDependencyConfigurator, IDependencyInitializer
{
    public async Task InitializeAsync(
        DependencyInitializationContext context,
        CancellationToken cancellationToken)
    {
        var migrator = context.ServiceProvider.GetRequiredService<IDatabaseMigrator>();
        await migrator.MigrateAsync(cancellationToken);
    }
}
```

#### `DependencyInitializationContext`

Passed to every `InitializeAsync` call. Inherits from `DependencyContext` (same environment and configuration as the configuration phase) and adds:

| Member | Type | Notes |
|---|---|---|
| `ServiceProvider` | `IServiceProvider` | The fully built DI container |
| `Logger` | `ILogger` | Logger scoped to `DependencyInitializationContext` |

#### Host integration

Call `host.InitializeApplicationAsync()` after `host.Build()` and before `host.RunAsync()`:

```csharp
var builder = Host.CreateApplicationBuilder(args);

await builder.AddApplicationAsync();     // configure phase

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.InitializeApplicationAsync(); // initialise phase

await host.RunAsync();
```

`InitializeApplicationAsync` is an extension method on `IHost` from `AielHostExtensions`.

For production systems, run environment and tenant migrations from the deploy pipeline rather than ordinary app startup. Use startup for compatibility checks, not environment-wide migration fan-out. For the multi-tenant operating model, see [phase-03-aiel-aspnet-operational-plan.md](phases/phase-03-aiel-aspnet-operational-plan.md).

---

### Startup Summary

```
AddApplicationAsync()                  Configure phase: register DI services
  └─► Phase 1, per dependency, topological order:
        PreConfigureAsync()            ← establish shared extension points
  └─► Phase 2, per dependency, topological order:
        ConfigureAsync()               ← implement service registration here

host.Build()                           DI container is built

InitializeApplicationAsync()           Initialise phase: use resolved services
  └─► per dependency, topological order:
        InitializeAsync()              ← implement post-build work here
```

Here is **end‑developer–facing documentation** for your logging approach — written as if it belongs in the Aiel developer guide. It explains the *why*, the *how*, and the *rules* clearly enough that any contributor can follow it without asking you for clarification.

It is structured, explicit, and successor‑friendly.

---

Here is **end‑developer–facing documentation** for your logging approach — written as if it belongs in the Aiel developer guide. It explains the *why*, the *how*, and the *rules* clearly enough that any contributor can follow it without asking you for clarification.

It is structured, explicit, and successor‑friendly.

---

# **Aiel Logging Guidelines**  
### *Structured, Typed, and Collision‑Free Logging for All Framework Modules*

Aiel uses a **strongly‑typed, compile‑time‑safe logging pattern** built on top of the `LoggerMessage` source generator. This pattern ensures:

- Zero event‑ID collisions across the entire framework  
- High‑performance logging with no boxing or string interpolation overhead  
- Consistent, discoverable log messages  
- Clear, predictable event identifiers visible in both structured logs and human‑readable output  

This document explains how to define event IDs, how to write logging helpers, and how to use them correctly.

---

## **1. Event IDs (`AielEventIds`)**

All log events in Aiel use a single shared enum:

```csharp
public enum AielEventIds
{
    // Pipeline (1000–1999)
    PipelineDispatching = 1000,
    PipelineSuccess     = 1001,
    PipelineFailure     = 1002,

    // Additional modules reserve their own numeric ranges...
}
```

### **Rules for event IDs**
1. **Every event ID must be unique across the entire framework.**  
2. **Each module owns a numeric range** (e.g., Pipeline = 1000–1999).  
3. **New events must be added to the enum**, never inline as raw integers.  
4. **Values must be explicitly assigned** — no implicit auto‑incrementing.  
5. **Event IDs must never change once published**, to preserve log history integrity.

### **Why an enum?**
- Provides compile‑time safety  
- Prevents accidental reuse  
- Makes event IDs self‑documenting  
- Allows default parameters in logging methods  

---

## **2. Logging Helper Methods**

Each module defines a static partial class containing its logging helpers.  
These helpers use the `LoggerMessage` attribute to generate high‑performance logging code.

Example (Pipeline module):

```csharp
internal static partial class PipelineLoggingHelper
{
    [LoggerMessage(
        EventId = (int)AielEventIds.PipelineDispatching,
        Level = LogLevel.Information,
        Message = "[{EventId}] Dispatching {InputType} [CorrelationId={CorrelationId}]")]
    internal static partial void LogDispatching(
        this ILogger logger,
        string inputType,
        Guid correlationId,
        AielEventIds eventId = AielEventIds.PipelineDispatching);

    [LoggerMessage(
        EventId = (int)AielEventIds.PipelineSuccess,
        Level = LogLevel.Information,
        Message = "[{EventId}] {InputType} dispatched successfully [CorrelationId={CorrelationId}]")]
    internal static partial void LogSuccess(
        this ILogger logger,
        string inputType,
        Guid correlationId,
        AielEventIds eventId = AielEventIds.PipelineSuccess);

    [LoggerMessage(
        EventId = (int)AielEventIds.PipelineFailure,
        Level = LogLevel.Warning,
        Message = "[{EventId}] {InputType} dispatch failed [CorrelationId={CorrelationId}]")]
    internal static partial void LogFailure(
        this ILogger logger,
        string inputType,
        Guid correlationId,
        AielEventIds eventId = AielEventIds.PipelineFailure);
}
```

### **Key characteristics of this pattern**

#### **2.1. Event ID is baked into the attribute**
`LoggerMessage` requires a compile‑time constant.  
Casting the enum value satisfies this requirement:

```csharp
EventId = (int)AielEventIds.PipelineDispatching
```

#### **2.2. Event ID is also included as a structured property**
The message template includes:

```
[{EventId}]
```

This ensures:

- Seq shows the ID in the rendered message  
- Serilog captures it as a structured property  
- Humans scanning logs see it immediately  

#### **2.3. The method includes an optional `eventId` parameter**
This is intentional:

```csharp
AielEventIds eventId = AielEventIds.PipelineDispatching
```

It allows:

- Clean call sites (no need to pass the ID manually)  
- The ability to override the ID in advanced scenarios  
- The event ID to appear as a structured property in the log payload  

---

## **3. Calling the Logging Methods**

Usage is intentionally simple:

```csharp
logger.LogDispatching("UserRegistered", correlationId);
logger.LogSuccess("UserRegistered", correlationId);
logger.LogFailure("UserRegistered", correlationId);
```

### **What developers do NOT need to do**
- No string formatting  
- No manual event ID passing  
- No worrying about collisions  
- No remembering numeric values  

The helper methods handle everything.

---

## **4. Adding New Log Events**

When adding a new log event:

### **Step 1 — Assign an event ID**
Add a new value to `AielEventIds` inside the correct module range:

```csharp
// Pipeline (1000–1999)
PipelineValidationError = 1003,
```

### **Step 2 — Add a logging helper**
Follow the established pattern:

```csharp
[LoggerMessage(
    EventId = (int)AielEventIds.PipelineValidationError,
    Level = LogLevel.Error,
    Message = "[{EventId}] Validation error: {Reason} [CorrelationId={CorrelationId}]")]
internal static partial void LogValidationError(
    this ILogger logger,
    string reason,
    Guid correlationId,
    AielEventIds eventId = AielEventIds.PipelineValidationError);
```

### **Step 3 — Use it**
```csharp
logger.LogValidationError("Missing required field", correlationId);
```

---

## **5. Design Principles Behind the Approach**

### **5.1. Deterministic**
Every event ID is stable and predictable.

### **5.2. Discoverable**
Developers can browse the enum to see all available events.

### **5.3. High‑performance**
`LoggerMessage` generates optimized IL with:

- No boxing  
- No allocations  
- No string interpolation  

### **5.4. Structured**
Every log entry includes:

- Event ID  
- Input type  
- Correlation ID  
- Severity level  
- Message template  

### **5.5. Successor‑friendly**
Future maintainers can extend the system without breaking anything.

---

# **Summary for Developers**

- Use the logging helpers — never log raw strings.  
- Add new event IDs to `AielEventIds` using the correct numeric range.  
- Follow the established pattern when creating new logging methods.  
- Do not reuse or renumber event IDs.  
- Keep messages structured and consistent.  

This approach ensures Aiel’s logging remains **predictable, high‑performance, and easy to maintain** as the framework evolves.

---

If you want, I can also produce:

- A “How to add logging to your module” quick‑start  
- Aiel‑wide event‑ID allocation table  
- A Roslyn analyzer to enforce the rules automatically

---

| Module / Area            | Range       |
| ------------------------ | ----------- |
| Consuming Applications   | 00000–09999 |
| Aiel (General)           | 10000–10999 |
| Aiel.Application         | 11000–11999 |
| Aiel.AspNetCore          | 12000–12999 |
| Aiel.Authorization       | 13000–13999 |
| Aiel.Domain              | 14000–14999 |
| Aiel.Emailing            | 15000–15999 |
| Aiel.EntityFrameworkCore | 16000–16999 |
| Aiel.Gps                 | 17000–17999 |
| Aiel.Mediator            | 18000–18999 |
| Aiel.MessageBus          | 19000–19999 |
| Aiel.MultiTenancy        | 20000–20999 |
| Aiel.MultiTenancy        | 21000–21999 |


## Execution

The execution layer is the CQRS and domain-event foundation that connects the application tier
(commands, queries) to the domain tier (aggregates, events, read models).

```
UX → Command Endpoint → Dispatcher → Pipeline → Handler →
Aggregate → Domain Events → Event Handlers → Read Models →
Query Dispatcher → Query Pipeline → Query Handler → UX
```

---

### Execution Context

Every dispatch begins with an `IExecutionContext`.  It carries the causal identity chain for a
single operation and a mutable property bag that pipeline stages and handlers can read and write.

```csharp
public interface IExecutionContext
{
    Guid  OperationId      { get; }   // unique ID for this operation
    Guid  CorrelationId    { get; }   // shared across the entire request chain
    Guid? CausationId      { get; }   // OperationId of the direct parent, if any
    Guid? ClientInstanceId { get; }   // originating client instance, if known
    IDictionary<string, object?> Properties { get; }  // mutable pipeline state
}
```

`DefaultExecutionContext` is the standard implementation.  Use the factory methods to construct one:

```csharp
// At the boundary (controller, endpoint, background worker):
var context = DefaultExecutionContext.CreateRoot();

// With an existing correlation ID (e.g. read from an inbound HTTP header):
var context = DefaultExecutionContext.CreateRoot(correlationId: inboundCorrelationId);
```

**Dispatchers create a child context per dispatch.**  The child inherits `CorrelationId` and
`ClientInstanceId` from the parent; its `CausationId` equals the parent's `OperationId`.  This
makes the full operation tree reconstructible from logs or telemetry.

---

### Command Dispatch

Commands represent write-side intent and MUST NOT return values.

```csharp
// Declare a command
public record CreateOrderCommand(CustomerId CustomerId, IReadOnlyList<OrderLine> Lines)
    : ICommand;

// Implement a handler
public sealed class CreateOrderCommandHandler(
    IRepository<Order, OrderId> repository,
    IDomainEventDispatcher eventDispatcher)
    : ICommandHandler<CreateOrderCommand>
{
    public async Task<Result> HandleAsync(
        CreateOrderCommand command,
        IExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var order = Order.Create(command.CustomerId, command.Lines);
        await repository.AddAsync(order, cancellationToken);
        await eventDispatcher.DispatchAsync(order.DomainEvents, context, cancellationToken);
        order.ClearDomainEvents();
        return Result.Success();
    }
}
```

Dispatch via `ICommandDispatcher`:

```csharp
var result = await commandDispatcher.DispatchAsync(
    new CreateOrderCommand(customerId, lines), context, cancellationToken);

if (!result.IsSuccess)
    // inspect result.Error
```

`DefaultCommandDispatcher` (registered by `AddAielCqrs`):

1. Creates a child `DefaultExecutionContext` from the caller's context.
2. Resolves `ICommandHandler<TCommand>` from DI.
3. Resolves all `ICommandPipelineBehavior<TCommand>` registrations from DI.
4. Builds the pipeline chain — first registered behavior wraps all others.
5. Executes the outermost delegate.

---

### Query Dispatch

Queries represent the read side and return a typed result.

```csharp
public record GetOrderQuery(OrderId OrderId) : IQuery<OrderSummary>;

public sealed class GetOrderQueryHandler(IReadModelRepository readModels)
    : IQueryHandler<GetOrderQuery, OrderSummary>
{
    public async Task<Result<OrderSummary>> HandleAsync(
        GetOrderQuery query,
        IExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var summary = await readModels.GetOrderSummaryAsync(query.OrderId, cancellationToken);
        return summary is null
            ? Result<OrderSummary>.Failure(new OrderNotFoundError(query.OrderId))
            : Result<OrderSummary>.Success(summary);
    }
}
```

Dispatch via `IQueryDispatcher`:

```csharp
var result = await queryDispatcher.DispatchAsync<GetOrderQuery, OrderSummary>(
    new GetOrderQuery(orderId), context, cancellationToken);

if (result.IsSuccess)
    return result.Value;
```

`DefaultQueryDispatcher` follows the same pipeline pattern as the command dispatcher.

---

### Pipeline Behaviors

Cross-cutting concerns are implemented as pipeline behaviors.  Commands and queries have **separate**
behavior interfaces; a type MUST NOT implement both.

| Interface | Delegate | Output |
|---|---|---|
| `ICommandPipelineBehavior<TCommand>` | `CommandPipelineHandlerDelegate` | `Task<Result>` |
| `IQueryPipelineBehavior<TQuery, TResult>` | `QueryPipelineHandlerDelegate<TResult>` | `Task<Result<TResult>>` |

Both delegates are closure-capturing: input and child context are captured when the chain is built.
A behavior calls `next(cancellationToken)` to pass control to the next stage.

**Execution order** matches registration order — first registered is outermost:

```
Registered: [B1, B2]

B1.HandleAsync
  └─► B2.HandleAsync
        └─► handler
      B2 after handler
  B1 after handler
```

#### Writing a Behavior

```csharp
public sealed class TimingPipelineBehavior<TCommand>(
    ILogger<TimingPipelineBehavior<TCommand>> logger)
    : ICommandPipelineBehavior<TCommand>
    where TCommand : ICommand
{
    public async Task<Result> HandleAsync(
        TCommand command,
        IExecutionContext context,
        CommandPipelineHandlerDelegate next,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = await next(cancellationToken);
        logger.LogInformation("{CommandType} completed in {Elapsed}ms",
            typeof(TCommand).Name, sw.ElapsedMilliseconds);
        return result;
    }
}
```

#### Built-in Behaviors

| Type | Covers | What it logs |
|---|---|---|
| `CommandLoggingPipelineBehavior<TCommand>` | Commands | Type name + correlation ID before; success or failure after |
| `QueryLoggingPipelineBehavior<TQuery, TResult>` | Queries | Same |

Both use the internal `PipelineLoggingHelper` for consistent structured-log formatting.

#### Registering Behaviors

Behaviors are **not** auto-registered by `AddAielCqrs`.  Register them explicitly after calling
`AddAielCqrs` so that ordering is intentional and visible:

```csharp
services.AddAielCqrs(typeof(MyHandler).Assembly);

// Open-generic registration — covers every TCommand / TQuery automatically.
services.AddTransient(
    typeof(ICommandPipelineBehavior<>),
    typeof(CommandLoggingPipelineBehavior<>));
services.AddTransient(
    typeof(IQueryPipelineBehavior<,>),
    typeof(QueryLoggingPipelineBehavior<,>));
```

---

### Domain Events

Aggregates accumulate domain events via `AggregateRoot<TId>`.  After state is persisted, dispatch
the accumulated events and clear them from the aggregate.

#### Recommended Pattern — Persist First, Then Dispatch

```csharp
var order = await repository.GetByIdAsync(orderId, cancellationToken);
order.Confirm();

await unitOfWork.SaveChangesAsync(cancellationToken);          // persist first

await eventDispatcher.DispatchAsync(
    order.DomainEvents, context, cancellationToken);           // dispatch second
order.ClearDomainEvents();
```

If dispatch fails after a successful save, events can be replayed from the persisted aggregate
state.  The Phase 03 outbox will automate this and make it transactional.

#### `IDomainEventDispatcher`

```csharp
Task DispatchAsync(IDomainEvent domainEvent, IExecutionContext context, CancellationToken ct);
Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, IExecutionContext context, CancellationToken ct);
```

`DefaultDomainEventDispatcher` resolves all registered `IDomainEventHandler<TEvent>` implementations
for the concrete event type at runtime and invokes them in registration order.

#### `IDomainEventHandler<TDomainEvent>`

```csharp
public sealed class OrderConfirmedProjectionHandler
    : IDomainEventHandler<OrderConfirmedEvent>
{
    public async Task HandleAsync(
        OrderConfirmedEvent domainEvent,
        IExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        // Update a read model, publish an integration event, etc.
        // Throw if the work cannot complete — the dispatcher propagates the exception.
    }
}
```

Handlers MUST NOT swallow failures.  A handler that cannot complete its work MUST throw, giving
the dispatch layer (outbox in Phase 03) an opportunity to retry.

---

### Repository Contract

`IRepository<TAggregate, TId>` is the write-side persistence abstraction declared in `Aiel.Domain`.
EF Core implementations are provided by `Aiel.EntityFrameworkCore` (Phase 03).

```csharp
public interface IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId        : notnull, IStrongId
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}
```

---

### Registration

Register all CQRS infrastructure and scan assemblies for handlers with a single call:

```csharp
services.AddAielCqrs(
    typeof(CreateOrderCommandHandler).Assembly,
    typeof(GetOrderQueryHandler).Assembly);
```

`AddAielCqrs` registers:

| Registration | Lifetime | Strategy |
|---|---|---|
| `ICommandDispatcher` → `DefaultCommandDispatcher` | Scoped | `TryAddScoped` |
| `IQueryDispatcher` → `DefaultQueryDispatcher` | Scoped | `TryAddScoped` |
| `IDomainEventDispatcher` → `DefaultDomainEventDispatcher` | Scoped | `TryAddScoped` |
| `ICommandHandler<TCommand>` implementations (scanned) | Scoped | `AddScoped` per closed interface |
| `IQueryHandler<TQuery, TResult>` implementations (scanned) | Scoped | `AddScoped` per closed interface |
| `IDomainEventHandler<TDomainEvent>` implementations (scanned) | Scoped | `AddScoped` per closed interface |

`TryAddScoped` ensures dispatchers are registered exactly once even if `AddAielCqrs` is called
multiple times.  Pipeline behaviors are **not** auto-registered — add them explicitly after this
call to keep ordering intentional and visible.

---

### Execution Summary

```
AddAielCqrs(assemblies)
  └─► Register ICommandDispatcher, IQueryDispatcher, IDomainEventDispatcher
  └─► Scan assemblies → register ICommandHandler, IQueryHandler, IDomainEventHandler

ICommandDispatcher.DispatchAsync(command, context, ct)
  └─► Create child IExecutionContext    (CausationId = parent.OperationId)
  └─► Resolve ICommandHandler<T>
  └─► Resolve ICommandPipelineBehavior<T> list
  └─► Build pipeline (first registered = outermost)
  └─► Execute pipeline
        └─► [behaviors wrap the handler call]
        └─► ICommandHandler<T>.HandleAsync(command, childContext, ct)
              └─► IRepository.GetByIdAsync / AddAsync
              └─► aggregate.Mutate()
              └─► unitOfWork.SaveChangesAsync()
              └─► IDomainEventDispatcher.DispatchAsync(events, context, ct)
                    └─► IDomainEventHandler<TEvent>.HandleAsync() × n (registration order)

IQueryDispatcher.DispatchAsync(query, context, ct)
  └─► Create child IExecutionContext
  └─► Resolve IQueryHandler<T, TResult>
  └─► Resolve IQueryPipelineBehavior<T, TResult> list
  └─► Build pipeline
  └─► IQueryHandler<T, TResult>.HandleAsync(query, childContext, ct)
        └─► Read-model lookup → Result<TResult>
```
