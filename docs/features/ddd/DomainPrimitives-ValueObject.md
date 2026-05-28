# Domain Primitives

## ValueObject

- Value objects are immutable by convention.
- Value objects do not carry identity, lifecycle, or concurrency semantics.

### Record vs POCO

For most DDD value objects **use a C# `record`**: it gives correct structural equality, immutability-friendly defaults, and far less boilerplate. Keep a small hand-rolled `ValueObject` base only when you need custom equality rules, special immutability constraints, or behavior that records don’t express cleanly.

**Record:**

```csharp
public sealed record FullName(String First, String Last);
```

**POCO:**

```csharp
public sealed class FullName(string? first, string? last) : ValueObject
{
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return first;
        yield return last;
    }
}

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<Object?> GetEqualityComponents();

    public Boolean Equals(ValueObject? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        using var left = GetEqualityComponents().GetEnumerator();
        using var right = other.GetEqualityComponents().GetEnumerator();

        while (true)
        {
            var leftHasNext = left.MoveNext();
            var rightHasNext = right.MoveNext();

            if (!leftHasNext && !rightHasNext)
            {
                return true;
            }

            if (leftHasNext != rightHasNext)
            {
                return false;
            }

            if (!Equals(left.Current, right.Current))
            {
                return false;
            }
        }
    }

    public override Boolean Equals(Object? obj) => Equals(obj as ValueObject);

    public override Int32 GetHashCode()
    {
        var hash = new HashCode();

        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    public static Boolean operator ==(ValueObject? left, ValueObject? right)
        => ReferenceEquals(left, null)
            ? ReferenceEquals(right, null)
            : left.Equals(right);

    public static Boolean operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}
```

### Comparison at a glance

| **Attribute** | **C# record** | **ValueObject base class** | **When to prefer** |
|---|---:|---|---|
| **Boilerplate** | Minimal; compiler generates equality, `Deconstruct`, `ToString`. | More code up front; you implement `GetEqualityComponents`. | Use records for most simple VOs.  |
| **Immutability** | `init` properties by default; easy to make immutable. | Fully under your control; can enforce stricter immutability. | Base class if you need custom immutability rules.  |
| **Equality semantics** | Structural equality generated from declared positional properties (or all properties for non-positional records). | You explicitly control which components participate in equality. | Base class when equality rules are non‑trivial.  |
| **Inheritance & polymorphism** | Records support inheritance but positional equality can be surprising across hierarchies. | You can implement type checks and equality rules exactly as you want. | Base class for complex inheritance scenarios.  |
| **ORM / persistence** | Works fine with EF Core; mapping choices matter (owned types vs entities). | Same, but explicit shape can be easier to map as owned types. | Either, but be explicit about EF Core owned types.  |

---

### Pros of using a C# record
- **Very low boilerplate** — compiler generates equality, `GetHashCode`, `ToString`, and `Deconstruct`.   
- **Immutable-by-default ergonomics** — `init` properties and positional syntax make immutable construction concise.   
- **Correct structural equality** for most VO scenarios — two records with the same data compare equal out of the box.   
- **Language-level features** (with-expressions, deconstruction) that are handy for value-style programming. 

---

### Pros of a ValueObject base class (your implementation)
- **Explicit control over equality** — you choose exactly which components matter and how they’re compared (useful for derived or computed components).   
- **Custom behavior and validation** can be centralized (e.g., normalization, caching, interning).   
- **Familiar DDD pattern** — some teams prefer the explicit `ValueObject` type to signal domain intent and to add domain helpers (e.g., `IsEmpty`, `Validate`). 

---

### Gotchas and pitfalls to watch for
- **Record positional vs non-positional equality** — positional records use the primary constructor fields for equality; adding non-positional properties can surprise you. Prefer explicit property declarations if you want clarity.   
- **Inheritance surprises** — equality across record hierarchies can be unintuitive (type checks, base vs derived equality). If you rely on inheritance, test equality semantics carefully.   
- **Mutable properties break VO semantics** — records can have mutable properties; ensure you use `init` or readonly fields to preserve immutability.   
- **Serialization and ORM mapping** — how you declare the record (positional vs property) affects serializers and EF Core mapping; map value objects as EF Core owned types when appropriate.   
- **Hash code stability** — if you compute hash codes from mutable components or include transient data, you can break hash-based collections. Use immutable components only. 

---

### Alternatives and patterns to consider
- **Record struct** (`record struct`) — value-type semantics (stack allocation) for small, high-volume VOs; watch for boxing and copying semantics.   
- **Hand-rolled `ValueObject` base** (like your sample) — when you need custom equality, normalization, or caching/interning. Good for complex domain rules.   
- **Libraries** — e.g., CSharpFunctionalExtensions, Ardalis.ValueObject, or other community implementations that provide battle-tested base classes and helpers. They can save time and encode best practices.   
- **Immutable classes with `IEquatable<T>`** — if you want full control without a base class, implement `IEquatable<T>` and `GetHashCode` yourself (your sample is a valid pattern). 

---

### Practical recommendations
1. **Default to `record`** for simple, immutable value objects (addresses, money, coordinates). It’s concise and correct for most cases.   
2. **Use a base `ValueObject`** only when: equality must ignore or transform fields, you need centralized normalization/validation, or you have complex inheritance rules.   
3. **Be explicit about immutability** — prefer `init` or readonly fields; avoid mutable properties.   
4. **Test equality and hashing** thoroughly (unit tests for `Equals`, `==`, `GetHashCode`) and test ORM mapping if you persist VOs. 
