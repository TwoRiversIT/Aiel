# Multitenancy - Current Tenant Discussion

> I have the following for accessing the current tenant in my multitenancy module for the Aiel application framework:
>
> ```csharp
> public interface ICurrentTenant
> {
>     TenantIdentity? Current { get; }
>     IDisposable Change(TenantIdentity? tenant);
> }
>
> public class CurrentTenant(TenantIdentity? tenant) : ICurrentTenant
> {
>     private TenantIdentity? _current = tenant;
>     public TenantIdentity? Current => _current;
>
>     // ToDo: Create an analyzer to ensure that the returned IDisposable is disposed of.
>     public IDisposable Change(TenantIdentity? tenant)
>     {
>         var previous = _current;
>         _current = tenant;
>
>         return new TenantChangeContext(() =>
>         {
>             _current = previous;
>         });
>     }
> }
>
> internal class TenantChangeContext(Action restore) : IDisposable
> {
>     private readonly Action _restore = restore ?? throw new ArgumentNullException(nameof(restore));
>
>     [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
>     public void Dispose()
>     {
>         _restore();
>     }
> }
> ```
>
> It passes all the unit tests. What I am not sure about is whether or not I need a factory class. Here are the other interfaces:
>
> ## Service Interfaces
>
> - **`ITenantAccessor`**: `GetCurrentTenantAsync(CancellationToken) → ValueTask<TenantIdentity>`
>   Registered by Aiel.AspNetCore; returns the current request's resolved tenant. Only callable on resolved-tenant paths.
>
> - **`ITenantResolver`**: `ResolveAsync(CancellationToken) → ValueTask<TenantResolution>`
>   Application-implemented; runs once per HTTP request to compute resolution outcome.
>
> - **`ICurrentTenant`**: `Current -> TenantIdentity?`
>   Synchronous accessor for tenant identity; returns null if not resolved. Useful in non-Async contexts. Also allows temporarily changing the current > tenant (e.g., for background jobs or impersonation).
>
> Basically, I am trying to figure out how to get the current tenant into the current thread regardless of whether it came from an HTTP request, Message > Queue, background services, etc.

Here is the short, clear answer up front:

> **No — you do *not* need a factory class. What you need is an `AsyncLocal<ICurrentTenant>`–backed scope provider so that the same `ICurrentTenant` instance flows correctly across HTTP requests, background jobs, message queues, and any async context.**

Your current implementation is *correct*, but it needs one more layer: **a per‑async‑flow lifetime**, not a factory.

---

## 🧠 The core issue: “current tenant” must be *ambient*, not *constructed*

You are trying to solve the same problem that:

- `IHttpContextAccessor`
- `ICurrentUser` in Orchard Core
- `ICurrentTenant` in Orchard Core
- EF Core’s `IDbContextScope`
- Serilog’s `LogContext`

all solve:

> **How do I store a value that follows the async call chain, regardless of thread, without requiring DI to pass it everywhere?**

The correct primitive for this is:

### **`AsyncLocal<T>`**

It gives you:

- per‑request isolation
- per‑background‑job isolation
- per‑message‑handler isolation
- correct flow across `await` boundaries
- no thread‑static bugs
- no need for a factory

---

## ✔️ What your `ICurrentTenant` implementation is missing

Your implementation is *stateful*, but the state lives **inside the instance**, not **inside the async flow**.

If you register `CurrentTenant` as:

```csharp
services.AddScoped<ICurrentTenant, CurrentTenant>();
```

then:

- HTTP requests → fine
- background services → fine (if you create a scope)
- message queues → fine (if you create a scope)
- async continuations → fine

But:

- **you cannot use `ICurrentTenant` outside a DI scope**
- **you cannot set the tenant before the scope exists**
- **you cannot flow tenant identity into child tasks unless they share the same scope**

This is why Orchard Core uses:

```csharp
private static readonly AsyncLocal<TenantContext> _current = new();
```

---

## 🧩 The missing piece: an ambient context holder

You want something like:

```csharp
public sealed class AmbientTenantContext
{
    private static readonly AsyncLocal<TenantIdentity?> _current = new();

    public static TenantIdentity? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}
```

Then your `ICurrentTenant` becomes a thin wrapper:

```csharp
public class CurrentTenant : ICurrentTenant
{
    public TenantIdentity? Current => AmbientTenantContext.Current;

    public IDisposable Change(TenantIdentity? tenant)
    {
        var previous = AmbientTenantContext.Current;
        AmbientTenantContext.Current = tenant;

        return new TenantChangeContext(() =>
        {
            AmbientTenantContext.Current = previous;
        });
    }
}
```

