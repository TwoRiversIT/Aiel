# Multitenant Settings System Design

## Status

Planned

---

## Authority Notice

The Aiel codebase remains the authoritative specification for implemented behavior. This document captures the intended design, scope, and sequencing for the settings feature until the runtime packages and tests become the source of truth.

---

## Problem Statement

Aiel has clear patterns for multitenancy, dependency configuration, strong typing, and explicit application boundaries, but it does not yet provide a first-class runtime settings system.

The missing capability is not simple bootstrap configuration. Aiel already supports configuration and options for startup concerns. What is missing is a structured runtime settings model that can:

- define settings explicitly
- resolve values through ordered providers
- support multitenant overrides
- support actor-specific overrides where appropriate
- validate values against known definitions
- audit changes deterministically
- remain compatible with Aiel’s explicit, strongly typed design philosophy

The goal is to build a settings subsystem inspired by ABP’s provider model while aligning with Aiel’s architectural standards: explicit contracts, strong types, deterministic behavior, no hidden magic, and no loosely structured runtime key/value sprawl.

---

## Scope

This phase defines the core runtime settings family only.

Included in scope:

- compile-time setting definitions
- definition registration and discovery
- provider-based value resolution
- host, tenant, and actor-aware overrides
- explicit read and write management APIs
- audit history for setting mutations
- storage-agnostic contracts
- first persistence slice designed for database-per-tenant deployments
- focused unit and integration test coverage
- feature design documentation

Excluded from scope:

- dynamically created runtime setting definitions
- UI or admin pages
- ASP.NET Core endpoints dedicated to settings
- client-side helpers
- source generators for setting definitions
- analyzers
- rollback and version-history workflows
- advanced migration tooling beyond the first persistence slice

---

## Design Goals

The settings system should:

- keep definitions explicit and known at bootstrap time
- separate setting definitions from stored values
- resolve values through deterministic provider precedence
- fail clearly for unknown definitions or invalid writes
- make scope and provenance visible to callers
- support sensitive settings without leaking raw values through logs or audit trails
- integrate with existing tenant and execution-context abstractions
- remain storage-agnostic at the contract level

The settings system should not:

- devolve into an arbitrary string-to-string store
- hide resolution rules behind conventions
- duplicate multitenancy or execution-context infrastructure
- assume one persistence model for all tenancy topologies

---

## What Is Being Developed

### 1. Settings Package Family

The recommended package family is:

| Package | Purpose |
|---|---|
| `Aiel.Settings.Domain.Shared` | Setting names, scope kinds, provider kinds, value metadata, shared value objects, strong IDs where needed |
| `Aiel.Settings.Domain` | Domain rules for overrides, validation boundaries, audit records, and mutation invariants |
| `Aiel.Settings.Application.Contracts` | Public contracts for definition registry, value resolution, management APIs, provider abstractions, and result models |
| `Aiel.Settings.Application` | Default resolver, manager, precedence orchestration, validation flow, and audit orchestration |
| `Aiel.Settings.EntityFrameworkCore` | EF Core persistence for host and tenant stores, mappings, and registration helpers |
| `Aiel.Settings.Testing` | Fakes, fixtures, and reusable test doubles |

This package shape follows the established cross-cutting feature pattern already used elsewhere in Aiel.

### 2. Explicit Setting Definitions

Settings are predefined, not invented at runtime.

A setting definition should carry:

- canonical setting name
- value kind or type metadata
- default value
- sensitivity classification
- allowed scopes
- allowed providers
- validation rules
- optional descriptive metadata for diagnostics and documentation

Runtime overrides must reference a known definition. Unknown or unregistered settings should fail explicitly.

### 3. Provider-Based Resolution

The settings model uses an ordered provider chain. Each provider contributes either a value or no value. The effective setting value is the first applicable value found in precedence order.

Recommended precedence:

1. Actor or User
2. Tenant
3. Global or Host
4. Configuration
5. Definition Default

This order should be a first-class contract, not an incidental implementation detail.

### 4. Management Surface

The core runtime should support:

- resolve effective value
- resolve value with provenance
- set override for a specific scope or provider
- clear override for a specific scope or provider
- enumerate definitions
- enumerate overrides for a scope

Writes must be explicit about their target scope or provider. Configuration and definition-default providers are read-only and must reject writes.

### 5. Audit History

Successful mutations must produce durable audit records.

Each audit record should capture:

