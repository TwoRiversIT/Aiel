# Strong ID First-Class Feature Extraction Plan

## Status

**Exploratory draft - prerequisite for permission system design**

---

## Background

Aiel already has the core shape of Strong ID support:

- `IStrongId` and `IStrongId<TValue>` live in `Aiel.Domain`.
- `StrongIdAttribute<TValue>` and `StrongIdBackingKind` live in `Aiel.Domain`.
- `StrongIdSourceGenerator` lives in `Aiel.Framework.Generators`.
- Strong ID diagnostics currently live in shared Roslyn descriptor files used by the generator and analyzer projects.
- `AggregateRoot<TKey>`, `Entity<TKey>`, and `IRepository<TEntity, TId>` already constrain identifiers to `IStrongId`.
- `DomainPrimitives-StrongID.md` already defines the intended authoring model and generator contract.

That is a good starting point, but it is not yet a first-class feature. The current placement makes
Strong ID look like a domain primitive owned by `Aiel.Domain`, while the required generator is
owned by a broad `Aiel.Framework.Generators` package. Consumers can see the attribute and interfaces without
necessarily receiving the generator that makes the recommended declaration shape work.

The root `Aiel` package currently packs `Aiel.Framework.Analyzers` into `analyzers/dotnet/cs` and
references `Aiel.Framework.Analyzers` as an analyzer project during local builds. It does not pack or
reference `Aiel.Framework.Generators` in the same way. That matters because Strong ID authoring depends on
generation, not only analysis.

If permissions are going to use Strong IDs for grant IDs, scope keys, subject keys, actor IDs,
tenant IDs, and capability snapshot IDs, Strong ID needs to become a stable feature package family
before the permission packages are implemented.

---

## Design Thesis

Strong ID is not just a convenience generator. It is the framework's identity type system.

Aiel should treat Strong ID as a first-class foundation feature with its own contracts, generators,
analyzers, persistence integrations, and package delivery story. Domain, permissions, multi-tenancy,
application contracts, API clients, and persistence packages should all depend on the same identity
abstraction rather than redefining identifier patterns locally.

The feature should make this shape the normal path:

```csharp
using Aiel.StrongIds;

[StrongId<Guid>(DisallowDefault = true)]
public readonly partial record struct AppointmentId;
```

The consumer should receive the required generator and analyzers by referencing the Strong ID package.
A project should not need to separately discover that `Aiel.Framework.Generators` is required.

---

## Goals

1. Promote Strong ID contracts out of `Aiel.Domain` into a dedicated foundation package.
2. Ship the Strong ID generator and analyzers with the Strong ID package that exposes the attribute.
3. Keep Strong ID independent of domain, application, infrastructure, ASP.NET Core, and EF Core concerns.
4. Preserve the current generator-first authoring model.
5. Support explicit, non-default construction semantics for `Guid`, `int`, `long`, and `string` backing values.
6. Provide analyzer coverage for invalid declarations and unsafe usage patterns.
7. Provide optional integration packages for EF Core, ASP.NET Core, System.Text.Json, and Dapper.
8. Make package dependencies clear so downstream features can depend on Strong ID without depending on all of Aiel.
9. Avoid breaking consumers silently; any namespace or package move requires an explicit migration plan.
10. Keep `Aiel.IdGeneration` separate from Strong ID, while allowing optional integration later.

---

## Non-Goals

1. Do not merge `Aiel.IdGeneration` into Strong ID.
2. Do not make Strong ID depend on `Aiel.Domain`.
3. Do not make Strong ID depend on EF Core, ASP.NET Core, Dapper, or JSON packages.
4. Do not support composite IDs in the first extraction.
5. Do not support arbitrary custom validation expressions in the first extraction.
6. Do not introduce implicit primitive-to-ID conversions.
7. Do not use runtime assembly scanning as the primary integration mechanism.

---

## Target Layer and Package Ownership

Strong ID belongs below domain. It is a shared foundation primitive used by multiple layers.

Recommended package family:

| Package | Responsibility |
|---|---|
| `Aiel.StrongIds` | Runtime contracts, attributes, backing-kind enum, core helper abstractions, and packaged Strong ID generator/analyzers |
| `Aiel.StrongIds.Generators` | Dedicated source generator implementation and generator-only diagnostics |
| `Aiel.StrongIds.Analyzers` | Usage analyzers that do not require generation, such as default usage and `.Value` access rules |
| `Aiel.StrongIds.EntityFrameworkCore` | EF Core value converters, value comparers, model-builder extensions, and generated registration helpers |
| `Aiel.StrongIds.AspNetCore` | Route binding, parameter binding, and model binding helpers |
| `Aiel.StrongIds.SystemTextJson` | JSON converter factory and optional generated converter registration |
| `Aiel.StrongIds.Dapper` | Dapper type handlers and explicit registration helpers |
| `Aiel.StrongIds.Testing` | Test helpers, sample IDs, invalid-value factories, and assertion helpers |

The first milestone does not have to create every integration package. The important boundary is that
the base `Aiel.StrongIds` package owns the authoring contract and delivers the compile-time tooling
required by that contract.

---

## Dependency Direction

The desired direction is:

```text
Aiel.Domain -> Aiel.StrongIds
Aiel.Application -> Aiel.Domain -> Aiel.StrongIds
Aiel.DataAccess.EntityFrameworkCore -> Aiel.StrongIds.EntityFrameworkCore -> Aiel.StrongIds
Aiel.Authorization.* -> Aiel.StrongIds
```

`Aiel.StrongIds` MUST NOT depend on `Aiel.Domain`. Otherwise Strong ID remains a domain-owned
concept rather than a foundation feature.

The root `Aiel` package MAY reference or include Strong ID as a convenience, but downstream
packages that need the contract should reference `Aiel.StrongIds` directly. That keeps feature
dependencies visible and avoids relying on a broad metapackage.

---

## Package Delivery Model

Strong ID declarations cannot work correctly unless the generator is available at compile time. The
package that exposes `[StrongId<TValue>]` should therefore also deliver the relevant generator and
analyzer assets.

Preferred model:

1. `Aiel.StrongIds` contains runtime contracts and packs the dedicated Strong ID generator and
    analyzers under `analyzers/dotnet/cs`.
2. `Aiel.StrongIds.Generators` and `Aiel.StrongIds.Analyzers` remain separately packable for
    local development, diagnostics testing, and advanced users.
3. `Aiel` may include `Aiel.StrongIds` as a dependency or repack its analyzer assets, but the
    direct Strong ID package remains the authoritative delivery unit.

This differs from the current root package shape where `Aiel` explicitly packs `Aiel.Framework.Analyzers`,
while `Aiel.Framework.Generators` remains separate. That current shape is not sufficient for first-class
Strong ID because the generator is required for the recommended authoring model.

Open packaging decision: decide whether `Aiel.StrongIds` directly includes the generator/analyzer
DLLs from sibling projects or whether it depends on a tooling package with analyzer assets. The
direct-include model is simpler for consumers but requires careful project ordering in local builds.

---

## Runtime Contract

The base contract should move from `Aiel.Domain` to `Aiel.StrongIds`.

```csharp
namespace Aiel.StrongIds;

public interface IStrongId;

public interface IStrongId<TValue> : IStrongId
{
    TValue Value { get; }
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class StrongIdAttribute<TValue> : Attribute
{
    public bool DisallowDefault { get; init; } = true;

    public StrongIdBackingKind BackingKind { get; init; } = StrongIdBackingKind.Value;

    public bool GenerateTryFrom { get; init; } = true;
}

public enum StrongIdBackingKind
{
    Value,
    Reference,
}
```

Open namespace decision: the cleanest namespace is `Aiel.StrongIds`. Keeping `Aiel.Domain` would
reduce immediate churn but would preserve the wrong ownership signal. Since Aiel is greenfield and
already has a zero-technical-debt posture, the plan should prefer the clean namespace and migrate now.

---

## Generator Contract

The existing generator contract remains the foundation:

```csharp
[StrongId<Guid>(DisallowDefault = true)]
public readonly partial record struct OrderId;
```

The generator emits:

- `Value`
- a validating constructor for value-backed IDs
- a private validating constructor for reference-backed IDs
- `From(TValue value)`
- `TryFrom(TValue value, out TStrongId id)` when enabled
- `IsDefault`
- `ToString()`

The extracted generator should change its metadata lookups from:

```text
Aiel.Domain.StrongIdAttribute`1
Aiel.Domain.IStrongId<TValue>
```

to:

```text
Aiel.StrongIds.StrongIdAttribute`1
Aiel.StrongIds.IStrongId<TValue>
```

The generator should continue to reject:

- non-partial declarations
- non-record declarations
- positional record syntax
- mismatched `IStrongId<TValue>` backing type
- user-declared `Value`
- user-declared instance constructors
- unsupported backing value types

Generator tests should move from the broad `Aiel.Framework.Generators.UnitTests` project into a dedicated
`Aiel.StrongIds.Generators.UnitTests` project, or at least into a dedicated test folder if the test
project split is deferred.

---

## Analyzer Contract

The first-class feature should separate generator diagnostics from usage analyzers.

Generator diagnostics protect declaration shape:

| Diagnostic | Purpose |
|---|---|
| `AIEL00013` | Strong ID declarations must be partial record types |
| `AIEL00014` | Strong ID declarations must not use positional record syntax |
| `AIEL00015` | Strong ID declarations must implement `IStrongId<TValue>` with the same backing type |
| `AIEL00016` | Strong ID declarations must not declare their own `Value` member |
| `AIEL00017` | Strong ID declarations must not declare instance constructors |
| `AIEL00018` | Unsupported strong ID backing type |

Usage analyzers protect architectural rules:

| Diagnostic | Purpose | Default Severity |
|---|---|---|
| `TRSI0001` | `default(TStrongId)` usage detected | Warning or Error by config |
| `TRSI0002` | `new TStrongId()` usage detected | Error |
| `TRSI0003` | Strong ID `.Value` access from domain assemblies or domain namespaces | Warning by default; configurable as Error |
| `TRSI0004` | Primitive ID used where a Strong ID is expected by convention | Warning |
| `TRSI0005` | Public API exposes primitive identifier with `Id` suffix | Warning |

The `.Value` rule should not pretend the CLR can enforce layer-specific visibility. It should use
analyzers and project conventions:

- Flag `.Value` access in domain projects and domain namespaces.
- Allow `.Value` access in application DTO mapping, persistence mapping, serializers, diagnostics,
  and infrastructure adapters.
- Respect normal Roslyn suppression through `[SuppressMessage]` with a non-empty justification.

---

## EF Core Integration

EF Core integration should be optional and explicit.

Recommended first shape:

```csharp
builder.Property(order => order.Id)
    .HasStrongIdConversion<OrderId, Guid>();
```

The EF package should provide:

- value converters for supported backing types
- value comparers for value-backed IDs
- model-builder or property-builder extension methods
- explicit registration helpers for generated Strong ID metadata
- tests against at least SQLite or InMemory plus relational metadata checks

A later milestone can add generated model registration:

```csharp
modelBuilder.ConfigureStrongIds(strongIds =>
{
    strongIds.AddOrderStrongIds();
});
```

The integration should not silently scan all assemblies at runtime. If scanning is offered for
convenience, it should be opt-in and clearly documented as a startup cost and behavior choice.

---

## ASP.NET Core Integration

ASP.NET Core integration should make Strong IDs work naturally in route values, query strings, and model binding.

Potential first shape:

```csharp
app.MapGet("/orders/{orderId}", (OrderId orderId) => ...);
```

Possible implementation options:

1. Generate `IParsable<TStrongId>` or `ISpanParsable<TStrongId>` implementations for supported backing types.
2. Generate or provide `TypeConverter` support.
3. Provide ASP.NET Core model binders in `Aiel.StrongIds.AspNetCore`.

Preferred first step: generate parsing members for supported backing types if that can be done
without expanding the public surface too much. Model binders can then be an optional adapter rather
than the only path.

---

## System.Text.Json Integration

JSON support should be explicit. Strong ID values are often part of application contracts and
generated clients, so a first-class feature needs a JSON plan even if it is not milestone one.

Options:

