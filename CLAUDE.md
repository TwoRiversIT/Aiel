# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

Aiel is a collection of NuGet packages for building .NET applications following Clean Architecture and Domain-Driven Design principles. It is a **generic, reusable framework** — never shaped for Aviendha-specific needs. Solution file: `Aiel.slnx` (SLNX format).

## Commands

```pwsh
# Build
dotnet build Aiel.slnx -c Debug --nologo

# Test all
dotnet test Aiel.slnx --no-restore --verbosity minimal

# Test one project
dotnet test tests\Aiel.Results.UnitTests --no-restore --verbosity minimal

# Test by name filter
dotnet test tests\Aiel.Results.UnitTests --filter "FullyQualifiedName~MyTest"
```

## Project Conventions

### Namespaces

All `.csproj` files set `<RootNamespace />` (empty). Namespaces come entirely from folder structure. A file at `src/Aiel.MultiTenancy/Aiel/MultiTenancy/TenantId.cs` has namespace `Aiel.MultiTenancy`. Mirror this pattern for new packages.

### File Headers

Every source file begins with the MIT license header (see any existing `.cs` file for the template).

### Module System (`AielDependency` / `[DependsOn]`)

Each package exposes a single `sealed class` that extends `AielDependency`. This is the DI entry point and the module graph node. It:
- Is decorated with `[DependsOn(typeof(OtherDependency))]` for each package it requires
- Overrides `ConfigureAsync` to register services into `context.Services`

```csharp
[DependsOn(typeof(AielAppFramework))]
[DependsOn(typeof(AielMultiTenancy))]
public sealed class AielAspNetCore : AielDependency
{
    public override ValueTask ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        context.Services.TryAddScoped<ITenantAccessor, HttpContextTenantAccessor>();
        return ValueTask.CompletedTask;
    }
}
```

The `Aiel.Generators` source generator produces a compile-time dependency graph from these attributes. New packages must follow this pattern and be added to `Aiel.slnx`.

### Adding New Projects

1. Create `src/Aiel.Foo/Aiel.Foo.csproj` — set `<RootNamespace />`, `<AssemblyName>Aiel.Foo</AssemblyName>`, `<IsPackable>true</IsPackable>`
2. Create `tests/Aiel.Foo.UnitTests/Aiel.Foo.UnitTests.csproj` — `Directory.Build.props` automatically sets `<IsPackable>false</IsPackable>` and `<OutputType>Exe</OutputType>` for projects whose names end with `Tests`
3. Add both to `Aiel.slnx` in the appropriate solution folders
4. Add a `README.md` to the `src/` project

### Fody / ConfigureAwait

`ConfigureAwait.Fody` is active on all non-test projects. Do not write `.ConfigureAwait(false)` — the weaver applies it automatically.

### Test Infrastructure

`Directory.Build.props` auto-imports and adds `Bogus`, `FluentAssertions`, `Moq`, and `xunit.v3` to every test project. No need to reference them explicitly. Use `TestContext.Current.CancellationToken` as the cancellation token in tests.

## Architecture

**Layer dependency direction:** Presentation → Application → Domain → Domain.Shared  
Infrastructure adapts inward; zero business rules in infrastructure.

### Key Package Map

