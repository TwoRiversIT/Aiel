# Aiel.MessageBus.Abstractions

Transport-backed integration message contracts for Aiel-based applications.

Provides the contract boundary between application code and message transport adapters:

- `IIntegrationMessage` — marker for types publishable through the transport-backed bus
- `MessageEnvelope<TMessage>` — message payload plus strongly-typed metadata
- `MessageMetadata` — first-class correlation, causation, actor, tenant, and message identifiers
- `IMessagePublisher` — publish messages through a transport adapter
- `IMessageHandler<TMessage>` — handle inbound messages from a transport adapter
- `IMessageTypeRegistry` — stable name resolution for message types
- `IMessageEnvelopeFactory` — centralized metadata creation
- `IMessageSerializer` / `SerializedMessage` — serialization boundary for adapters and outbox

`MessageBusAbstractionsDependency` registers `IMessageEnvelopeFactory` and `IMessageTypeRegistry`. It does **not** register `IMessagePublisher` — a transport adapter must register it.

## Messaging Boundary Distinction

| Concept | Contract | Scope |
|---|---|---|
| Mediator notifications | `IPublisher` / `INotification` | In-process only |
| Domain-event dispatch | `IDomainEventDispatcher` | Internal application/domain |
| Message bus integration | `IMessagePublisher` / `IMessageHandler<T>` | Transport-backed, inter-process |