1. Generate a nested converter per Strong ID type.
2. Provide a reflection-based `JsonConverterFactory` in `Aiel.StrongIds.SystemTextJson`.
3. Generate a converter registration helper from Strong ID metadata.

Preferred staged approach:

- v1: no automatic JSON converter generation in the base package.
- v2: add `Aiel.StrongIds.SystemTextJson` with explicit options registration.
- v3: consider generated registration helpers if client contract generation needs them.

This keeps the base package small and avoids surprising serialization behavior.

---

## Relationship To Id Generation

`Aiel.IdGeneration` and `Aiel.StrongIds` solve different problems.

| Feature | Responsibility |
|---|---|
| `Aiel.IdGeneration` | Create unique scalar values such as sequential GUIDs, keys, and time-based IDs |
| `Aiel.StrongIds` | Wrap scalar values in type-safe identifiers with validation and analyzer support |

They should remain separate packages.

Optional future integration could add factory helpers such as:

```csharp
var id = OrderId.From(guidGenerator.NewGuid());
```

or generated convenience methods:

```csharp
var id = OrderId.New(guidGenerator);
```

That integration should not be part of the initial extraction. Strong ID should not need an ID generator to exist.

---

## Migration Plan

### Phase 1: Create package boundaries

1. Add `src/Aiel.StrongIds/Aiel.StrongIds.csproj`.
2. Move `IStrongId`, `IStrongId<TValue>`, `StrongIdAttribute<TValue>`, and
    `StrongIdBackingKind` into `Aiel.StrongIds`.
3. Add `src/Aiel.StrongIds.Generators/Aiel.StrongIds.Generators.csproj`.
4. Move `StrongIdSourceGenerator` into the dedicated generator project.
5. Add `src/Aiel.StrongIds.Analyzers/Aiel.StrongIds.Analyzers.csproj` when usage analyzers begin.
6. Add all new projects to `Aiel.slnx`.

### Phase 2: Update references

1. Update `Aiel.Domain` to reference `Aiel.StrongIds`.
2. Update aggregate, entity, and repository constraints to import the new namespace.
3. Update generator metadata names from `Aiel.Domain` to `Aiel.StrongIds`.
4. Update tests to use `using Aiel.StrongIds;`.
5. Update docs that currently identify Strong ID as owned by `Aiel.Domain`.

### Phase 3: Package compile-time tooling

1. Ensure `Aiel.StrongIds` packs `Aiel.StrongIds.Generators.dll` under `analyzers/dotnet/cs`.
2. Ensure `Aiel.StrongIds` packs `Aiel.StrongIds.Analyzers.dll` under `analyzers/dotnet/cs`
    when that project exists.
3. Decide whether the root `Aiel` package also includes or depends on `Aiel.StrongIds`.
4. Add package tests that verify a consuming project can reference only `Aiel.StrongIds` and
    successfully compile a generated Strong ID.

### Phase 4: Add first integration package

1. Add `Aiel.StrongIds.EntityFrameworkCore`.
2. Implement explicit `HasStrongIdConversion<TStrongId, TValue>()` extensions.
3. Add tests for supported backing types.
4. Document exact EF usage.

### Phase 5: Add usage analyzers

1. Implement `default(TStrongId)` detection.
2. Implement `new TStrongId()` detection.
3. Implement `.Value` access detection for domain assemblies and namespaces.
4. Implement primitive public `Id` API warnings after the core migration is stable.

### Phase 6: Add optional transport integrations

1. Add ASP.NET Core binding or generated parsing support.
2. Add System.Text.Json registration support.
3. Add Dapper type-handler support if Dapper repositories need Strong ID mapping.

---

## Compatibility Strategy

This is a public API move because the namespace and assembly for `IStrongId` and
`[StrongId<TValue>]` would change.

Since Aiel is greenfield, the recommended path is to make the breaking move now and avoid carrying
forwarding shims. If compatibility is still desired during transition, use one short-lived bridge
release:

```csharp
namespace Aiel.Domain;

[Obsolete("Use Aiel.StrongIds.StrongIdAttribute<TValue>.")]
public sealed class StrongIdAttribute<TValue> : Aiel.StrongIds.StrongIdAttribute<TValue>;
```

