# Multitenancy System

## Executive Summary

Aiel provides a flexible, extensible multitenancy framework designed to support a wide range of application requirements while maintaining strong safety guarantees and predictable operational behavior.

**Framework scope:** Aiel supplies **reusable tenant-identity contracts**, **fail-closed HTTP middleware**, and **database-migration primitives**. Applications (like Aviendha) own tenant provisioning, storage topology, and operator policy.

**Primary audience:** backend engineers building multitenant applications on .NET/Aiel.

**What Aiel provides:**

- Tenant-identity resolution contracts (`TenantId`, `TenantIdentity`, `TenantResolution`)
- HTTP middleware for fail-closed tenant-required endpoints
- Entity Framework Core integration and discriminator query filters
- Migration orchestration primitives (per-target retry, telemetry)

**What applications provide:**

- Tenant provisioning and catalog management
- Storage topology and connection resolution
- Actor context and tenant-resolution policy
- Migration batch strategy and per-tenant binding

**See also:**

- [`Aiel.MultiTenancy` README](../../src/Aiel.MultiTenancy/README.md) for public API reference
- [`Aiel.AspNetCore` README](../../src/Aiel.AspNetCore/README.md) for HTTP pipeline usage
- [`Aiel.DataAccess.EntityFrameworkCore` README](../../src/Aiel.DataAccess.EntityFrameworkCore/README.md) for data access patterns

---

## Host vs Tenant

- **Host**: The platform or service provider that owns and operates the multitenant application. The host is responsible for provisioning tenant environments, managing the tenant catalog, and defining resolution policies. The host IS NOT an instance of the application, nor is it the default tenant.

- **Tenant**: An individual customer or client of the multitenant application. Each tenant operates within its own isolated context, as defined by the tenancy model in use.

## Goals and Non‑Goals

### Goals (Framework Layer)

- Define reusable tenant-identity contracts that applications can build on.
- Provide fail-closed HTTP middleware that enforces explicit tenant resolution outcomes.
- Offer Entity Framework Core integration for discriminator tenancy models.
- Provide migration primitives (per-target retry, telemetry, retry orchestration).
- Keep tenant identity free of storage details, provisioning logic, or application-specific policy.

### Non‑Goals (Application Responsibility)

- Aiel does not provide admin tooling, CLI, or UI for tenant provisioning.
- Aiel does not own tenant catalog schema, connection string resolution, or storage topology mapping.
- Aiel does not impose actor identity or authentication models—applications define tenant-resolution policy.
- Aiel does not provide a universal ORM that abstracts all models automatically.
- Aiel does not manage migrations at scale; applications own batch strategy and per-tenant binding.

---

## Supported Tenancy Models

Aiel includes two first‑class tenancy models — **Discriminator** and **Database‑per‑Tenant** — and supports **Hybrid** configurations through well‑defined extension points. Schema‑per‑Tenant is intentionally excluded to uphold Aiel's *Safety by Design* philosophy.

| **Model** | **Isolation** | **Cost** | **Operational Complexity** | **Best for** | **Migration Difficulty** |
| --- | ---: | ---: | ---: | --- | ---: |
| **Discriminator** | Low | Low | Low | Small tenants; cost‑sensitive workloads | Low → Medium |
| **Database per tenant** | High | Medium → High | Medium → High | Strong isolation; regulated workloads | Low → Medium |
| **Hybrid** | Variable | Variable | High | Mixed tenant profiles | Variable |

---

## 1. Discriminator Model (Shared Database, Shared Schema)

All tenants share a single database and schema. Each table includes a `tenant_id` column, and Aiel enforces tenant scoping through query filters, middleware, and data‑access patterns.

### Discriminator Characteristics

- Isolation: Low
- Cost: Low
- Operational Complexity: Low
- Migration Difficulty: Low → Medium

### Discriminator Best Fit

- Lightweight SaaS applications
- Internal enterprise tools
- Early‑stage products
- Low‑regulation environments

### Discriminator Pros

- Simple to operate
- Minimal infrastructure footprint
- Fast tenant onboarding
- Unified migrations

### Discriminator Cons

- Weak isolation
- Shared blast radius
- Limited compliance story
- Migration to stronger isolation requires data extraction and re‑testing

---

## 2. Database‑per‑Tenant Model

Each tenant receives its own dedicated database instance or logical database within a cluster.

### Database‑per‑Tenant Characteristics

