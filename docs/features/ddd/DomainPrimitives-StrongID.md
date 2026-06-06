# Domain Primitives

## Strongly Typed IDs

The Strong ID runtime contract now lives in `Aiel.StrongIds`. Domain types consume that contract, but Strong ID is no longer owned by `Aiel.Domain`.

A strong ID type should provide these guarantees:

- Non-default semantics for explicit construction.
- Value-based equality and stable hashing.
- No implicit primitive-to-ID conversions.
- A single scalar backing value for persistence, serialization, and machine integration.
- No hidden runtime behavior.

### Scope of responsibility

Strong IDs are technical identifiers for databases, persistence boundaries, machines, and inter-process communication.

They are not human-readable business keys.

If the domain needs a human-readable identifier, booking code, invoice number, slug, or external reference, that must be modeled as a separate field or value object with its own invariants and lifecycle.

Examples:

- `OrderId` is the aggregate identifier used internally and in persistence.
- `OrderNumber` is a separate business identifier shown to users.
- `TenantId` is the internal foreign-key identity.
- `TenantCode` or `DomainName` is the human-facing lookup key.

### Primary constructor limitation

This declaration shape is attractive but is not a good source-generator contract:

```csharp
public readonly partial record struct OrderId(Guid Value);
```

Why it fails as the authoring model:

- The positional record already synthesizes a public constructor before the generator gets involved.
- The generator cannot reliably replace that constructor with a validating one.
- For `struct` types, `default(OrderId)` always exists and bypasses all constructor validation anyway.

The practical consequence is that a primary-constructor declaration cannot be the mechanism that guarantees `Guid.Empty` is rejected.

### Recommended generator-first model

The developer should declare intent only, and the source generator should emit the full implementation.

Recommended authoring shape for a struct-backed strong ID:

```csharp
[StrongId<Guid>(DisallowDefault = true)]
public readonly partial record struct OrderId;
```

Generated implementation shape:

```csharp
public readonly partial record struct OrderId : IStrongId<Guid>
{
    public Guid Value { get; }

    public OrderId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(value));

        Value = value;
    }

    public static OrderId From(Guid value) => new(value);

    public static bool TryFrom(Guid value, out OrderId id)
    {
        if (value == Guid.Empty)
        {
            id = default;
            return false;
        }

        id = new OrderId(value);
        return true;
    }

    public bool IsDefault => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
```

### Struct versus class

`record struct` is the recommended default for strong IDs because it has the right value semantics, avoids allocations, and is friendly to EF Core value conversion.

`record struct` also has one unavoidable limitation:

- `default(TStrongId)` is always possible.

That means the invariant must be stated precisely:

- All explicit construction paths must validate.
- The default value is treated as an invalid, uninitialized sentinel.

If Aiel ever needs an ID type where invalid construction must be impossible rather than merely discouraged and analyzable, use a generated sealed record class instead:

```csharp
[StrongId<Guid>(DisallowDefault = true, BackingKind = StrongIdBackingKind.Reference)]
public sealed partial record OrderId;
```

That allows the generator to expose only factory methods and a private constructor, which gives a stricter invariant at the cost of allocation and slightly more ceremony.

### Enforcement strategy

The design should combine generated code with analyzers.

Domain code MUST treat strong IDs as opaque values.

- Domain logic may compare strong IDs, pass them through method boundaries, and assign them to entities.
- Domain logic should not branch on, serialize, or otherwise depend on the underlying scalar representation.
- Application and infrastructure code may use the underlying value for DTO mapping, persistence, message contracts, and diagnostics.

Because C# does not provide a practical way to expose the underlying scalar to application and infrastructure layers while perfectly hiding it from arbitrary domain consumers, v1 uses analyzers and conventions rather than pretending the language can enforce this boundary by itself.

The generator should emit:

- `Value`
- a validating constructor
- `From`
- `TryFrom`
- `IsDefault`
- `ToString`
- optional parsing and serialization helpers when needed

Analyzers should flag:

- `default(OrderId)`
- `new OrderId()`
- equality checks against `default`
- any primitive-to-ID API shape that bypasses generated factories
- strong-ID `.Value` access from domain assemblies or domain namespaces

### Recommendation

Aiel should standardize on this rule:

- Do not use positional record syntax as the developer-authored source-generator input for strong IDs.
- Use a partial type declaration plus metadata attributes.
- Treat struct-backed strong IDs as validated value types with an invalid default sentinel.
- Reserve class-backed strong IDs for the rare cases where absolute construction safety matters more than allocation cost.

### `StrongIdAttribute` contract

The first generator version should keep the declaration contract narrow and explicit.

Recommended attribute shape:

```csharp
namespace Aiel.StrongIds;

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
    Reference
}
```

Supported authoring shape:

```csharp
[StrongId<Guid>(DisallowDefault = true)]
public readonly partial record struct OrderId;
```

Authoring rules:

- The annotated type must be a partial record struct or a partial sealed record.
- The annotated type must implement `IStrongId<TValue>`.
- The annotated type must not declare a primary constructor.
- The annotated type must not declare its own `Value` member, instance constructor, or conflicting factory methods.
- The generator owns the implementation surface.

Value exposure note:

- In v1, the generated type exposes a public `Value` member because EF Core mapping, serializers, DTO mapping, and message contracts need deterministic access to the backing scalar.
- The architectural rule is that domain code treats `Value` as an escape hatch it must not use.
- This rule is enforced by analyzers rather than by pretending layer-specific visibility exists in the CLR type system.

Initial supported underlying value types:

- `Guid`
- `int`
- `long`
- `string`

Validation rules for v1:

- `Guid` rejects `Guid.Empty` when `DisallowDefault` is `true`.
- `int` and `long` reject `0` when `DisallowDefault` is `true`.
- `string` rejects `null`, `String.Empty`, and whitespace-only values when `DisallowDefault` is `true`.
- For v1, string-backed strong IDs validate with `String.IsNullOrWhiteSpace(value)`, trim before storage, and preserve the original non-whitespace content without normalizing or canonicalizing it.

### Generator contract

For every valid strong-ID declaration, the generator should emit:

- a `Value` property of `TValue`
- a validating constructor when `BackingKind` is `Value`
- a private validating constructor when `BackingKind` is `Reference`
- a `From(TValue value)` factory that throws on invalid input
- a `TryFrom(TValue value, out TStrongId id)` factory when `GenerateTryFrom` is `true`
- `ToString()`, equality, and deconstruction only when not already provided by the chosen record kind
- optional debugger display metadata if it does not add hidden runtime behavior

The generator should not emit:

- implicit conversions from the primitive value type
- parsing from arbitrary strings unless explicitly requested later
- JSON converters in v1
- EF Core converters in v1

### Diagnostics contract

The analyzer and generator should fail fast with explicit diagnostics.

Suggested initial diagnostics:

- `AIEL00013`: Strong ID declarations must be partial record types.
- `AIEL00014`: Strong ID declarations must not use positional record syntax.
- `AIEL00015`: Strong ID declarations must implement `IStrongId<TValue>` with the same `TValue` as the attribute.
- `AIEL00016`: Strong ID declarations must not declare their own `Value` property.
- `AIEL00017`: Strong ID declarations must not declare instance constructors.
- `AIEL00018`: Unsupported strong ID backing type.
- `TRSG0007`: `default(TStrongId)` usage detected.
- `TRSG0008`: Strong ID backing value access is not allowed from domain code.

`TRSG0008` guidance:

- Trigger when code in a domain assembly or under a domain namespace accesses `.Value` on a strong-ID type.
- Do not trigger for generated code.
- Do not trigger in application, infrastructure, serialization, or persistence layers.
- Treat logging, string interpolation, JSON shaping, branching, and primitive comparisons based on `.Value` as violations inside domain code.

Examples that should be flagged in domain code:

```csharp
if (order.Id.Value == Guid.Empty)
{
    throw new DomainException("Order ID is invalid.");
}

var tenantPartition = tenantId.Value.ToString()[..8];
```

Examples that should be allowed outside the domain layer:

```csharp
dto.Id = order.Id.Value;

builder.Property(x => x.Id)
    .HasConversion(id => id.Value, value => OrderId.From(value));
```

### Generated API surface examples

The contract above is easier to implement correctly if the generated API surface is narrow and uniform.

#### Guid-backed strong ID

Authoring input:

```csharp
[StrongId<Guid>(DisallowDefault = true)]
public readonly partial record struct OrderId;
```

Generated shape:

```csharp
public readonly partial record struct OrderId : IStrongId<Guid>
{
    public Guid Value { get; }

    public OrderId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(value));

        Value = value;
    }

    public static OrderId From(Guid value) => new(value);

    public static bool TryFrom(Guid value, out OrderId id)
    {
        if (value == Guid.Empty)
        {
            id = default;
            return false;
        }

        id = new(value);
        return true;
    }

    public bool IsDefault => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
```

#### String-backed strong ID

Authoring input:

```csharp
[StrongId<string>(DisallowDefault = true)]
public readonly partial record struct ExternalSystemId;
```

Generated shape:

```csharp
public readonly partial record struct ExternalSystemId : IStrongId<string>
{
    public string Value { get; }

    public ExternalSystemId(string value)
    {
        if (String.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ExternalSystemId cannot be null, empty, or whitespace.", nameof(value));

        Value = value.Trim();
    }

    public static ExternalSystemId From(string value) => new(value);

    public static bool TryFrom(string value, out ExternalSystemId id)
    {
        if (String.IsNullOrWhiteSpace(value))
        {
            id = default;
            return false;
        }

        id = new(value);
        return true;
    }

    public bool IsDefault => String.IsNullOrWhiteSpace(Value);

    public override string ToString() => Value;
}
```

Notes:

- Strings MUST be trimmed before storage.
- The stored string preserves the original non-whitespace content and casing.
- v1 does not normalize case or canonicalize the value.
- If a canonical human-facing key is needed, it should be modeled as a different concept rather than overloaded into `StrongId`.

### Non-goals for v1

The first generator version should not try to solve every ID scenario.

- No custom validation expressions.
- No composite IDs.
- No multi-field IDs.
- No hidden ORM integration.
- No automatic domain-specific parsing logic.

That narrower scope keeps the generator understandable and avoids the exact kind of magic Aiel is trying to prevent.
