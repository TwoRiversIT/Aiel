### `Entity<TKey>`

```csharp
namespace Aiel.Domain;

public abstract class Entity<TKey> : IEquatable<Entity<TKey>>
    where TKey : notnull, IStrongId
{
    public TKey Id { get; protected init; }
    public long Version { get; protected set; }

    protected Entity(TKey id)
    {
        Id = id;
    }

    protected Entity()
    {
        Id = default!;
    }

    public bool Equals(Entity<TKey>? other);

    public override bool Equals(object? obj);

    public override int GetHashCode();

    public static bool operator ==(Entity<TKey>? left, Entity<TKey>? right);

    public static bool operator !=(Entity<TKey>? left, Entity<TKey>? right);
}
```

Semantics:

- `Id` is the identity boundary for equality.
- `TKey` must be a strong ID, not a primitive key.
- `Version` is a monotonic optimistic concurrency value controlled by persistence infrastructure.
- Equality requires the same runtime type and the same non-default key.
- Two transient entities with default keys are not equal unless they are the same reference.
- `Version` is part of concurrency control and is not part of equality.