- Isolation: High
- Cost: Medium → High
- Operational Complexity: Medium → High
- Migration Difficulty: Low → Medium

### Database‑per‑Tenant Best Fit

- Strong isolation requirements
- Regulated or high‑sensitivity domains
- Large or unpredictable workloads
- Multi‑region or residency‑aware deployments

### Database‑per‑Tenant Pros

- Strong isolation
- Reduced blast radius
- Per‑tenant backup/restore
- Flexible scaling
- Future‑proof for residency and encryption

### Database‑per‑Tenant Cons

- Many databases → many connection pools
- Requires provisioning and migration automation
- Higher storage and compute overhead

---

## 3. Hybrid Model (Mixed Isolation Strategies)

Hybrid deployments allow different tenants to use different tenancy models.

### Hybrid Characteristics

- Isolation: Variable
- Cost: Variable
- Operational Complexity: High
- Migration Difficulty: Variable

### Hybrid Best Fit

- Platforms with diverse tenant sizes
- Cost‑optimized deployments
- Systems requiring per‑tenant customization

### Hybrid Pros

- Cost optimization
- Scalability
- Migration flexibility
- Operational adaptability

### Hybrid Cons

- More code paths
- Higher testing burden
- More complex migrations
- Additional operational tooling

---

## Why Aiel Does Not Include Schema‑per‑Tenant

Schema‑per‑Tenant is intentionally excluded due to:

- Operational fragility
- Difficult per‑tenant restores
- Shared blast radius
- Scaling limitations
- Misleading isolation guarantees

Developers may implement it using extension points, but it is not recommended.

---

## Aiel Tenant-Identity Contract (Landed in #16–#19)

The Aiel framework supplies a reusable tenant-identity contract surface that applications build on. **This is Aiel's stable API; applications own resolution policy and provisioning.**

### Core Types

- **`TenantId`**: A strong Guid-backed identifier for a tenant.
- **`TenantIdentity`**: A resolved tenant (TenantId + optional HostHint for routing).
- **`TenantResolution`**: A discriminated union capturing resolution outcomes:
  - `Resolved` — tenant matched; carries TenantIdentity
  - `Missing` — no signal in context
  - `Ambiguous` — multiple tenants matched; needs disambiguation
  - `Rejected` — access denied (carries reason: TenantInactive, MembershipRevoked, TenantMismatch)
  - `Error` — resolution fault (carries reason: MembershipLookupFailed, UnexpectedException)

### Service Interfaces

- **`ITenantAccessor`**: `GetCurrentTenantAsync(CancellationToken) → ValueTask<TenantIdentity>`
  Registered by Aiel.AspNetCore; returns the current request's resolved tenant. Only callable on resolved-tenant paths.

- **`ITenantResolver`**: `ResolveAsync(CancellationToken) → ValueTask<TenantResolution>`
  Application-implemented; runs once per HTTP request to compute resolution outcome.

- **`ICurrentTenant`**: `Current -> TenantIdentity?`
  Synchronous accessor for tenant identity; returns null if not resolved. Useful in non-Async contexts. Also allows temporarily changing the current tenant (e.g., for background jobs or impersonation).

### HTTP Pipeline Integration

Aiel.AspNetCore supplies:

- **`UseAielTenantResolution()`**: Middleware that calls `ITenantResolver` once per request and stores the outcome.
- **`RequireTenant()`**: Endpoint metadata that marks a route as tenant-required. Requests with non-Resolved outcomes get fail-closed responses.
- **`GetTenantResolution(HttpContext)`**: Safe accessor for all five resolution outcomes (safe to call on any endpoint).
- **`AddAielTenantAccess()`**: Registers the ASP.NET-backed `ITenantAccessor` for use in handlers.

### Trust Boundaries

Aiel defines two ingress constants:

- **`SubjectClaimType = "sub"`**: JWT claim for actor identification (NOT for tenant materialization).
- **`TenantIdOverrideHeaderName = "X-Tenant-ID"`**: Privileged internal override (must be stripped at public edge).

**Trust rule:** `sub` identifies the actor. Applications resolve the active tenant based on actor context, optionally using host/domain hints. Public X-Tenant-ID headers are forbidden.

---

## Data Access Patterns

### Discriminator Model (Shared Database, Shared Schema)

All tenants share one database and schema. Each table includes a `tenant_id` column.

**In Aiel:**