However, the current attribute is sealed, so a type-forwarding or duplicated-attribute bridge would
need careful design. The cleaner path is a direct namespace migration while the framework is still
pre-stable.

Open decision: whether `TypeForwardedTo` is worth the complexity for pre-stable packages. The likely answer is no.

---

## Documentation Updates

Update these documents as part of the extraction:

| Document | Change |
|---|---|
| `docs/DomainPrimitives.md` | State that domain primitives depend on `Aiel.StrongIds` for typed identifiers |
| `docs/DomainPrimitives-StrongID.md` | Move or rewrite as the Strong ID feature spec |
| `docs/ConceptualOverview.md` | Describe Strong ID as a foundation feature, not only a domain primitive |
| `src/Aiel.StrongIds/README.md` | Add package usage, authoring model, supported backing types, and package delivery details |
| `src/Aiel.Domain/README.md` | Mention that entity and aggregate IDs are constrained by `Aiel.StrongIds` |
| `docs/planning/permission-system.md` | Replace the prerequisite note with a link to the Strong ID extraction plan after this plan is accepted |

Do not edit `permission-system.md` while active review is in progress unless requested.

---

## Test Plan

Minimum tests for the extraction:

1. Generator emits expected members for `Guid`, `int`, `long`, and `string` backing types.
2. Generator rejects invalid declaration shapes with stable diagnostics.
3. Generated `From` rejects invalid default values.
4. Generated `TryFrom` returns `false` for invalid default values without throwing.
5. String-backed IDs trim before storage and preserve non-whitespace casing.
6. A consuming project can reference `Aiel.StrongIds` only and compile a generated Strong ID.
7. `Aiel.Domain` compiles against `Aiel.StrongIds` after the namespace move.
8. EF Core conversion helpers map Strong IDs to backing scalar values and back.
9. Usage analyzers flag default construction and prohibited `.Value` access in domain code.
10. Roslyn suppressions work with `[SuppressMessage]` and require useful justifications where configured.

---

## Risks

| Risk | Mitigation |
|---|---|
| Consumers reference the attribute but do not receive the generator | Pack generator/analyzer assets directly in `Aiel.StrongIds` |
| Namespace move breaks existing code | Make the move before stable release and provide mechanical migration guidance |
| Strong ID becomes a dumping ground for ID generation | Keep `Aiel.IdGeneration` separate and define optional integration only |
| EF integration becomes hidden runtime magic | Prefer explicit property-builder extensions first |
| `.Value` access rule becomes too noisy | Start as configurable warning and allow `[SuppressMessage]` with justification |
| Package graph becomes circular | Keep `Aiel.StrongIds` below `Aiel.Domain` and free of domain dependencies |

---

## Current Open Questions

1. Should the base namespace be `Aiel.StrongIds` or `Aiel.Identifiers`?
2. Should `Aiel.StrongIds` pack generator/analyzer DLLs directly or depend on tooling packages?
3. Should the root `Aiel` package include Strong ID as a dependency, a repackaged analyzer asset, or neither?
4. Should generated Strong IDs implement `IParsable<TSelf>` in the first extraction?
5. Should `Aiel.StrongIds.EntityFrameworkCore` be part of the first extraction or a second milestone?
6. Should usage analyzers keep the existing `TRSG` prefix or split into generator `TRSG` and usage `TRSI` prefixes?
7. Should the first migration preserve old `Aiel.Domain` types with forwarding, or make a clean
    breaking namespace move?
8. How should local project references ensure analyzer/generator assets flow during repository
    development and package tests?

---

## Working Verdict

Strong ID should be extracted before the permission feature moves from design to implementation.

The permission system wants Strong IDs everywhere: grant IDs, scope keys, subject keys, actor IDs,
tenant IDs, resource references, capability snapshot IDs, and migration identifiers. Keeping Strong
ID as a partial domain primitive would push technical debt into every permission package. Making it
first-class now gives Aiel one identity story across domain, application, persistence, HTTP, client
generation, and authorization.

The key implementation rule is simple: the package that exposes `[StrongId<TValue>]` MUST deliver
the generator and analyzers required to use it correctly.
