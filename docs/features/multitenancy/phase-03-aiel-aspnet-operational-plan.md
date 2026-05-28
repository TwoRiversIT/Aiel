# Aiel ASP.NET Core Operational Plan

## Status (as of issue #19)

**Partial implementation with ongoing work** (Implementation code is authoritative; this document retained for reference and project history)

---

## Authority Notice

The Aiel codebase (contracts, implementations, and tests) is the authoritative specification for this phase. This document describes the planning context, operational ownership boundaries, and historical rationale for how phase 03 was designed. Refer to the implementation in `Aiel/src/` and test coverage in `Aiel/tests/` as the definitive current behavior.

---

**Landed in #16–#19:**

- ✅ `Aiel.MultiTenancy`: Tenant-identity contracts (`TenantId`, `TenantIdentity`, `TenantResolution`)
- ✅ `Aiel.AspNetCore`: HTTP middleware (`UseAielTenantResolution`, `RequireTenant()`, `ITenantAccessor`)
- ✅ `Aiel.EntityFrameworkCore`: Base context rename (`AielDbContext`), query filters for `IMultiTenant` entities
- ✅ `Aiel.EntityFrameworkCore`: Migration primitives (`IDatabaseMigrator`, `DbContextMigrator<T>`, retry and telemetry)

**Still to implement (Aviendha or application):**

- Tenant catalog schema and provisioning
- `ITenantResolver` implementation (actor-to-tenant policy)
- Connection resolution (storage topology)
- Migration batch orchestration and checkpointing
- Operator commands and health endpoints

---

## Ownership Seam: What Aiel Owns vs. What Applications Own

**Aiel provides:**

- Tenant-identity contracts and fail-closed HTTP semantics
- `AielDbContext` with query filters and save-time stamping
- Migration retry logic and OpenTelemetry spans
- Trust constants (`sub` claim, `X-Tenant-ID` header)

**Applications provide:**

- Tenant catalog and metadata storage
- `ITenantResolver` implementation (policy for mapping actors to tenants)
- Storage topology and connection resolution
- Migration batch strategy and checkpointing
- Backup/restore and operational workflows


---

## Next Steps: What Aviendha (or Applications) Must Build

### 1. Implement `ITenantResolver`

Aviendha must provide a tenant resolver that computes `TenantResolution` from the HTTP context:

```csharp
public sealed class AviendhaResolver(ITenantCatalogService catalog)
    : ITenantResolver
{
    public async ValueTask<TenantResolution> ResolveAsync(CancellationToken cancellationToken)
    {
        // Aviendha's custom logic:
        // 1. Extract actor from authentication (JWT "sub" claim)
        // 2. Determine active tenant from actor context (membership validation)
        // 3. Handle X-Tenant-ID override (internal use only)
        // 4. Return one of five outcomes: Resolved, Missing, Ambiguous, Rejected, Error
    }
}
```

The resolver is registered in DI and runs once per HTTP request. It must never leak exceptions; all failures map to `TenantResolution.Error` or `TenantResolution.Rejected`.

### 2. Build Tenant Catalog Schema

Store tenant metadata:

- `TenantId` (key)
- `Name`, `DomainHint`, `Status` (active/suspended/offboarded)
- `StorageModel` (discriminator / database / hybrid)
- For database-per-tenant tenants: connection details or vault references
- `CreatedAt`, `UpdatedAt` audit fields

No fixed schema imposed by Aiel; Aviendha decides the storage (PostgreSQL table, document store, etc.).

### 3. Resolve Connections

After `TenantIdentity` is known (via `ITenantResolver`), resolve the connection string:

- **Discriminator tenants**: Use the shared database connection.
- **Database-per-tenant tenants**: Look up tenant binding in catalog, retrieve from secrets manager.

Aviendha registers its own `ITenantResolver` and connection logic; Aiel does not manage this.

### 4. Orchestrate Migrations

Aiel provides `IDatabaseMigrator` for single-target migration with retry and telemetry. Aviendha owns:

- Batch strategy (how many tenants concurrently)
- Checkpointing (persist which tenants are done)
- Rollout policy (canary rollout? full batch? one-at-a-time?)
- Operator commands (CLI or job runner to execute migrations)

Recommended approach:

```csharp
public sealed class AviendhaMultiTenantMigrator : BackgroundService
{
    private readonly IMigrationCheckpointService _checkpoints;
    private readonly ITenantCatalogService _catalog;
    private readonly IServiceProvider _sp;
    
    // 1. Fetch pending tenants from checkpoint
    // 2. Batch into groups (e.g., 10 at a time)
    // 3. For each batch:
    //    - Create a scope per tenant
    //    - Resolve connection for that tenant
    //    - Call IDatabaseMigrator.MigrateAsync()
    //    - On success: record checkpoint
    //    - On failure: stop batch, wait for manual operator review
}
```

Migrations run outside the request path (via ServiceHost or deployment job), not during app startup.

---

## Migration Strategy Matrix