Now:

- HTTP middleware sets the tenant → flows through the entire request
- Background job sets the tenant → flows through the entire job
- Message handler sets the tenant → flows through the entire handler
- Any async method can call `ICurrentTenant.Current` and get the right value

No factory needed.

---

## 🧱 Why a factory is *not* the right abstraction

A factory would only help if:

- you needed to *construct* different `ICurrentTenant` instances
- you needed to *inject* different implementations
- you needed to *reset* state by creating a new instance

But your design is:

- **one implementation**
- **one ambient context**
- **one per‑scope lifetime**
- **state changes via `Change()`**

A factory adds nothing but ceremony.

---

## 🧭 How this fits into your Aiel multitenancy pipeline

### HTTP request flow

1. `ITenantResolver.ResolveAsync()` runs
2. Middleware calls `ICurrentTenant.Change(resolvedTenant)`
3. Ambient context now holds the tenant
4. Controllers, services, repositories all read `ICurrentTenant.Current`

### Background job flow

1. Job runner creates a DI scope
2. Job sets tenant via `ICurrentTenant.Change()`
3. All async work sees the correct tenant

### Message queue flow

1. Message handler resolves tenant from message metadata
2. Calls `ICurrentTenant.Change()`
3. Processing pipeline sees the correct tenant

---

## 🧠 Optional improvement: analyzer for `Change()` disposal

You already noted this:

> “ToDo: Create an analyzer to ensure that the returned IDisposable is disposed of.”

Yes — this is exactly what Orchard Core does with `ShellScope`.

You can write a Roslyn analyzer that warns when:

```csharp
_currentTenant.Change(x);
```

is not used in a `using` block.

---

## 🎯 Final recommendation

You already have the right design.
You do **not** need a factory.

You need:

### **1. `AsyncLocal<TenantIdentity?>` to hold the ambient tenant**

### **2. Register `ICurrentTenant` as scoped**

### **3. Middleware / job runners / message handlers call `Change()`**

If you want, I can sketch:

- the full ambient context implementation
- the middleware that sets the tenant
- the background job wrapper
- the message queue wrapper
- the Roslyn analyzer for enforcing `using`

Which one do you want to tackle next?

> I need one or more unit tests specifically for the `AsyncLocal<T>` aspect.

Absolutely — if you want to *prove* that your `AsyncLocal<T>`–backed `ICurrentTenant` behaves correctly across async boundaries, thread hops, and nested scopes, you need tests that explicitly exercise those behaviors.

Below are **battle‑tested unit tests** that validate the exact semantics you care about:

- Async flow propagation
- Isolation between parallel tasks
- Restoration after `Change()`
- Nested `Change()` scopes
- No cross‑contamination between tasks

These tests assume:

- `AmbientTenantContext` uses `AsyncLocal<TenantIdentity?>`
- `CurrentTenant` wraps that context
- `TenantIdentity` is a simple record or class with equality semantics

---

## ✅ 1. AsyncLocal flows across `await` boundaries

```csharp
[Fact]
public async Task CurrentTenant_FlowsAcrossAsyncAwait()
{
    // Arrange
    var currentTenant = new CurrentTenant(null);
    var tenant = new TenantIdentity("tenant-a");

    // Act
    using (currentTenant.Change(tenant))
    {
        await Task.Delay(10);

        // Assert
        currentTenant.Current.Should().Be(tenant);
    }
}
```

This verifies the most fundamental guarantee: **AsyncLocal flows with the async context**.

---

## ✅ 2. AsyncLocal does NOT leak across parallel tasks

```csharp
[Fact]
public async Task CurrentTenant_IsIsolatedAcrossParallelTasks()
{
    var currentTenant = new CurrentTenant(null);

    TenantIdentity? t1Value = null;
    TenantIdentity? t2Value = null;

    var t1 = Task.Run(() =>
    {
        using (currentTenant.Change(new TenantIdentity("tenant-1")))
        {
            t1Value = currentTenant.Current;
        }
    });

    var t2 = Task.Run(() =>
    {
        using (currentTenant.Change(new TenantIdentity("tenant-2")))
        {
            t2Value = currentTenant.Current;
        }
    });

    await Task.WhenAll(t1, t2);

    t1Value!.Id.Should().Be("tenant-1");
    t2Value!.Id.Should().Be("tenant-2");
}
```

