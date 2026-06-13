# CQRS Action Dispatcher

## Overview

Inspired by libraries like MediatR, the Action Dispatcher provides a clean, decoupled way to implement the Command and Query patterns. It allows you to define actions (commands/queries) and their handlers without tight coupling, while also supporting cross-cutting concerns through pipeline behaviors.

## Public API

```csharp
public interface IAction;

public interface ICommand : IAction;

public interface IQuery<out TResult> : IAction;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    ValueTask Handle(TCommand command, CancellationToken cancellationToken = default);
}

public interface IQueryHandler<in TQuery, TDto>
    where TQuery : IQuery<TDto>
    where TDto : Result<TDto>
{
    ValueTask<TDto> Handle(TQuery query, CancellationToken cancellationToken = default);
}

public delegate ValueTask<TDto> ActionHandlerDelegate<TDto>();

public interface IPipelineBehavior<in TAction, TDto>
    where TAction : IAction<TDto>
{
    ValueTask<Result<TDto>> Handle(
        TAction action,
        ActionHandlerDelegate<TDto> next,
        CancellationToken cancellationToken = default);
}

public interface ISender
{
    ValueTask<TDto> Send<TDto>(
        IAction<TDto> action,
        CancellationToken cancellationToken = default);
}
```

Implementation details are hidden behind the `Dispatcher` class, which is registered as the implementation for `ISender`. The dispatcher uses reflection to find the appropriate handler and pipeline behaviors for each action type.

```csharp

internal sealed class Dispatcher(
    IServiceProvider provider,
    DispatcherRegistry registry) : ISender, IPublisher
{
    public ValueTask<TResult> Send<TResult>(
        IAction<TResult> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!registry.ActionWrappers.TryGetValue(action.GetType(), out var wrapper))
        {
            throw new InvalidOperationException(
                $"No handler registered for action type '{action.GetType().FullName}'.");
        }

        // Reference-type cast - cheap, no boxing.
        return ((ActionHandlerBase<TResult>)wrapper).Handle(action, provider, cancellationToken);
    }

    public ValueTask Publish<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (!registry.NotificationWrappers.TryGetValue(notification.GetType(), out var wrapper))
        {
            return ValueTask.CompletedTask;
        }

        return wrapper.Handle(notification, provider, cancellationToken);
    }
}

/// <summary>
/// Built once at startup, frozen for the lifetime of the application. Singleton.
/// </summary>
internal sealed class DispatcherRegistry(
    FrozenDictionary<Type, ActionHandlerBase> actionWrappers,
    FrozenDictionary<Type, NotificationHandlerBase> notificationWrappers)
{
    public FrozenDictionary<Type, ActionHandlerBase> ActionWrappers { get; } = actionWrappers;
    public FrozenDictionary<Type, NotificationHandlerBase> NotificationWrappers { get; } = notificationWrappers;
}

public static class DispatcherRegistration
{
    public static IServiceCollection AddDispatcher(this IServiceCollection services, Assembly assembly)
    {
        var actionWrappers = new Dictionary<Type, ActionHandlerBase>();
        var notificationWrappers = new Dictionary<Type, NotificationHandlerBase>();

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface) continue;

            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType) continue;
                var def = iface.GetGenericTypeDefinition();

                if (def == typeof(IActionHandler<,>))
                {
                    services.AddScoped(iface, type);

                    var args = iface.GetGenericArguments();
                    var actionType = args[0];
                    var resultType = args[1];

                    if (!actionWrappers.ContainsKey(actionType))
                    {
                        var wrapperType = typeof(ActionHandlerWrapper<,>)
                            .MakeGenericType(actionType, resultType);
                        actionWrappers[actionType] =
                            (ActionHandlerBase)Activator.CreateInstance(wrapperType)!;
                    }
                }
                else if (def == typeof(INotificationHandler<>))
                {
                    services.AddScoped(iface, type);

                    var notificationType = iface.GetGenericArguments()[0];
                    if (!notificationWrappers.ContainsKey(notificationType))
                    {
                        var wrapperType = typeof(NotificationHandlerWrapper<>)
                            .MakeGenericType(notificationType);
                        notificationWrappers[notificationType] =
                            (NotificationHandlerBase)Activator.CreateInstance(wrapperType)!;
                    }
                }
            }
        }

        var registry = new DispatcherRegistry(
            actionWrappers.ToFrozenDictionary(),
            notificationWrappers.ToFrozenDictionary());

        services.AddSingleton(registry);
        services.AddScoped<Dispatcher>();
        services.AddScoped<ISender>(sp => sp.GetRequiredService<Dispatcher>());
        services.AddScoped<IPublisher>(sp => sp.GetRequiredService<Dispatcher>());

        return services;
    }

    /// <summary>
    /// Registers an open-generic pipeline behavior. Order of registration is order of execution.
    /// </summary>
    public static IServiceCollection AddPipelineBehavior(
        this IServiceCollection services,
        Type openGenericBehaviorType)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), openGenericBehaviorType);
        return services;
    }
}

internal abstract class ActionHandlerBase;

internal abstract class ActionHandlerBase<TResult> : ActionHandlerBase
{
    public abstract ValueTask<TResult> Handle(
        IAction<TResult> action,
        IServiceProvider provider,
        CancellationToken cancellationToken = default);
}

internal sealed class ActionHandlerWrapper<TAction, TResult> : ActionHandlerBase<TResult>
    where TAction : IAction<TResult>
{
    public override ValueTask<TResult> Handle(
        IAction<TResult> action,
        IServiceProvider provider,
        CancellationToken cancellationToken = default)
    {
        var typed = (TAction)action;
        var handler = provider.GetRequiredService<IActionHandler<TAction, TResult>>();
        var behaviors = provider.GetServices<IPipelineBehavior<TAction, TResult>>();

        // Build the pipeline: handler at the core, behaviors wrapped outside in registration order.
        // Iterating in reverse means the first registered behavior runs outermost.
        ActionHandlerDelegate<TResult> pipeline = () => handler.Handle(typed, cancellationToken);

        foreach (var behavior in behaviors.Reverse())
        {
            var next = pipeline;
            var current = behavior;
            pipeline = () => current.Handle(typed, next, cancellationToken);
        }

        return pipeline();
    }
}
```

