# Plan: Message Bus Abstractions (Tasks 1–6, 9)

## Context

Aiel has in-process coordination (`Aiel.Mediator` notifications, `IDomainEventDispatcher`) but no framework-owned abstraction for transport-backed inter-process messaging. Without this boundary, transport-specific concepts leak into application code and metadata conventions (correlation, tenant, actor) become ad hoc per application. The design document at `docs/features/message-bus/message-bus-abstraction.md` specifies the full feature; this plan implements Tasks 1–6 and Task 9 (all v1 completion criteria), leaving Task 8 (outbox implementation) as a documented follow-on. Task 0 (design input verification) was completed in the prior conversation session.

## New Packages

### 1. `src/Aiel.MessageBus.Abstractions/`

**csproj:** `RootNamespace` empty, `net10.0`, `IsPackable=true`, README.md packed. Project references: `Aiel.Application.Contracts`, `Aiel.MultiTenancy`. `<PackageReference Update="ConfigureAwait.Fody"><TreatAsUsed>true</TreatAsUsed></PackageReference>`.

**`FodyWeavers.xml`:** `<ConfigureAwait />` (same pattern as `Aiel.Results`).

**File layout** — all files in `Aiel/MessageBus/`, namespace `Aiel.MessageBus`:

| File | Purpose |
|---|---|
| `MessageBusAbstractionsDependency.cs` | `AielDependency` subclass; `[DependsOn(AielApplicationContracts)]` + `[DependsOn(AielMultiTenancy)]`; registers `IMessageEnvelopeFactory` and `IMessageTypeRegistry`; does **not** register `IMessagePublisher` |
| `IIntegrationMessage.cs` | Marker interface |
| `MessageTypeAttribute.cs` | `[AttributeUsage(Class\|Struct, Inherited=false)]`; `public String Name { get; }` |
| `MessageTypeName.cs` | `readonly record struct(String Value)` |
| `MessagePropertyName.cs` | `readonly record struct(String Value)` |
| `ActorKind.cs` | `readonly record struct(String Value)` |
| `ActorIdentifier.cs` | `readonly record struct(String Value)` |
| `SagaId.cs` | `readonly record struct(Guid Value)` |
| `MessageActorSnapshot.cs` | `sealed record(ActorKind Kind, ActorIdentifier Identifier)` |
| `MessageMetadata.cs` | `sealed record`; redeclare `MessageId` and `CorrelationId` as `{ get; init; }` with `= EnsureNotEmpty(...)` validation (same pattern as `DefaultExecutionContext.EnsureNotEmpty`) |
| `MessageEnvelope.cs` | `sealed record MessageEnvelope<TMessage> where TMessage : IIntegrationMessage` |
| `TransportContext.cs` | `sealed record(String TransportName, String? NativeMessageId, Int32 DeliveryAttempt)` |
| `InboundMessageContext.cs` | `sealed record InboundMessageContext<TMessage> where TMessage : IIntegrationMessage` |
| `IMessageEnvelopeFactory.cs` | `Create<TMessage>(TMessage, IExecutionContext, TenantIdentity? = null)` |
| `IMessagePublisher.cs` | Two overloads: envelope overload + convenience overload delegating to `IMessageEnvelopeFactory` |
| `IMessageHandler.cs` | `IMessageHandler<TMessage> where TMessage : IIntegrationMessage` |
| `SerializedMessage.cs` | `sealed record(MessageTypeName, ReadOnlyMemory<Byte> Body, MessageMetadata)` |
| `IMessageSerializer.cs` | Serialize/Deserialize pair |
| `IMessageTypeRegistry.cs` | `GetName<TMessage>()` + `Resolve(MessageTypeName)` |
| `IOutboxWriter.cs` | **vNext seam** — comment: `// vNext — seam only; implemented in Aiel.MessageBus.Outbox; not registered in v1` |
| `IOutboxDispatcher.cs` | **vNext seam** |
| `IInboundMessageContext.cs` | **vNext seam** — non-generic view for middleware |
| `IMessageConsumptionMiddleware.cs` | **vNext seam** |
| `DefaultMessageEnvelopeFactory.cs` | `internal sealed class`; registered by `MessageBusAbstractionsDependency` as scoped |
| `DefaultMessageTypeRegistry.cs` | `internal sealed class`; registered as singleton |