- `AielDbContext` (formerly TrDbContext) accepts a `TenantIdentity` or `ITenantResolver` in its constructor.
- Entities implementing `IMultiTenant` get automatic query filters (only return rows matching current tenant) and save-time stamping.
- Fail-closed: non-resolved tenants cannot read or write `IMultiTenant` data.

**Application responsibility:**

- Decide which entities are `IMultiTenant` (tenant-scoped) vs. control-plane (shared, e.g., billing).
- Implement actor-to-tenant policy that maps authenticated users to tenants.
- Provision tenant metadata (name, domain, status).

### Database-per-Tenant Model

Each tenant has a dedicated database. Connection resolution happens *after* tenant identity is known.

**In Aiel:**

- `AielDbContext` still applies query filters and save-time stamping for `IMultiTenant` entities.
- Migration primitives (`IDatabaseMigrator`, `DatabaseMigratorBase`, `DbContextMigrator<T>`) provide per-target retry, telemetry spans, and execution-strategy support.
- No batching or orchestration; applications own batch strategy.

**Application responsibility:**

- Resolve connection strings based on TenantId (look up tenant storage binding in catalog).
- Provision databases per tenant.
- Define batch strategy for large migrations (e.g., migrate N tenants at a time, with checkpointing).
- Handle per-tenant backup/restore and failover.

### Hybrid Model

Applications mix discriminator and database-per-tenant tenants by having per-tenant provisioning decide the storage model and connection strategy.

**In Aiel:**

No special support; both models use the same query filters and migration primitives.

**Application responsibility:**

- Store model choice in tenant metadata (catalog).
- Route connection resolution based on model choice.
- Define migration batch strategy that respects model diversity.

---

## Extension Points for Applications

Applications provide:

1. **`ITenantResolver` implementation**: Compute TenantResolution from HttpContext (actor context, host, domain, X-Tenant-ID override).
2. **Catalog/metadata store**: Map TenantId to domain, model, connection details, status.
3. **Connection resolution**: After TenantIdentity is known, resolve connection string or use the discriminator database.
4. **Migration orchestration**: Define how many tenants to migrate concurrently, with what retry policy and batch checkpoint.
5. **Secrets management**: Store and retrieve connection strings, encryption keys, and per-tenant credentials.

---

## Excluded: Intentional Non-Goals

Aiel deliberately does not provide:

- **Admin UI/CLI** for tenant provisioning or management.
- **Tenant catalog schema** or persistence layer (applications own this).
- **Storage topology mapping** or connection pooling logic.
- **Actor authentication models** (applications define "who can access which tenant").
- **Per-tenant backup/restore orchestration** (applications own this).
- **Tenant migration orchestration** at scale (applications own batch strategy and risk management).
- **Schema-per-Tenant as a first-class model** (operationally fragile; see *Why Aiel Does Not Include Schema‑per‑Tenant*).

---

## Getting Started

See the **README files** for practical quick-start guides:

- **`Aiel.MultiTenancy` README**: Public contract surface, outcome types, constants
- **`Aiel.AspNetCore` README**: HTTP middleware, fail-closed endpoints, tenant-required metadata
- **`Aiel.DataAccess.EntityFrameworkCore` README**: AielDbContext, query filters, migration pipeline, seeding

---

## For Aviendha: A Reference Implementation

Aviendha (the validation application) demonstrates how to build on Aiel:

1. Define a tenant catalog schema and provisioning workflow.
2. Implement `ITenantResolver` that maps actor claims (JWT `sub`) to active tenants.
3. For discriminator tenants: connect to a shared database.
4. For database-per-tenant: resolve connection strings from the catalog.
5. Use `AielDbContext` for data access (both models work with the same ORM integration).
6. Orchestrate migrations through `IDatabaseMigrator` with custom batch and retry logic.

See `phase-03-aiel-aspnet-operational-plan.md` for Aviendha's operational design.

---

## Summary

Aiel is a reusable, unopinionated **tenant-identity framework**—not a full provisioning or operations platform. Applications own:

- Tenant catalog and metadata storage
- Actor-to-tenant resolution policy
- Connection/storage binding
- Migration orchestration and risk management
- Backup, restore, and operational workflows

Aiel owns:

- Tenant-identity contracts and fail-closed semantics
- HTTP middleware for tenant-required routes
- Entity Framework Core discriminator support and query filters
- Migration retry and telemetry primitives
