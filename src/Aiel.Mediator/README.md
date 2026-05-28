# Aiel.Mediator

A CQRS-inspired mediator and dispatcher for command, query, and event workflows in .NET applications. This package provides assembly-scanned handler registration, composable pipeline behaviors, and first-class support for structured result-based error handling.

---

## Overview

Aiel.Mediator is the central dispatcher for all action-oriented logic in a Aiel-based application. It scans your assemblies for handlers that implement `ICommandHandler<>`, `IQueryHandler<,>`, and `INotificationHandler<>` interfaces, automatically registers them into the dependency container, and provides a builder to compose pipeline behaviors around each dispatch.

### What this package provides

- **Dispatcher** — A centralized mediator (`ISender` / `IPublisher`) that routes commands, queries, and notifications to their handlers
- **Assembly-scanned handler registration** — Automatically discovers and registers handlers without manual configuration
- **Pipeline behaviors** — Inject cross-cutting concerns like validation and logging around every dispatch
- **Built-in behaviors** — `ValidationBehavior` (FluentValidation) and `LoggingBehavior` (structured logging with timing)
- **Scoped execution** — Each request gets its own dependency scope, ensuring clean isolation
- **Result-based error handling** — Action handlers return `Result`, while `ISender.QueryAsync()` surfaces typed `Result<TDto>` values and validation errors as `ValidationError`

## Key concepts

### Actions, commands, and queries

An **action** is any application operation that routes through the dispatcher. Actions come in two flavors:

- **Commands** — Mutate state. Implement `ICommand`, return `Result` (success or failure).
- **Queries** — Read-only operations. Implement `IQuery<TDto>`; query handlers return `Result`, and `ISender.QueryAsync()` returns `Result<TDto>` to the caller.

Both are marked as `sealed record` or `sealed class` and carry all the data needed by their handler.

### Handlers

A handler processes one specific command or query type. Implement the appropriate interface:

```csharp
public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    public async ValueTask<Result> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        // Mutate state, return Result.Success() or Result.Failure(error)
    }
}

public sealed class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto>
{
    public async ValueTask<Result> HandleAsync(
        GetUserQuery query,
        CancellationToken cancellationToken)
    {
        // Read state, return Result.Success(dto) or Result.Failure(error)
    }
}
```

### Pipeline behaviors

Behaviors wrap each dispatch and run in registration order (first added = outermost). Use them for validation, logging, caching, authorization, or any cross-cutting concern.

```csharp
public sealed class MyBehavior<TAction> : IPipelineBehavior<TAction>
    where TAction : IAction
{
    public async ValueTask<Result> HandleAsync(
        TAction request,
        ActionHandlerDelegate next,
        CancellationToken cancellationToken)
    {
        // Before: inspect request
        var result = await next();
        // After: inspect result
        return result;
    }
}
```

### Notifications

Notifications are one-way events that do not return a result. Multiple handlers can subscribe to the same notification, and all are awaited sequentially.

```csharp
public sealed record UserCreatedNotification(Guid UserId) : INotification;

public sealed class SendWelcomeEmailHandler : INotificationHandler<UserCreatedNotification>
{
    public async ValueTask HandleAsync(
        UserCreatedNotification notification,
        CancellationToken cancellationToken)
    {
        // Send email. If one or more handlers throw, PublishAsync() surfaces an AggregateException after every registered handler has run.
    }
}
```

## Registration and usage

### Setup

Call `AddDispatcher()` in your service configuration and chain `.WithBehavior()` calls before `.Build()`:

```csharp
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services
            .AddDispatcher(typeof(Program).Assembly)
            .WithBehavior(typeof(ValidationBehavior<>))
            .WithBehavior(typeof(LoggingBehavior<>))
            .Build();
    });
```

The builder scans the assemblies you pass (in this example, your application assembly) for any classes implementing `ICommandHandler<>`, `IQueryHandler<,>`, or `INotificationHandler<>`, registers them as scoped services, and freezes the dispatcher registry.

### Dispatching commands

Inject `ISender` and call `ExecuteAsync()`:

```csharp
public sealed class UserController
{
    private readonly ISender _sender;

    public UserController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(CreateUserDto dto, CancellationToken ct)
    {
        var command = new CreateUserCommand(dto.Name, dto.Email);
        var result = await _sender.ExecuteAsync(command, ct);

        return result.IsSuccess
            ? Ok()
            : BadRequest(result.Error);
    }
}
```

### Dispatching queries

Use the same `ISender` interface with generic type argument:

```csharp
[HttpGet("users/{id}")]
public async Task<IActionResult> GetUser(Guid id, CancellationToken ct)
{
    var query = new GetUserQuery(id);
    var result = await _sender.QueryAsync<UserDto>(query, ct);

    return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(result.Error);
}
```

### Publishing notifications

Inject `IPublisher` and call `PublishAsync()`. If no handlers are registered, the publish is a no-op:

```csharp
var notification = new UserCreatedNotification(userId);
await _publisher.PublishAsync(notification, cancellationToken);
```

## Main abstractions

### Interfaces you implement