| Package | `AielDependency` class | Purpose |
|---|---|---|
| `Aiel` | `AielAppFramework` | Shared primitives, module system, `DisposableBase`, extensions |
| `Aiel.Domain` | `AielDomain` | `Entity<TKey>`, `AggregateRoot<TKey>`, `IAggregateRepository<,>`, `IDomainEvent` |
| `Aiel.Application.Contracts` | `AielApplicationContracts` | `ICommand`, `IQuery<TResult>`, `ISpecification<T>`, `IQuerySpecification<T>`, `IExecutionContext`, `IDomainEventDispatcher`, `PageRequest`, `PagedResult<T>` |
| `Aiel.Application` | `AielApplication` | Application-layer implementations |
| `Aiel.Results` | `AielResults` | `Result` and `Result<T>` — the only allowed public API return type |
| `Aiel.Mediator` | — | In-process CQRS dispatcher: `ISender`, `IPublisher`, `IPipelineBehavior<T>` |
| `Aiel.StrongIds` | — | `[StrongId<Guid>]` attribute + source generator producing `record struct` typed IDs |
| `Aiel.MultiTenancy` | `AielMultiTenancy` | `TenantId`, `TenantIdentity`, `ITenantResolver`, `ITenantAccessor`, `IMultiTenant` |
| `Aiel.Authorization.*` | `AielAuthorization*` | Authorization domain, application, client, EF Core, generators |
| `Aiel.EntityFrameworkCore` | — | `AielDbContext`, EF Core integration, outbox, domain event dispatch, query specs |
| `Aiel.AspNetCore` | `AielAspNetCore` | HTTP pipeline: tenant resolution middleware, `ITenantAccessor` via `IHttpContextAccessor` |
| `Aiel.Testing` | — | `IntegrationTestFixture` + `IntegrationTestBase<TSut, TFixture>` for xUnit integration tests |
| `Aiel.Analyzers` / `Aiel.Mediator.Analyzers` | — | Roslyn analyzers enforcing framework conventions |
| `Aiel.Generators` | — | Source generator for dependency graph from `[DependsOn]` attributes |

### CQRS and Mediator

Commands (`sealed record : ICommand`) mutate state via `ICommandHandler<TCommand>` returning `ValueTask<Result>`. Queries (`sealed record : IQuery<TDto>`) read state via `IQueryHandler<TQuery, TDto>`. Both dispatch through `ISender`. Register with:

```csharp
services.AddDispatcher(assembly).WithBehavior(typeof(ValidationBehavior<>)).Build();
```

### Result Pattern

`Result` and `Result<T>` are the only allowed public API return types. No `null` returns, no exceptions for control flow. Chain with `.Map()`, `.Bind()`, `.Match()`, `.Tap()` and async variants. Custom error types inherit `Error` as `sealed class` with an internal `ErrorCode` singleton. Call `builder.Services.AddResultPattern()` at startup (required for Blazor WASM JSON deserialization).

### Strong IDs

Declare with `[StrongId<Guid>]` on a `partial record struct`. The source generator produces equality, converters, and factory methods. Never use raw `Guid` or `int` as entity identifiers.

### Read Side

All queries operate on projection-based read models via `IQuerySpecification<TReadModel>` for filtering. Paging uses `PageRequest`; sorting uses `SortRequest`. `IQuerySpecification` must not own paging or sorting. `IReadRepository<TReadModel>` is the read-side contract.

### Execution Context and Tracing

`IExecutionContext` carries `OperationId`, `CorrelationId`, `CausationId`, `ClientInstanceId`, `Actor`, and `Properties`. These flow through command/query handlers, outbox records, and structured log scopes. They are diagnostic metadata — not business data.

### Multi-Tenancy

`AielDbContext` applies global EF Core query filters for `IMultiTenant` entities and stamps `TenantId` on save. Supports Discriminator and Database-per-Tenant models. Schema-per-tenant is intentionally excluded. `ITenantAccessor.GetCurrentTenantAsync` resolves the current `TenantIdentity`; `ITenantResolver.ResolveAsync` returns a discriminated union (`TenantResolution.Resolved | Missing | Ambiguous | Rejected | Error`).

### Integration Testing

`IntegrationTestFixture` creates one `IHost` per test class. `IntegrationTestBase<TSut, TFixture>` creates one service scope per test. Configuration loads from `appsettings.Testing.json`. Expensive setup goes in `InitializeFixtureAsync`; per-test cleanup goes in a `InitializeAsync` override on a derived test base class.

## Coding Conventions

- **Framework types** (`String`, `Int32`, `Boolean`, `Guid`) not C# aliases (`string`, `int`, `bool`)
- **`sealed`** on all concrete types by default
- **`record`** for immutable data; `sealed record` for commands, queries, notifications, value objects
- **`CancellationToken`** on every `async` method, even if unused today
- No `null` returns — use `Result<T>` or value objects
- No tuples in public APIs — use value objects, records, or named types
- No magic strings or numbers in control flow — use `const` or enums
- Commands end with `Command`, queries end with `Query`, handlers end with `Handler`
- Breaking changes are acceptable when they improve design — this is greenfield