```csharp
public static IServiceCollection AddPipelineBehavior(
    this IServiceCollection services,
    Type openGenericBehaviorType)
{
    services.AddScoped(typeof(IPipelineBehavior<,>), openGenericBehaviorType);
    return services;
}
```

```csharp
public sealed class LoggingBehavior<TAction, TResult>(ILogger<LoggingBehavior<TAction, TResult>> logger)
    : IPipelineBehavior<TAction, TResult>
    where TAction : IAction<TResult>
{
    public async ValueTask<TResult> Handle(
        TAction action,
        ActionHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        var actionName = typeof(TAction).Name;
        logger.LogInformation("Handling {ActionName}", actionName);

        var sw = Stopwatch.StartNew();
        try
        {
            var result = await next();
            sw.Stop();
            logger.LogInformation("Handled {ActionName} in {Elapsed}ms", actionName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Handler {ActionName} threw after {Elapsed}ms", actionName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
```

```csharp
public sealed class ValidationBehavior<TAction, TResult>(IEnumerable<IValidator<TAction>> validators)
    : IPipelineBehavior<TAction, TResult>
    where TAction : IAction<TResult>
{
    public async ValueTask<TResult> Handle(
        TAction action,
        ActionHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TAction>(action);
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            if (!result.IsValid)
            {
                failures.AddRange(result.Errors);
            }
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
```