| Interface | Purpose | Return type |
|-----------|---------|-------------|
| `IActionHandler<TAction>` | Base handler interface | `Result` |
| `ICommandHandler<TCommand>` | Command handler (sealed record or sealed class) | `Result` |
| `IQueryHandler<TQuery, TDto>` | Query handler | `Result` |
| `INotificationHandler<TNotification>` | Event subscriber | `ValueTask` (no result) |
| `IPipelineBehavior<TAction>` | Middleware-like wrapper | `Result` |

### Interfaces you use (dispatcher)

| Interface | Method | Purpose |
|-----------|--------|---------|
| `ISender` | `ExecuteAsync(ICommand, CancellationToken)` | Dispatch a command |
| `ISender` | `QueryAsync<TDto>(IQuery<TDto>, CancellationToken)` | Dispatch a query with typed result |
| `IPublisher` | `PublishAsync<TNotification>(TNotification, CancellationToken)` | Emit a one-way notification |

### Types

- **`Result<T>`** — Discriminated union of success (carries `T`) or failure (carries `Error`). Returned by the dispatcher's `QueryAsync()` method; query handlers typically produce it via `Result.Success(dto)` while still satisfying the `Result`-returning handler contract.
- **`Result`** — Discriminated union of success or failure. Returned by all command handlers and the dispatcher's `ExecuteAsync()` method.
- **`ValidationError`** — A specialized `Error` subclass that carries `IEnumerable<ValidationFailure>` from FluentValidation. Created by `ValidationBehavior` when validation fails.

## Example flow

Here's a complete, minimal example:

```csharp
// 1. Define the command (sealed, carries all input)
public sealed record CreateUserCommand(string Name, string Email) : ICommand;

// 2. Optional: define validators
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress();
    }
}

// 3. Implement the handler
public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IUserRepository _users;

    public CreateUserCommandHandler(IUserRepository users)
    {
        _users = users;
    }

    public async ValueTask<Result> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = new User(command.Name, command.Email);
        await _users.AddAsync(user, cancellationToken);
        return Result.Success();
    }
}

// 4. Register and compose behaviors in Program.cs
services
    .AddDispatcher(typeof(Program).Assembly)
    .WithBehavior(typeof(ValidationBehavior<>))
    .WithBehavior(typeof(LoggingBehavior<>))
    .Build();

// 5. Dispatch from a controller or application service
var result = await sender.ExecuteAsync(new CreateUserCommand("Alice", "alice@example.com"), ct);
if (result.IsSuccess)
{
    return Ok("User created");
}
else if (result.Error is ValidationError ve)
{
    return BadRequest(new { errors = ve.Failures });
}
else
{
    return StatusCode(500, new { error = result.Error.Message });
}
```

## Testing and limitations

### Testing handlers in isolation

Handlers are scoped services that depend on repositories, services, or other DI-managed types. Unit test them directly:

```csharp
[Fact]
public async Task HandleAsync_WithValidCommand_CreatesUser()
{
    var repository = new Mock<IUserRepository>();
    var handler = new CreateUserCommandHandler(repository.Object);

    var command = new CreateUserCommand("Bob", "bob@example.com");
    var result = await handler.HandleAsync(command, CancellationToken.None);

    Assert.True(result.IsSuccess);
    repository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### Testing the full pipeline

For integration tests, use a test host and inject `ISender`:

```csharp
[Fact]
public async Task ExecuteAsync_WithInvalidCommand_ValidationFails()
{
    var host = Host.CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            services.AddScoped<IUserRepository, InMemoryUserRepository>();
            services
                .AddDispatcher(typeof(Program).Assembly)
                .WithBehavior(typeof(ValidationBehavior<>))
                .Build();
        })
        .Build();

    var sender = host.Services.GetRequiredService<ISender>();
    var result = await sender.ExecuteAsync(new CreateUserCommand("", "invalid"), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.IsType<ValidationError>(result.Error);
}
```

### Known limitations and design decisions

- **Behaviors run in registration order** — The first behavior added wraps all others. This is fixed at startup; you cannot alter the pipeline per-request.
- **Notification handlers are invoked sequentially** — All handlers for a notification run one after another in registration order. If you need parallel execution, you can `Task.WhenAll()` them inside a behavior.
- **Notification publish awaits all handlers** — Exceptions from notification handlers are collected and rethrown as an `AggregateException` after every registered handler has been invoked. If no handlers are registered for a notification type, `PublishAsync()` returns immediately (no-op).
- **Handlers are scoped** — Each dispatch creates a new dependency scope and is cleaned up when the dispatch completes. This ensures isolation but prevents long-lived cached state within a handler.
- **Query handlers must return `Result`** — The dispatcher casts query results to `Result<TDto>` internally. If a behavior short-circuits with a plain `Result.Failure()`, it is promoted to `Result<TDto>.Failure()` automatically.
- **Validation behavior requires FluentValidation** — If you use `ValidationBehavior<>`, you must add `AbstractValidator<TAction>` implementations for any action you want to validate. Behaviors without validators are skipped.
s without validators are skipped.
