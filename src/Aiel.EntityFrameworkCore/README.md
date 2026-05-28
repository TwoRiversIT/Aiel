# Aiel.EntityFrameworkCore

Provides opinionated, production-ready tooling for **migrating** and **seeding** databases with
Entity Framework Core inside the Aiel App Framework module system.

---

## Features

| Feature | Description |
| -------- |---|
| Auto-discovery of migrators | Any `IDatabaseMigrator` registered with DI is automatically enrolled into `AielMigrationOptions`. |
| Retry with jitter | `DatabaseMigratorBase` retries failed migrations up to *n* times (default 3) with a random back-off of 5–15 s. |
| OpenTelemetry tracing | Migration runs emit an `ActivitySource` span (`"Migrations"`) for distributed tracing. |
| Execution-strategy support | `DbContextMigrator<TDbContext>` applies EF Core's resilience execution strategy before migrating. |
| Seeding pipeline | `SeedingExtensions` exposes `SeedAsync` overloads on `IHost`, `IServiceProvider`, and `IServiceScope`. |
| Module integration | `AielEntityFrameworkCore` wires everything up as a Aiel dependency module. |

---

## Installation

You can install the Aiel.EntityFrameworkCore package via NuGet Package Manager Console:

```pwsh
Install-Package Aiel.EntityFrameworkCore
```

Or via .NET CLI:

```pwsh
dotnet add package Aiel.EntityFrameworkCore
```

---

## Quick Start

### 1. Register the module

If you are using the Two Rivers App Framework, the module will get registered by including 
it as a dependency in your app's startup configuration.

```csharp
[DependsOn(typeof(AielEntityFrameworkCore))]
public sealed class MyApplication : AielApplication;
```

If you are not using the full Two Rivers App Framework, you can still leverage the migration and
seeding features by manually registering the module.

```csharp
// Program.cs
builder.AddAielMigrations();
```

### 2. Derive from `AielDbContext` for tenant-aware data access

`AielDbContext` replaces the old `TrDbContext` name. No compatibility shim is provided.
Use one of the supported constructors:

- `base(options, tenantIdentity)` when the tenant is already trusted and resolved.
- `base(options, tenantResolver)` when EF must consume explicit `TenantResolution` outcomes.

```csharp
public sealed class AppDbContext : AielDbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantResolver tenantResolver)
        : base(options, tenantResolver)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
}
```

For `IMultiTenant` entities, query filters fail closed unless the current tenant resolution is
`TenantResolution.Resolved`. `SaveChangesAsync` stamps new entities only for resolved tenants, so
missing or error outcomes cannot leak or create cross-tenant data.

### 3. Add a `DbContextMigrator`

Register your `DbContext` and its migrator in one call. The auto-discovery hook in
`AielEntityFrameworkCore` detects every `IDatabaseMigrator` that is added to DI and
enrolls it automatically.

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

services.AddScoped<IDatabaseMigrator, DbContextMigrator<AppDbContext>>();
```

### 4. Migrate and seed on startup

Call the extension methods **before** `app.Run()`:

```csharp
var app = builder.Build();

await app.MigrateAsync();   // applies pending EF Core migrations
await app.SeedAsync();      // runs IDataSeeder implementations

await app.RunAsync();
```

---

## Migration Pipeline

```
IHost.MigrateAsync()
  └─ IServiceProvider.MigrateAsync()
       └─ IServiceScope.MigrateAsync()
            └─ MigrationManager.MigrateAsync()
                 └─ foreach IDatabaseMigrator in AielMigrationOptions.Migrators
                      └─ IDatabaseMigrator.MigrateAsync()
                           └─ DatabaseMigratorBase.TryAsync()  ← retry + tracing
                                └─ DbContextMigrator<T>.ApplyMigrationsAsync()
```

### `IDatabaseMigrator`

Implement this interface to create a custom migrator for any `DbContext`.

```csharp
public class MyMigrator(IServiceProvider sp)
    : DbContextMigrator<MyDbContext>(sp);
```

### `DatabaseMigratorBase`

Abstract base that provides:

- **`TryAsync`** — wraps a migration task with retry logic and an OpenTelemetry activity span.
  Retries up to `retryCount` times (default `3`) with a random delay of 5–15 seconds between
  attempts. Re-throws on final failure.

### `DbContextMigrator<TDbContext>`

Concrete implementation of `IDatabaseMigrator`. Resolves `TDbContext` from DI, checks for
pending migrations, and applies them via EF Core's execution strategy. Logs migration count,
start, and completion (or `"No migrations to apply"` when up-to-date).

### `MigrationManager`

Orchestrates all registered `IDatabaseMigrator` instances within a new DI scope. Configured
via `AielMigrationOptions`.

### `AielMigrationOptions`

```csharp
services.AddAielMigrations(options =>
{
    // Migrators are normally auto-discovered; manual registration is also supported.
    options.Migrators.Add<MyCustomMigrator>();
});
```

---

## Seeding Pipeline

Seeding delegates to the `IDataSeeder` abstraction (from `Aiel`). Register your seeder
and call `SeedAsync`:

```csharp
services.AddScoped<IDataSeeder, MyDataSeeder>();

// On startup:
await app.SeedAsync();
```

### `SeedingOptions`

Bind from configuration to supply admin bootstrap credentials:

```json
{
  "SeedingOptions": {
    "AdminEmail": "admin@example.com",
    "AdminPassword": "S3cur3!",
    "AdminPermissions": ["read", "write", "admin"]
  }
}
```

```csharp
// Injected wherever needed:
public MySeeder(IOptions<SeedingOptions> opts) { ... }
```

---

## OpenTelemetry

The `DatabaseMigratorBase` emits activities under the source name `"Migrations"`. To capture
them, add the source to your tracer provider:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(DatabaseMigratorBase.ActivitySourceName));
```

---

## Custom Migrator Example

```csharp
// Extend DbContextMigrator<T> to override migration behaviour.
public class TenantDbMigrator(IServiceProvider sp)
    : DbContextMigrator<TenantDbContext>(sp)
{
    protected override async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
    {
        // Custom pre-migration logic here.
        await base.ApplyMigrationsAsync(cancellationToken);
    }
}

// Registration — auto-discovered by AielEntityFrameworkCore.
services.AddScoped<IDatabaseMigrator, TenantDbMigrator>();
```

---

## Dependencies

| Package | Purpose |
|---|---|
| `Microsoft.EntityFrameworkCore` | Core EF abstractions |
| `Microsoft.EntityFrameworkCore.Relational` | Execution strategy and relational migrations API |
| `Aiel` | App Framework module system, `IDataSeeder`, `TypeSet<T>` |
