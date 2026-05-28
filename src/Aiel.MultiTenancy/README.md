# Aiel.MultiTenancy

Multi-tenancy contracts for tenant-scoped entities, current-tenant resolution, and trust-boundary constants.

## Public Contract Surface

### Core Identity

| Type | Kind | Purpose |
| --- | --- | --- |
| `TenantId` | `readonly record struct` | Guid-backed strong identifier. Implements `IStrongId<Guid>`. |
| `TenantIdentity` | `sealed record` | Resolved tenant identity: `TenantId` + optional `HostHint` routing hint. |

### Resolution Contract

| Type | Kind | Purpose |
| --- | --- | --- |
| `TenantResolution` | abstract record (DU) | Discriminated union of resolution outcomes. |
| `TenantResolution.Resolved` | sealed record | Tenant resolved; carries `TenantIdentity`. |
| `TenantResolution.Missing` | sealed record | No tenant signal in the current context. |
| `TenantResolution.Ambiguous` | sealed record | Multiple tenants matched; disambiguation required. |
| `TenantResolution.Rejected` | sealed record | Access denied; carries `TenantRejectionReason`. |
| `TenantResolution.Error` | sealed record | Control-plane resolution fault; carries `TenantResolutionErrorReason`. |

### Reason Codes

| Type | Kind | Purpose |
| --- | --- | --- |
| `TenantRejectionReason` | enum | Typed codes for `Rejected`: `TenantInactive`, `MembershipRevoked`, `TenantMismatch`. |
| `TenantResolutionErrorReason` | enum | Typed codes for `Error`: `MembershipLookupFailed`, `UnexpectedException`. Store-binding failures belong in a downstream runtime binding contract. |

### Service Interfaces

| Interface | Method | Returns |
| --- | --- | --- |
| `ITenantAccessor` | `GetCurrentTenantAsync(CancellationToken)` | `ValueTask<TenantIdentity>` (non-nullable) |
| `ITenantResolver` | `ResolveAsync(CancellationToken)` | `ValueTask<TenantResolution>` (non-nullable) |

### Entity Contract

| Interface | Property | Type |
| --- | --- | --- |
| `IMultiTenant` | `TenantId` | `TenantId` |

### Trust-Boundary Constants

`TenantResolutionConstants` exposes ingress constants only:

| Constant | Value | Usage |
| --- | --- | --- |
| `SubjectClaimType` | `"sub"` | Actor identification from JWT. NOT tenant materialization. |
| `TenantIdOverrideHeaderName` | `"X-Tenant-ID"` | Privileged internal override. Must be stripped at the public edge. |

---

## Aiel Tenancy Model Comparison

Aiel provides two first‑class tenancy models — **Discriminator** and **Database‑per‑Tenant** — and supports **Hybrid** configurations through extension points. These models differ in isolation guarantees, operational characteristics, and long‑term scalability.

| **Model** | **Isolation** | **Cost** | **Operational Complexity** | **Best for** | **Migration Difficulty** |
| --- | ---: | ---: | ---: | --- | ---: |
| **Discriminator** | Low | Low | Low | Lightweight SaaS, internal tools, early‑stage products | Low → Medium |
| **Database per tenant** | High | High | Medium → High | Strong isolation, regulated domains, large tenants | Low → Medium |
| **Hybrid** | Variable | Variable | High | Mixed tenant profiles, cost optimization at scale | Variable |

---

## Discriminator Model

A single database and schema shared by all tenants, with a `tenant_id` column on each table.

### Discriminator Pros

- **Simple to operate**: one database, one schema, unified migrations.
- **Cost‑efficient**: minimal infrastructure footprint.
- **Fast to onboard**: provisioning a new tenant is a metadata operation.
- **Good for early‑stage products**: low operational overhead.

### Discriminator Cons

- **Weak isolation**: all tenant data is co‑located; accidental cross‑tenant access is possible without strict safeguards.
- **Shared blast radius**: a bad migration or query affects all tenants.
- **Limited compliance story**: difficult to meet strong isolation or residency requirements.
- **Migration complexity**: moving from discriminator → database‑per‑tenant requires:
  - Extracting tenant data
  - Creating new databases
  - Re‑testing all data access paths
  - Coordinating cutover without downtime

### Discriminator Best Fit

- Internal enterprise apps  
- Lightweight SaaS  
- Products with low regulatory burden  
- Early‑stage systems prioritizing simplicity  

---

## Database‑per‑Tenant Model

Each tenant receives its own dedicated database instance (or logical database within a cluster).

### Database-per-Tenant Pros

- **Strong isolation**: tenant data is physically separated.
- **Reduced blast radius**: issues affect only one tenant.
- **Per‑tenant backup/restore**: simple, reliable, and fast.
- **Flexible scaling**: tenants can be distributed across clusters or regions.
- **Operational clarity**: each tenant is a self‑contained unit.
- **Future‑proof**: supports data residency, sharding, and per‑tenant encryption.

### Database-per-Tenant Cons

- **Connection overhead**: many databases → many connection pools; requires pooling strategy.
- **Operational automation required**:
  - Provisioning databases
  - Running migrations per tenant
  - Monitoring and backups
- **Higher cost**: more storage and compute overhead per tenant.

### Database-per-Tenant Best Fit

- Regulated domains  
- Systems requiring strong isolation  
- Tenants with large or unpredictable workloads  
- Products expecting long‑term scale and regional distribution  

---

## Hybrid Model

A flexible approach where different tenants use different models (e.g., small tenants on discriminator, large tenants on database‑per‑tenant).

### Hybrid Pros

- **Cost optimization**: small tenants can be packed densely.
- **Scalability**: large tenants can be isolated and scaled independently.
- **Migration flexibility**: tenants can move between models as they grow.
- **Operational adaptability**: supports diverse tenant profiles.

### Hybrid Cons

- **More code paths**: DAL must handle multiple isolation strategies.
- **More complex migrations**: moving tenants between models requires orchestration.
- **Higher testing burden**: every feature must work across multiple tenancy modes.
- **More operational tooling**: provisioning, monitoring, and backups differ per model.

### Hybrid Best Fit

- Platforms serving a wide range of tenant sizes  
- Systems with cost‑sensitive small tenants and high‑value large tenants  
- Products expecting to evolve their isolation strategy over time  

---

## Why Aiel Does Not Provide Schema‑per‑Tenant

Aiel intentionally excludes Schema‑per‑Tenant as a built‑in model due to its poor long‑term characteristics:

- **Operational fragility**: migrations across thousands of schemas are error‑prone.  
- **Difficult restores**: restoring a single tenant requires manual extraction.  
- **Shared blast radius**: all schemas share the same database instance.  
- **Scaling limitations**: one instance must handle all tenants’ workloads.  
- **Misleading isolation**: appears safer than discriminator but shares many of the same risks.  

Aiel’s philosophy of **Safety by Design** means avoiding models that look attractive early but become liabilities at scale. Developers may still implement schema‑per‑tenant using extension points, but Aiel does not endorse it as a recommended pattern.

---

## Summary

- **Discriminator**: simple, cheap, low isolation.  
- **Database‑per‑Tenant**: strong isolation, scalable, operationally heavier but safer.  
- **Hybrid**: flexible but complex; best for platforms with diverse tenant needs.  
- **Schema‑per‑Tenant**: intentionally excluded to avoid long‑term operational pitfalls.