**`DefaultMessageEnvelopeFactory.Create<TMessage>` logic:**
- `MessageId = Guid.NewGuid()`
- `CorrelationId = executionContext.CorrelationId`
- `CausationMessageId = null` (adapters set this from inbound message)
- `ProducerOperationId = executionContext.OperationId`
- `ClientInstanceId = executionContext.ClientInstanceId`
- `Actor = new MessageActorSnapshot(new ActorKind(executionContext.Actor.GetType().Name), new ActorIdentifier(executionContext.Actor.AuditIdentity))`
- `Tenant = tenant` (explicit parameter)
- `OccurredAtUtc = DateTimeOffset.UtcNow`
- `Properties = ReadOnlyDictionary<MessagePropertyName, String>.Empty`
- `SagaCorrelationId = null`
- `MessageType = _registry.GetName<TMessage>()`

**`DefaultMessageTypeRegistry` logic:**
- `GetName<TMessage>()`: reflects `[MessageType]` attribute on `TMessage`; falls back to `typeof(TMessage).FullName!`
- `Resolve(MessageTypeName)`: calls `Type.GetType(name.Value, throwOnError: true)!` (works for the CLR-name default; adapters can provide their own registry for attribute-named types)

---

### 2. `src/Aiel.MessageBus.Sagas/`

**csproj:** same pattern; project reference to `Aiel.MessageBus.Abstractions`. `FodyWeavers.xml` with `<ConfigureAwait />`.

**File layout** — `Aiel/MessageBus/Sagas/`, namespace `Aiel.MessageBus.Sagas`:

| File | Purpose |
|---|---|
| `SagasDependency.cs` | `[DependsOn(typeof(MessageBusAbstractionsDependency))]`; `ConfigureAsync` is empty |
| `SagaState.cs` | `abstract class`; `SagaId { get; internal set; }`, `Boolean IsCompleted { get; private set; }`, `protected internal void MarkComplete()` |
| `IAmStartedByMessage.cs` | Lifecycle marker; `where TMessage : IIntegrationMessage` |
| `IHandleSagaMessage.cs` | Lifecycle marker |
| `ICorrelateMessage.cs` | `SagaId GetSagaId(TMessage message, MessageMetadata metadata)` |
| `SagaHandlingContext.cs` | `sealed record SagaHandlingContext<TSagaState, TMessage>(..., IMessagePublisher Publisher)` with v1 atomicity caveat XML doc |
| `ISagaMessageHandler.cs` | `ValueTask HandleAsync(SagaHandlingContext<...> context, CancellationToken)` |
| `ISagaRepository.cs` | `FindAsync`, `SaveAsync`, `DeleteAsync`; `where TSagaState : SagaState, new()` |

---

### 3. `src/Aiel.MessageBus.Testing/`

**csproj:** project reference to `Aiel.MessageBus.Abstractions` only. No `AielDependency` — consumed directly as a test helper, not wired into the module graph.

**File layout** — `Aiel/MessageBus/Testing/`, namespace `Aiel.MessageBus.Testing`:

| File | Purpose |
|---|---|
| `RecordingMessagePublisher.cs` | Implements `IMessagePublisher`; stores envelopes in a `List<object>`; exposes `IReadOnlyList<MessageEnvelope<TMessage>> GetPublished<TMessage>()` |
| `FakeInboundMessageContextBuilder.cs` | Fluent builder for `InboundMessageContext<TMessage>` in tests |

---

## Test Projects