- setting name
- written scope or provider
- actor identity
- tenant context when present
- timestamp
- correlation identifiers
- sanitized or fingerprinted old value
- sanitized or fingerprinted new value

Sensitive settings must not be echoed in raw form where audit or diagnostics would expose them.

### 6. Multitenancy and Storage Model

The contracts should be storage-agnostic. The first persistence slice should, however, be planned around database-per-tenant deployments rather than assuming shared-database discriminator storage is the only model.

The recommended split is:

- a host or control-plane persisted provider for global settings and audit concerns
- a tenant persisted provider for tenant-local overrides
- actor-local overrides that remain tenant-bound when actor scoping is enabled

This keeps the public model compatible with discriminator, database-per-tenant, and hybrid tenancy approaches without baking a storage choice into the core contracts.

---

## Core Design Decisions

### D1. Definitions are compile-time and explicit

Settings are declared by code and registered at bootstrap. Runtime-created definitions are out of scope.

### D2. Definitions and values are separate concepts

A definition describes what a setting is. Providers and stores describe where a value comes from. These concerns should not be merged.

### D3. Provider precedence is contractual

Resolution order is part of the public design and should be validated by tests.

### D4. Writes are scope-specific and explicit

The system should never “guess” where a write belongs. The caller must target a concrete writable scope or provider.

### D5. Actor overrides are tenant-bound in v1

Actor-level settings should remain tenant-bound to avoid cross-tenant ambiguity and keep precedence deterministic.

### D6. Sensitive settings are first-class

Sensitivity is definition metadata, not an afterthought. Masking and non-echoing behavior are required in the first slice.

### D7. Persistence stays behind storage-agnostic contracts

The first EF Core implementation may optimize for database-per-tenant deployments, but the application contracts must remain neutral.

### D8. Edge adapters are deferred

The first slice should prove the core runtime model before adding HTTP, UI, generator, or analyzer layers.

---

## Completion Criteria

This phase is complete when all of the following are true:

- a settings package family exists with dependency direction consistent with Aiel conventions
- public settings vocabulary is frozen by contract tests
- setting definitions are explicit, registered, and discoverable through a registry
- provider precedence is implemented and tested
- unknown definitions and invalid writes fail deterministically
- configuration and default providers reject writes explicitly
- the management API supports resolve, provenance, set, clear, and enumeration flows
- audit records are emitted for successful mutations
- sensitive settings are masked or fingerprinted appropriately
- host and tenant persistence are covered by tests
- actor-scoped override behavior is covered in at least one tenant-aware integration test
- feature documentation reflects the implemented model
- targeted package builds and tests pass

---

## Implementation Plan

### Phase 1. Public vocabulary and contracts

Define the core public types and result models first. Freeze terminology around settings, scopes, providers, provenance, and audit.

### Phase 2. Definition registry

Introduce a registry for explicit setting definitions. Keep registration explicit in this phase rather than generator-driven.

### Phase 3. Provider model

Add provider abstractions and a default orchestrator that resolves values in the defined precedence order.

### Phase 4. Management and audit

Implement explicit set and clear operations, validation flow, provenance reporting, and append-only audit capture.

### Phase 5. Persistence

Add EF Core-backed host and tenant stores behind storage-agnostic contracts, with the first slice designed to work cleanly in database-per-tenant environments.

### Phase 6. Verification slice

Prove the system with a small set of representative predefined settings that exercise:

- default-only behavior
- configuration fallback
- host override
- tenant override
- actor override
- sensitive value handling

### Phase 7. Follow-on work

Defer client adapters, HTTP endpoints, generators, analyzers, rollback workflows, and broader migration tooling until the core runtime model is validated.

---

## Verification Strategy

The first implementation should be validated with:

1. contract tests for public vocabulary and null-free surfaces
2. unit tests for provider precedence
3. unit tests for invalid provider and scope writes
4. unit tests for unknown definitions and type mismatches
5. application tests for resolve, set, clear, and provenance behavior
6. persistence tests for host and tenant stores
7. integration coverage for actor overrides within tenant context

---

## Summary

This settings system should bring runtime configuration into Aiel as a first-class, explicit, multitenant feature. It should preserve Aiel’s design posture by using predefined definitions, deterministic provider precedence, explicit writes, strong typing, auditability, and storage-agnostic contracts, while keeping the first slice narrow enough to validate the model before expanding into adapters and tooling.