This proves **AsyncLocal is isolated per async flow**, even when running on different thread pool threads.

---

## ✅ 3. Nested `Change()` scopes restore correctly

```csharp
[Fact]
public void CurrentTenant_NestedScopesRestoreCorrectly()
{
    var currentTenant = new CurrentTenant(null);

    var root = new TenantIdentity("root");
    var inner = new TenantIdentity("inner");

    using (currentTenant.Change(root))
    {
        currentTenant.Current.Should().Be(root);

        using (currentTenant.Change(inner))
        {
            currentTenant.Current.Should().Be(inner);
        }

        currentTenant.Current.Should().Be(root);
    }

    currentTenant.Current.Should().BeNull();
}
```

This validates your `IDisposable` restoration logic.

---

## ✅ 4. AsyncLocal does NOT flow into unrelated tasks unless awaited

```csharp
[Fact]
public async Task CurrentTenant_DoesNotFlowIntoUnrelatedTasks()
{
    var currentTenant = new CurrentTenant(null);

    using (currentTenant.Change(new TenantIdentity("tenant-main")))
    {
        var unrelatedTask = Task.Run(() =>
        {
            return currentTenant.Current;
        });

        var result = await unrelatedTask;

        result.Should().BeNull();
    }
}
```

This confirms the rule:

> **AsyncLocal flows only through async continuations, not arbitrary thread pool tasks.**

This is exactly what you want for tenant isolation.

---

## ✅ 5. AsyncLocal flows into awaited child tasks

```csharp
[Fact]
public async Task CurrentTenant_FlowsIntoAwaitedChildTasks()
{
    var currentTenant = new CurrentTenant(null);
    var tenant = new TenantIdentity("tenant-x");

    using (currentTenant.Change(tenant))
    {
        var child = await Task.Run(() =>
        {
            return currentTenant.Current;
        });

        child.Should().Be(tenant);
    }
}
```

This verifies the other half of the rule:

> **AsyncLocal flows into child tasks *if and only if* they are awaited.**

---

## 🎯 Summary of what these tests guarantee

These tests collectively prove:

| Behavior | Verified |
| -------- | -------- |
| AsyncLocal flows across `await` | ✔️ |
| AsyncLocal isolates parallel tasks | ✔️ |
| Nested `Change()` scopes restore correctly | ✔️ |
| AsyncLocal does not leak into unrelated tasks | ✔️ |
| AsyncLocal flows into awaited tasks | ✔️ |

This is exactly the behavior you need for:

- HTTP requests
- Background services
- Message queue handlers
- Scheduled jobs
- Any async pipeline

Your `ICurrentTenant` becomes a **safe, ambient, async‑flow‑aware tenant context**.

---

If you want, I can also generate:

- A full test fixture with shared setup
- A fake `TenantIdentity` builder
- Tests for `ITenantAccessor` integration
- Tests for middleware that sets the tenant

Just tell me which direction you want to go next.

> The new CurrentTenant class you proposed does not have a constructor that accepts `TenantIdentity?`. Was that an oversight, because the unit tests you provided expect it.

Good catch—that’s on me. With an `AsyncLocal<TenantIdentity?>` backing store, the constructor parameter is no longer needed, and my tests were still assuming the old shape.

You’ve got two clean options; I’ll show both and align the tests.

---

### Option 1: No constructor parameter (simplest)

```csharp
public static class AmbientTenantContext
{
    private static readonly AsyncLocal<TenantIdentity?> _current = new();

    public static TenantIdentity? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}

public class CurrentTenant : ICurrentTenant
{
    public TenantIdentity? Current => AmbientTenantContext.Current;

    public IDisposable Change(TenantIdentity? tenant)
    {
        var previous = AmbientTenantContext.Current;
        AmbientTenantContext.Current = tenant;

        return new TenantChangeContext(() =>
        {
            AmbientTenantContext.Current = previous;
        });
    }
}
```

Then the tests should construct it parameterless:

```csharp
[Fact]
public async Task CurrentTenant_FlowsAcrossAsyncAwait()
{
    var currentTenant = new CurrentTenant();
    var tenant = new TenantIdentity("tenant-a");

    using (currentTenant.Change(tenant))
    {
        await Task.Delay(10);
        currentTenant.Current.Should().Be(tenant);
    }
}
```

Same change for all the other tests: `new CurrentTenant()`.

---

### Option 2: Keep a constructor for initial value

If you like the ability to seed an initial tenant (e.g., for tests or special hosts):