### `tests/Aiel.MessageBus.UnitTests/`
References `Aiel.MessageBus.Abstractions`. Key tests:
- `MessageMetadata_RejectsEmptyMessageId` / `MessageMetadata_RejectsEmptyCorrelationId`
- `DefaultMessageTypeRegistry_DefaultsToFullyQualifiedTypeName`
- `DefaultMessageTypeRegistry_UsesAttributeNameWhenPresent`
- `DefaultMessageEnvelopeFactory_CopiesCorrelationAndClientInstanceFromContext`
- `DefaultMessageEnvelopeFactory_SetsProducerOperationId`
- `DefaultMessageEnvelopeFactory_AcceptsNullTenant`
- `MessageBusAbstractionsDependency_DoesNotRegisterIMessagePublisher` — resolving `IMessagePublisher` via `IServiceCollection.BuildServiceProvider()` throws `InvalidOperationException`
- `ContractSurface_ContainsNoBrokerSdkReference` — verifies the assembly has no reference to broker packages

### `tests/Aiel.MessageBus.Sagas.UnitTests/`
References `Aiel.MessageBus.Sagas`. Key tests:
- `SagaState_IsCompletedStartsFalse`
- `SagaState_MarkCompleteSetsTrueOnce`

### `tests/Aiel.MessageBus.Testing.UnitTests/`
References `Aiel.MessageBus.Testing`. Key tests:
- `RecordingPublisher_RecordsPublishedEnvelopes`
- `FakeInboundMessageContextBuilder_BuildsValidContext`

---

## Modified Files

### `src/Aiel.Application.Contracts/Aiel/Messaging/MessageContext.cs` — **DELETE**
Design decision D8: replaced by `MessageEnvelope<TMessage>` and `InboundMessageContext<TMessage>`. Verify no usages in `src/` before deleting (`grep -rn "MessageContext" src/ --include="*.cs"`).

### `Aiel.slnx` — add 6 entries
Under the existing `/src/` folder and `/tests/` folder, no new solution folders needed:
```xml
<Project Path="src/Aiel.MessageBus.Abstractions/Aiel.MessageBus.Abstractions.csproj" />
<Project Path="src/Aiel.MessageBus.Sagas/Aiel.MessageBus.Sagas.csproj" />
<Project Path="src/Aiel.MessageBus.Testing/Aiel.MessageBus.Testing.csproj" />
<Project Path="tests/Aiel.MessageBus.UnitTests/Aiel.MessageBus.UnitTests.csproj" />
<Project Path="tests/Aiel.MessageBus.Sagas.UnitTests/Aiel.MessageBus.Sagas.UnitTests.csproj" />
<Project Path="tests/Aiel.MessageBus.Testing.UnitTests/Aiel.MessageBus.Testing.UnitTests.csproj" />
```

---

## Conventions Checklist (for every new file)

- MIT license header (23-line block, year 2026, Two Rivers Information Technology Inc.)
- Framework types: `String`, `Int32`, `Boolean`, `Guid` (not C# aliases)
- `sealed` on all concrete types; `sealed record` for value-carrying types
- `CancellationToken cancellationToken = default` on every async method
- File-scoped namespaces

---

## Out of Scope (follow-on)

- `Aiel.MessageBus.Outbox` — seam interfaces `IOutboxWriter`/`IOutboxDispatcher` are defined here but not implemented
- Transport adapters (Rebus, MassTransit)
- Consumer pipeline middleware activation (`IMessageConsumptionMiddleware` seam defined, not registered)

---

## Verification

```pwsh
# Verify no broker SDK reference crept in
dotnet build src\Aiel.MessageBus.Abstractions --no-restore --nologo

# Run targeted tests
dotnet test tests\Aiel.MessageBus.UnitTests --no-restore --verbosity minimal
dotnet test tests\Aiel.MessageBus.Sagas.UnitTests --no-restore --verbosity minimal
dotnet test tests\Aiel.MessageBus.Testing.UnitTests --no-restore --verbosity minimal

# Full solution must remain clean
dotnet build Aiel.slnx -c Debug --nologo
dotnet test Aiel.slnx --no-restore --verbosity minimal
```