| Tenancy Model | Migration Unit | Deploy-Time | Startup-Time | Rollback | Recommendation |
|---|---|---|---|---|---|
| **Shared database, discriminator** | One shared database | Pipeline migrates once before new app nodes take traffic | Startup MAY auto-migrate in local development and tests. Production startup SHOULD only verify the required version. | Easiest rollback when migrations stay backward compatible; defer destructive changes. | Default for lower-isolation systems |
| **Database-per-tenant** | One tenant database at a time + shared catalog | Pipeline migrates catalog first, then tenant databases in controlled batches with resumable checkpointing | Web startup checks catalog reachability and version only. MUST NOT fan out across tenant databases. | Prefer forward-fix plus backup or restore per tenant. Disable traffic for failed tenants without blocking healthy tenants. | For strong isolation and regulated domains |
| **Hybrid** | Mixed per migration policy | Catalog stores model per tenant; route accordingly | Same as the models in use (discriminator check only, or catalog check) | Per-model rollback strategy | Platforms with diverse tenant needs |

---

## Operational Ownership Split

| Owner | Responsibilities |
|---|---|
| **Aiel.AspNetCore** | HTTP middleware for tenant resolution, fail-closed enforcement, `RequireTenant()` metadata, tenant-optional fallback |
| **Aiel.EntityFrameworkCore** | `AielDbContext`, query filters for `IMultiTenant`, migration retry/telemetry, per-target migrator abstraction |
| **Application (Aviendha)** | Tenant catalog schema, `ITenantResolver` implementation, connection resolution, migration batch policy, checkpointing, operator commands, backup/restore workflows |
| **Deploy pipeline** | Execute migrations outside app startup, manage rollout checkpoints, stop on failed batches, verify version compatibility before traffic release |

---

## Safety Rules (Aiel-Enforced)

- **No nullable tenant returns**: `ITenantAccessor` and `ITenantResolver` always return non-null outcomes; errors are explicit.
- **Fail-closed tenant-required endpoints**: If tenant resolution is not `Resolved`, the endpoint rejects the request with a mapped HTTP status.
- **No concurrent migrations on the same target**: Aiel migration primitives assume single-writer per database. Batch orchestration must serialize.
- **Query filters are automatic**: Any entity implementing `IMultiTenant` gets filtered; there is no opt-out path during query execution.

---

## Aviendha Example: Database-Per-Tenant Architecture

### Catalog Database (Shared Control Plane)

```
CREATE TABLE tenants (
  tenant_id UUID PRIMARY KEY,
  name TEXT NOT NULL,
  domain_hint TEXT UNIQUE,
  status TEXT, -- active, suspended, offboarded
  storage_model TEXT, -- discriminator, database
  created_at TIMESTAMPTZ,
  updated_at TIMESTAMPTZ
);

CREATE TABLE tenant_storage_bindings (
  binding_id UUID PRIMARY KEY,
  tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
  storage_model TEXT,
  connection_secret_ref TEXT, -- vault/keyvault reference
  created_at TIMESTAMPTZ
);

CREATE TABLE migration_checkpoints (
  checkpoint_id UUID PRIMARY KEY,
  migration_run_id UUID NOT NULL,
  tenant_id UUID,
  status TEXT, -- pending, in_progress, complete, failed
  started_at TIMESTAMPTZ,
  completed_at TIMESTAMPTZ,
  last_error TEXT,
  UNIQUE(migration_run_id, tenant_id)
);
```

### Aviendha's `ITenantResolver` Pseudocode

```csharp
public async ValueTask<TenantResolution> ResolveAsync(CancellationToken cancellationToken)
{
    // 1. Extract actor from JWT "sub" claim
    var sub = httpContext.User.FindFirst("sub")?.Value;
    if (sub is null)
        return new TenantResolution.Missing();

    // 2. Look up actor in catalog (find actor record + active tenant)
    var actor = await catalog.GetActorAsync(sub, cancellationToken);
    if (actor is null)
        return new TenantResolution.Error(TenantResolutionErrorReason.MembershipLookupFailed);

    // 3. Check if actor's active tenant is still valid
    var tenant = await catalog.GetTenantAsync(actor.ActiveTenantId, cancellationToken);
    if (tenant?.Status != "active")
        return new TenantResolution.Rejected(TenantRejectionReason.TenantInactive);

    // 4. Handle X-Tenant-ID override (internal override for admins)
    if (httpContext.Request.Headers.TryGetValue("X-Tenant-ID", out var overrideHeader))
    {
        if (Guid.TryParse(overrideHeader, out var overrideTenantId))
        {
            if (overrideTenantId != actor.ActiveTenantId)
                return new TenantResolution.Rejected(TenantRejectionReason.TenantMismatch);
        }
    }

    // 5. Return resolved
    return new TenantResolution.Resolved(
        new TenantIdentity(actor.ActiveTenantId, tenant.DomainHint)
    );
}
```

### Aviendha's Connection Resolution

After `TenantIdentity` is known, Aviendha resolves the connection before creating `AielDbContext`:

```csharp
public sealed class AviendhaDbContext : AielDbContext
{
    private readonly ITenantCatalogService _catalog;

    public AviendhaDbContext(
        DbContextOptions<AviendhaDbContext> options,
        ITenantResolver resolver,
        ITenantCatalogService catalog)
        : base(options, resolver)
    {
        _catalog = catalog;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Aiel.AspNetCore middleware already resolved the tenant
        var tenantId = /* from ITenantAccessor */;
        
        // Aviendha looks up connection based on tenant
        var binding = await _catalog.GetStorageBindingAsync(tenantId);
        var connectionString = await _secrets.GetConnectionStringAsync(binding.SecretRef);

        optionsBuilder.UseSqlServer(connectionString);
    }
}
```

---

## Rollout Phases for Aviendha