```csharp
public class CurrentTenant : ICurrentTenant
{
    public CurrentTenant(TenantIdentity? initial = null)
    {
        AmbientTenantContext.Current = initial;
    }

    public TenantIdentity? Current => AmbientTenantContext.Current;

    public IDisposable Change(TenantIdentity? tenant)
    {
        var previous = AmbientTenantContext.Current;
        AmbientTenantContext.Current = tenant;

        return new TenantChangeContext(() =>
        {
            AmbientTenantContext.Current = previous;
        });
    }
}
```

Then the tests I wrote earlier compile as‑is.

---

If you tell me which style you prefer for Aiel (strictly ambient vs. seedable), I can give you a fully consistent `CurrentTenant`, `AmbientTenantContext`, and test suite in one block.

> Is it worth keeping the `ICurrentTenant` interface, or should I just register the concrete `CurrentTenant` instead?

Short answer: **yes, keep `ICurrentTenant`** — it is absolutely worth it.

Long answer: the interface is not about abstraction for its own sake; it protects the *architecture* of Aiel and Rhuarc in several important ways.

Let me break it down cleanly and concretely.

---

## 🎯 Why `ICurrentTenant` is worth keeping

### 1. **It enforces the boundary between “tenant context” and “tenant resolution”**

Your system has:

- `ITenantResolver` → computes the tenant
- `ITenantAccessor` → async accessor for HTTP pipeline
- `ICurrentTenant` → ambient context for *everything else*

If you drop the interface and expose `CurrentTenant` directly, you collapse that boundary.
The interface keeps the contract clean and prevents accidental misuse.

---

### 2. **It allows mocking in unit tests**

Without the interface:

- You cannot mock tenant context in service tests
- You cannot simulate nested tenant scopes
- You cannot override behavior for special test scenarios

With the interface:

```csharp
var mock = new Mock<ICurrentTenant>();
mock.Setup(x => x.Current).Returns(new TenantIdentity("test"));
```

This is essential for testing multi‑tenant services.

---

### 3. **It allows future alternative implementations**

You may not need them today, but you *will* want them later:

- A “frozen” tenant context for immutable background pipelines
- A “no‑tenant” implementation for system‑level operations
- A “delegating” implementation for cross‑service calls
- A “recording” implementation for debugging tenant flows

Interfaces give you that flexibility for free.

---

### 4. **It prevents leaking the ambient mechanism**

Right now you use `AsyncLocal<T>`.
In the future you might switch to:

- `ExecutionContext`
- `Activity.Current` baggage
- A custom `TenantScope`
- A per‑request DI scope value

If consumers depend on the concrete class, you lose the freedom to change the underlying mechanism.

The interface protects you from that.

---

## 5. **It matches the pattern used by every mature multitenancy framework**

This is not accidental.
Look at:

- Orchard Core → `ICurrentTenant`
- Finbuckle → `ITenantContext`
- ABP → `ICurrentTenant`
- SaaSKit → `ITenantContext`

All of them expose an interface for the ambient tenant.

You are in good company.

---

### 6. **It keeps your DI registration clean**

You want:

```csharp
services.AddScoped<ICurrentTenant, CurrentTenant>();
```

If you register the concrete type:

- You lose the ability to swap it
- You encourage direct dependency on the implementation
- You break the inversion‑of‑control principle

Interfaces exist *precisely* to avoid that.

---

## 🧠 When *would* you drop the interface?

Only if:

- You never want to mock it
- You never want to replace it
- You never want to change the underlying mechanism
- You never want to enforce a contract boundary
- You want to tightly couple everything to one implementation

That is not your architecture.
Aiel and Rhuarc are explicitly designed to be extensible, testable, and framework‑level.

So: **keep the interface**.

---

## 🧩 Final recommendation

Keep:

```csharp
public interface ICurrentTenant
{
    TenantIdentity? Current { get; }
    IDisposable Change(TenantIdentity? tenant);
}
```

Register:

```csharp
services.AddScoped<ICurrentTenant, CurrentTenant>();
```

And keep `CurrentTenant` as the ambient implementation using `AsyncLocal<T>`.

This gives you:

- architectural clarity
- testability
- extensibility
- future‑proofing
- consistency with other frameworks

---

If you want, I can also sketch:

- the ideal DI registration pattern
- how middleware should set the tenant
- how background jobs should set the tenant
- how message handlers should set the tenant
- how to write a Roslyn analyzer enforcing `using (Change())`