```csharp
public interface ICacheable
{
    string CacheKey { get; }
    TimeSpan? Expiration => null;
}

public sealed record GetProductQuery(Guid Id) : IAction<ProductDto?>, ICacheable
{
    public string CacheKey => $"product:{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}

public sealed class CachingBehavior<TAction, TResult>(
    HybridCache cache,
    ILogger<CachingBehavior<TAction, TResult>> logger)
    : IPipelineBehavior<TAction, TResult>
    where TAction : IAction<TResult>
{
    public async ValueTask<TResult> Handle(
        TAction action,
        ActionHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        if (action is not ICacheable cacheable)
        {
            return await next();
        }

        var options = cacheable.Expiration is { } expiration
            ? new HybridCacheEntryOptions { Expiration = expiration }
            : null;

        return await cache.GetOrCreateAsync(
            cacheable.CacheKey,
            async ct =>
            {
                logger.LogInformation("Cache miss for {Key}", cacheable.CacheKey);
                return await next();
            },
            options,
            cancellationToken: cancellationToken);
    }
}```

```csharp
public interface ITransactional;

public sealed class TransactionBehavior<TAction, TResult>(
    AppDbContext db,
    ILogger<TransactionBehavior<TAction, TResult>> logger)
    : IPipelineBehavior<TAction, TResult>
    where TAction : IAction<TResult>
{
    public async ValueTask<TResult> Handle(
        TAction action,
        ActionHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        if (action is not ITransactional)
        {
            return await next();
        }

        // The InMemory provider does not support real transactions - this is a no-op there.
        // For SQL Server / Postgres / SQLite this opens, commits, or rolls back as expected.
        if (!db.Database.IsInMemory())
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await next();
                await tx.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                logger.LogWarning("Rolling back transaction for {Action}", typeof(TAction).Name);
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        }

        return await next();
    }
}
```

```csharp

// Registration in Program.cs or Startup.cs
builder.Services.AddDispatcher(Assembly.GetExecutingAssembly());

// Pipeline configuration in the orders you want them to execute (first registered runs outermost)
builder.Services.AddPipelineBehavior(typeof(LoggingBehavior<,>));
builder.Services.AddPipelineBehavior(typeof(ValidationBehavior<,>));
builder.Services.AddPipelineBehavior(typeof(PermissionBehavior<,>));
builder.Services.AddPipelineBehavior(typeof(CachingBehavior<,>));
builder.Services.AddPipelineBehavior(typeof(TransactionBehavior<,>));

```

```csharp
public sealed record ProductCreatedNotification(Guid ProductId, string Name) : INotification;

public sealed class LogProductCreatedHandler(ILogger<LogProductCreatedHandler> logger)
    : INotificationHandler<ProductCreatedNotification>
{
    public ValueTask Handle(ProductCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Product created: {Id} - {Name}", notification.ProductId, notification.Name);
        return ValueTask.CompletedTask;
    }
}

app.MapPost("/products", async (
    CreateProductCommand command, ISender sender, IPublisher publisher, CancellationToken ct) =>
{
    var id = await sender.Send(command, ct);
    await publisher.Publish(new ProductCreatedNotification(id, command.Name), ct);
    return Results.Created($"/products/{id}", new { id });
});
```

```csharp
public interface INotification;

public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    ValueTask Handle(TNotification notification, CancellationToken cancellationToken = default);
}

public interface IPublisher
{
    ValueTask Publish<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification;
}

internal abstract class NotificationHandlerBase
{
    public abstract ValueTask Handle(
        object notification,
        IServiceProvider provider,
        CancellationToken cancellationToken = default);
}

internal sealed class NotificationHandlerWrapper<TNotification> : NotificationHandlerBase
    where TNotification : INotification
{
    public override async ValueTask Handle(
        object notification,
        IServiceProvider provider,
        CancellationToken cancellationToken = default)
    {
        var typed = (TNotification)notification;
        var handlers = provider.GetServices<INotificationHandler<TNotification>>();

        // Sequential await - simple and predictable. Swap for Task.WhenAll if you want parallel.
        foreach (var handler in handlers)
        {
            await handler.Handle(typed, cancellationToken);
        }
    }
}
```
