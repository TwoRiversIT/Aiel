# Two Rivers Aiel

![NuGet Version](https://img.shields.io/nuget/v/Aiel?link=https%3A%2F%2Fgithub.com%2Ftwotivers%2FAiel)

A comprehensive collection of NuGet packages for building modern .NET applications following Clean Architecture and Domain-Driven Design principles.

## Architecture Principles

The Aiel Application Framework follows these core patterns:

- **Clean Architecture** - Clear separation between Domain, Application, Infrastructure, and Presentation layers
- **Domain-Driven Design** - Rich domain models with value objects, entities, and aggregates
- **CQRS** - Separate read and write models where appropriate
- **Result Pattern** - Explicit error handling without exceptions for control flow
- **Specification Pattern** - Encapsulate query logic in reusable, composable specifications
- **Strong Typing** - Prefer value objects and enums over primitive obsession
- **Performance** - Optimized for high-performance scenarios (sequential GUIDs, efficient comparers)
- **Testability** - All components designed for easy testing with dependency injection

The following documents are the basis for the framework. While not required reading, they document the core philosophy and architectural goals that guide the framework implementation.

- [Conceptual Overview](./docs/ConceptualOverview.md)
- [Architecture](./docs/ArchitectureOverview.md)
- [Domain Primitives Contract](./docs/features/ddd/DomainPrimitives.md)
- [Aggregate Root Discussion](./docs/features/ddd/AggregateRootDiscussion.md)

## Framework Core

### [Aiel.Framework](./src/Aiel.Framework/README.md)

Module system and dependency graph for composing Aiel-based applications.

**Features**:

- `AielDependency` — base class for all Aiel modules; override `ConfigureAsync` to register services
- `[DependsOn]` — attribute-driven module dependency graph resolved at startup
- `DependencyManager` / `DependencyRoot` — runtime module orchestration
- `DisposableExtensions` — safe disposal helpers

### [Aiel.Utilities](./src/Aiel.Utilities/README.md)

Fundamental utilities, value objects, and extensions used across the framework.

**Features**:

- `DisposableBase` — safe `IAsyncDisposable` and `IDisposable` pattern implementation
- `NaturalComparer<T>` — human-friendly string sorting
- Extension methods for `String`, `IPAddress`, and more
- `ReflectionUtils` — extract constants and metadata via reflection
- `EnumerableComparer` — sequence comparison
- IP address comparers

## Domain & Application Layer

### [Aiel.Domain](./src/Aiel.Domain/README.md)

Domain-layer base types: aggregates, entities, repositories, and domain events.

**Features**:

- `Entity<TKey>` and `AggregateRoot<TKey>` — base classes for domain objects
- `IAggregateRepository<TAggregate, TKey>` — repository contract
- `IDomainEvent` — marker interface for domain events

### [Aiel.Application.Contracts](./src/Aiel.Application.Contracts/README.md)

Application-layer contracts for commands, queries, specifications, and read-side shaping concerns.

**Features**:

- `ISpecification<T>` — pure business-rule composition
- `IQuerySpecification<T>` — provider-translatable read-side filtering
- `ICommand`, `IQuery<TResult>` — command and query markers
- `IExecutionContext` — operation, correlation, causation, and actor metadata
- `IDomainEventDispatcher` — domain event dispatch contract
- `PageRequest`, `SortRequest`, and `PagedResult<T>` — read-side shaping

### [Aiel.Application](./src/Aiel.Application/README.md)

Application-layer implementations.

## CQRS & Mediator

### [Aiel.Mediator](./src/Aiel.Mediator/README.md)

In-process CQRS dispatcher with pipeline behavior support.

**Features**:

- `ISender` / `IPublisher` — dispatch commands, queries, and notifications
- Assembly-scanned handler registration
- `IPipelineBehavior<T>` — composable cross-cutting behaviors
- Built-in `ValidationBehavior` (FluentValidation) and `LoggingBehavior`
- Scoped execution per request

### Aiel.Actions

CQRS action primitives: `IAction`, `IExecutionContext`, `IActor`, `SystemActor`.

### [Aiel.Actions.Commands](./src/Aiel.Actions.Commands/README.md)

Command-side CQRS contracts and dispatching.

**Features**:

- `ICommand`, `ICommandHandler<T>`, `ICommandDispatcher`
- `ICommandPipelineBehavior<T>` — command-scoped pipeline
- `IUnitOfWork` + `UnitOfWorkCommandPipelineBehavior`
- Structured logging via `CommandLoggingPipelineBehavior`

### [Aiel.Actions.Queries](./src/Aiel.Actions.Queries/README.md)

Query-side CQRS contracts and dispatching.

**Features**:

- `IQuery<TResult>`, `IQueryHandler<TQuery, TResult>`, `IQueryDispatcher`
- `IQueryPipelineBehavior<T>` — query-scoped pipeline
- `PageRequest` and `PagedResult<T>` for paged reads
- Structured logging via `QueryLoggingPipelineBehavior`

### [Aiel.Actions.Queries.EntityFrameworkCore](./src/Aiel.Actions.Queries.EntityFrameworkCore/README.md)

EF Core query execution over `IQuerySpecification<T>`.

## Result Pattern

### [Aiel.Results](./src/Aiel.Results/README.md)

Result Pattern implementation for representing operation outcomes without exceptions.

**Features**:

- `Result` and `Result<T>` — type-safe operation results
- `Error` types with standard error codes (`NotFound`, `Conflict`, `Validation`, etc.)
- Functional operations: `Map`, `Bind`, `Match`, `Tap`
- Async support with `MapAsync`, `BindAsync`, `MatchAsync`
- Trimming and AOT compatible
- JSON serialization support

### [Aiel.Results.Generators](./src/Aiel.Results.Generators/README.md)

Source generators for custom error types (see [Aiel.Results](./src/Aiel.Results/README.md) for documentation).

## Strong IDs

### [Aiel.StrongIds](./src/Aiel.StrongIds/README.md)

Source-generated strongly-typed identifiers.

**Features**:

- `[StrongId<Guid>]` attribute on a `partial record struct` — generates equality, converters, and factory methods
- `IStrongId<T>` — shared interface for typed ID constraints
- Bundled source generator (`Aiel.StrongIds.Generators`)

### [Aiel.StrongIds.AspNetCore](./src/Aiel.StrongIds.AspNetCore/README.md)

ASP.NET Core binding for Strong IDs: `TypeConverter` registration and `System.Text.Json` integration.

### [Aiel.StrongIds.EntityFrameworkCore](./src/Aiel.StrongIds.EntityFrameworkCore/README.md)

EF Core value converters for Strong IDs via `HasStrongIdConversion<TStrongId, TValue>()`.

## Security & Identity

### [Aiel.Security](./src/Aiel.Security/README.md)

Claims-based authentication extensions for extracting and working with user claims.

**Features**:

- `ClaimsPrincipalExtensions` — extract user information (`FullName`, `Email`, `TimeZone`)
- `ClaimExtensions` — type-safe claim value extraction (`String`, `Int32`, `Guid`)
- `AielClaims` — standard claim type constants
- `EmailAddress` integration with claims

## Authorization

### [Aiel.Authorization](./src/Aiel.Authorization/README.md)

Full authorization stack: domain, application, client, EF Core, generators, and analyzers.

| Package | Purpose |
|---|---|
| `Aiel.Authorization.Domain` / `.Domain.Shared` | Authorization domain model and shared types |
| `Aiel.Authorization.Application` / `.Application.Contracts` | Application-layer authorization handlers and contracts |
| `Aiel.Authorization.Client` | Client-side `IActionCapabilitySnapshotCache` and `CanExecute` helpers |
| `Aiel.Authorization.Client.Blazor` | Blazor `<CanExecute>` component for capability-driven visibility |
| `Aiel.Authorization.EntityFrameworkCore` / `.PostgreSql` | EF Core persistence for authorization data |
| `Aiel.Authorization.Generators` | Source generator: emits `IActionAuthorizationChecker<T>` and permission name constants from `[AuthorizationDefinition]` |
| `Aiel.Authorization.Analyzers` | Roslyn analyzers enforcing authorization conventions |
| `Aiel.Authorization.Testing` | Test doubles for authorization |

## Multi-Tenancy

### [Aiel.MultiTenancy](./src/Aiel.MultiTenancy/README.md)

Multi-tenancy contracts for tenant-scoped entities and current-tenant resolution.

**Features**:

- `TenantId` — `readonly record struct` strong identifier
- `TenantDescriptor` — resolved tenant identity with optional host routing hint
- `TenantResolution` — discriminated union of resolution outcomes (`Resolved`, `Missing`, `Ambiguous`, `Rejected`, `Error`)
- `ITenantResolver` / `ITenantAccessor` — resolution and access contracts
- `IMultiTenant` — marker for tenant-scoped entities

## Data Access

### [Aiel.DataAccess.Dapper](./src/Aiel.DataAccess.Dapper/README.md)

Column mapping for Dapper enabling property-to-column name mapping via attributes.

**Features**:

- `[HasColumnMaps]` and `[ColumnName]` attributes for declarative mapping
- `ColumnMapper` for automatic mapping discovery from assemblies
- Type-safe mapping without manual configuration

### [Aiel.DataAccess.EntityFrameworkCore](./src/Aiel.DataAccess.EntityFrameworkCore/README.md)

EF Core migration and seeding infrastructure.

**Features**:

- `IDatabaseMigrator` / `DatabaseMigratorBase` — migration contracts with retry-with-jitter
- `DbContextMigrator<TDbContext>` — applies EF Core resilience execution strategy before migrating
- `SeedingExtensions` — `SeedAsync` overloads on `IHost`, `IServiceProvider`, `IServiceScope`
- OpenTelemetry tracing for migration runs (`"Migrations"` activity source)
- Auto-discovery of `IDatabaseMigrator` registrations

## ASP.NET Core & Blazor

### [Aiel.AspNetCore](./src/Aiel.AspNetCore/README.md)

ASP.NET Core integration for the Aiel framework.

**Features**:

- `UseAielTenantResolution()` — per-request tenant resolution middleware
- `RequireTenant()` — endpoint extension for fail-closed tenant enforcement
- `AddAielTenantAccess()` — registers HTTP-context-backed `ITenantAccessor`
- `GetTenantResolution()` — per-request `TenantResolution` via `HttpContext`

### Aiel.AspNetCore.Blazor / Aiel.AspNetCore.Blazor.WebAssembly

Blazor server and WebAssembly integration points (in development).

## Messaging

### [Aiel.MessageBus.Abstractions](./src/Aiel.MessageBus.Abstractions/README.md)

Transport-agnostic integration messaging contracts.

**Features**:

- `IIntegrationMessage` — marker for transport-publishable messages
- `MessageEnvelope<TMessage>` — payload plus strongly-typed metadata
- `MessageMetadata` — correlation, causation, actor, tenant, and message identifiers
- `IMessagePublisher` / `IMessageHandler<T>` — publish and consume contracts
- `IMessageSerializer` / `SerializedMessage` — serialization boundary for adapters and outbox

### [Aiel.MessageBus.Sagas](./src/Aiel.MessageBus.Sagas/README.md)

Durable, correlated saga orchestration contracts.

**Features**:

- `SagaState` — abstract base class for saga state bags
- `IAmStartedByMessage<T>` / `IHandleSagaMessage<T>` — lifecycle markers
- `ICorrelateMessage<TSagaState, TMessage>` — type-safe message-to-saga correlation
- `ISagaRepository<TSagaState>` — persistence seam

### [Aiel.MessageBus.Testing](./src/Aiel.MessageBus.Testing/README.md)

Test doubles for message bus: `RecordingMessagePublisher` and `FakeInboundMessageContextBuilder<T>`.

## GPS / NMEA Parsing

### [Aiel.Gps](./src/Aiel.Gps/README.md)

NMEA 0183 sentence parser built on `System.IO.Pipelines` and `ReadOnlySequence<byte>`.

### [Aiel.Gps.HP](./src/Aiel.Gps.HP/README.md)

High-performance, zero-allocation NMEA 0183 parser for .NET — the next-generation replacement for `Aiel.Gps`.

**Features**:

- True zero-allocation parsing via `ReadOnlySpan<Byte>` and `ref struct` lexer
- ~84 ns per message; 10× faster than `Aiel.Gps`
- Source-generated discriminated union (`NmeaMessage`) for exhaustive type-safe pattern matching
- Async stream processing with `System.IO.Pipelines`

## ID Generation & GUIDs

### [Aiel.IdGeneration](./src/Aiel.IdGeneration/README.md)

Unique identifier generation for various scenarios including database-optimized sequential GUIDs.

**Features**:

- `TimeBasedIdGenerator` — time-based IDs with Base36 encoding
- `KeyGenerator` — cryptographically secure random keys
- `CombGuid` — factory for database-specific sequential GUIDs
- `SqlServerCombGuid` — SQL Server-optimized sequential GUIDs
- `PostgreSqlCombGuid` — PostgreSQL/MySQL/Oracle-optimized sequential GUIDs
- `DatabaseType` enum for selecting appropriate GUID strategy
- `Base36` encoding/decoding utilities

## Internet Types & Email

### [Aiel.InternetTypes](./src/Aiel.InternetTypes/README.md)

Internet-related value objects and types.

**Features**:

- `DomainName` — strongly-typed domain names
- `Serial` — DNS serial numbers with automatic incrementing
- `TTL` — time-to-live values for DNS records
- `Label` — DNS label validation

### [Aiel.Emailing](./src/Aiel.Emailing/README.md)

Email validation, composition, and sending abstractions.

**Features**:

- `MailMessageBuilder` — fluent API for building emails with Markdown support
- `IEmailSender` — abstraction for sending emails
- Multiple email validators (W3C, Strict, Pattern-based, Parsing)
- `Email` and `EmailAddress` value objects
- FluentValidation integration

## Logging

### [Aiel.Logging](./src/Aiel.Logging/README.md) / [Aiel.Logging.Analyzers](./src/Aiel.Logging.Analyzers/README.md)

Roslyn analyzers and code fixes that enforce the Aiel structured-logging convention at compile time.

**Rules**:

| ID | Rule | Severity |
|---|---|---|
| AIEL00008 | `UseAielEventIds` — `EventId` must use a typed enum cast, not a raw integer | Error |
| AIEL00009 | `MissingEventIdParameter` — every `[LoggerMessage]` method must declare an `eventId` parameter | Error |
| AIEL00010 | `MissingEventIdInMessage` — `Message` string must contain the `[{EventId}]` placeholder | Error |
| AIEL00011 | `NoDirectILoggerCalls` — use `[LoggerMessage]` partial methods, not `ILogger.LogXxx(...)` | Warning |
| AIEL00012 | `EventIdMismatch` — attribute `EventId` and parameter default must refer to the same enum member | Error |

## Testing

### [Aiel.Testing](./src/Aiel.Testing/README.md)

Integration testing framework with dependency injection support.

**Features**:

- `IntegrationTestFixture` — base class for xUnit fixtures; one `IHost` per test class
- `IntegrationTestBase<TSut, TFixture>` — one service scope per test
- Configuration management (`appsettings.Testing.json`)
- Lazy SUT initialization
- Proper lifetime management for fixtures and scopes

### [Aiel.Testing.CodeAnalysis](./src/Aiel.Testing.CodeAnalysis/README.md)

Utilities for testing Roslyn analyzers and source generators using `Microsoft.CodeAnalysis.Testing`.

## Installation

All packages are available on NuGet. Install via Package Manager Console:

```pwsh
Install-Package Aiel.Framework
Install-Package Aiel.Results
Install-Package Aiel.Mediator
Install-Package Aiel.StrongIds
Install-Package Aiel.IdGeneration
# ... etc
```

Or via .NET CLI:

```pwsh
dotnet add package Aiel.Framework
dotnet add package Aiel.Results
dotnet add package Aiel.Mediator
dotnet add package Aiel.StrongIds
dotnet add package Aiel.IdGeneration
# ... etc
```

## Quick Start

### Using the Module System

```csharp
[DependsOn(typeof(AielResults))]
[DependsOn(typeof(AielMediator))]
public sealed class MyAppModule : AielDependency
{
    public override ValueTask ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        context.Services.AddScoped<IMyService, MyService>();
        return ValueTask.CompletedTask;
    }
}
```

### Using Strong IDs

```csharp
using Aiel.StrongIds;

[StrongId<Guid>]
public partial record struct UserId;

// Usage
var id = UserId.New();
```

### Using the Result Pattern

```csharp
using Aiel.Results;

public class UserService
{
    public Result<User> GetById(UserId id)
    {
        var user = _repository.Find(id);

        if (user is null)
            return Error.NotFound($"User with ID {id} was not found");

        return user; // Implicit conversion to Result<User>
    }
}
```

### Using the Mediator (CQRS)

```csharp
// Define a query
public sealed record GetUserQuery(UserId Id) : IQuery<UserDto>;

// Implement the handler
public sealed class GetUserHandler : IQueryHandler<GetUserQuery, UserDto>
{
    public async ValueTask<Result> HandleAsync(GetUserQuery query, IQueryDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        // ...
    }
}

// Register and dispatch
services.AddDispatcher(assembly).WithBehavior(typeof(ValidationBehavior<>)).Build();

var result = await sender.QueryAsync(new GetUserQuery(id), cancellationToken);
```

### Using Database-Specific GUIDs

```csharp
using Aiel.IdGeneration;

// For SQL Server
var sqlGuid = CombGuid.NewGuid(DatabaseType.SqlServer);

// For PostgreSQL
var pgGuid = CombGuid.NewGuid(DatabaseType.PostgreSql);
```

### Using Claims Extensions

```csharp
using Aiel.Security;

[ApiController]
public class ProfileController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            FullName = User.FullName(),
            Email = User.Email(),
            TimeZone = User.ZoneInfo()
        });
    }
}
```

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your changes.

### Building the Solution

To build the solution, ensure you have the .NET SDK installed. Then run the following command in the root directory:

```bash
dotnet build
```

Yeah, it is that simple.

### Running Tests

```bash
dotnet test --solution Aiel.slnx
```

Currently includes 1200+ passing tests across all projects.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
