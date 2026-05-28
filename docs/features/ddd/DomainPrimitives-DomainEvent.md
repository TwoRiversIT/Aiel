
```csharp
namespace Aiel.Domain;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
    string EventType { get; }
}
```

Semantics:

- `EventId` is the idempotency key for outbox dispatch and downstream handlers.
- `OccurredOn` records when the event was raised in domain time.
- `EventType` is the stable serialization discriminator stored with the outbox payload.
- `EventType` should default to a stable CLR name only if that name is treated as part of the compatibility contract. If cross-version compatibility is required, event types should declare an explicit constant name.