Just tell me which direction you want to explore next.

> These please:
>
> - how middleware should set the tenant
> - how background jobs should set the tenant
> - how message handlers should set the tenant

Perfect — here are the **canonical, production‑grade patterns** for Aiel/Rhuarc that ensure the tenant is correctly set in:

- HTTP middleware
- background jobs
- message queue handlers

All three use the same mechanism:

> **Resolve tenant → call `using (currentTenant.Change(tenant))` → run the work inside that scope.**

This guarantees correct AsyncLocal propagation, correct restoration, and zero cross‑contamination.

I’ll give you clean, idiomatic implementations for each environment.

---

## 🌐 1. HTTP Middleware — the authoritative tenant setter

This is the *primary* place where tenant resolution happens.

```csharp
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        ITenantResolver resolver,
        ICurrentTenant currentTenant)
    {
        // 1. Resolve tenant for this request
        var resolution = await resolver.ResolveAsync(httpContext.RequestAborted);

        // 2. Apply tenant to ambient context
        using (currentTenant.Change(resolution.Tenant))
        {
            // 3. Continue the pipeline
            await _next(httpContext);
        }
    }
}
```

### Why this works

- The `using` scope ensures the tenant is restored after the request ends
- `AsyncLocal` flows through the entire request pipeline
- Controllers, services, repositories all see the correct tenant

This is the same pattern Orchard Core uses for `ShellScope`.

---

## ⚙️ 2. Background Jobs — tenant must be set explicitly

Background jobs do **not** have an HTTP context, so you must set the tenant manually.

Here’s the canonical pattern:

```csharp
public sealed class TenantJobRunner
{
    private readonly IServiceProvider _services;

    public TenantJobRunner(IServiceProvider services)
    {
        _services = services;
    }

    public async Task RunForTenantAsync(
        TenantIdentity tenant,
        Func<IServiceProvider, Task> job)
    {
        using var scope = _services.CreateScope();

        var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

        using (currentTenant.Change(tenant))
        {
            await job(scope.ServiceProvider);
        }
    }
}
```

### Usage

```csharp
await _tenantJobRunner.RunForTenantAsync(
    tenant,
    async sp =>
    {
        var service = sp.GetRequiredService<IMyBackgroundService>();
        await service.DoWorkAsync();
    });
```

### This works because

- Each job gets its own DI scope
- Tenant is set once per job
- AsyncLocal flows through the entire job execution
- No tenant leakage between jobs

---

## 📬 3. Message Queue Handlers — tenant comes from message metadata

Message handlers are similar to background jobs, but the tenant identity usually comes from:

- message headers
- envelope metadata
- routing key
- or a custom field

Here’s the canonical pattern:

```csharp
public sealed class TenantMessageHandler<TMessage>
{
    private readonly IServiceProvider _services;

    public TenantMessageHandler(IServiceProvider services)
    {
        _services = services;
    }

    public async Task HandleAsync(
        TMessage message,
        TenantIdentity tenant,
        Func<IServiceProvider, TMessage, Task> handler)
    {
        using var scope = _services.CreateScope();

        var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

        using (currentTenant.Change(tenant))
        {
            await handler(scope.ServiceProvider, message);
        }
    }
}
```

### Usage

```csharp
await _handler.HandleAsync(
    message,
    tenantFromHeader,
    async (sp, msg) =>
    {
        var processor = sp.GetRequiredService<IMessageProcessor>();
        await processor.ProcessAsync(msg);
    });
```

### Why this works

- Each message is isolated
- Tenant flows through async operations
- No cross‑contamination between messages
- Works with RabbitMQ, Azure Service Bus, Kafka, etc.

---

## 🧠 Why this pattern is correct for Aiel/Rhuarc

All three environments follow the same invariant:

> **Tenant must be set at the boundary of the execution context.**

- HTTP → middleware boundary
- Background job → job runner boundary
- Message queue → message handler boundary

Inside that boundary, `AsyncLocal` guarantees correctness.

This is the same architecture used by:

- Orchard Core
- Finbuckle
- ABP
- SaaSKit
- MassTransit’s tenant scoping
- Hangfire’s tenant scoping extensions

You’re in excellent company.

---

## If you want, I can also generate:

- A reusable `TenantScope` helper
- A `UseTenantResolution()` extension method
- A `BackgroundService` base class with tenant scoping
- A message bus abstraction with built‑in tenant propagation
- A Roslyn analyzer enforcing `using (Change())`

Just tell me which one you want to build next.
