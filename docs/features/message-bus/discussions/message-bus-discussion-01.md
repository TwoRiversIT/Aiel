# Message Bus Abstraction Discussion 01

> Before starting please read #file:Framework.md . Then please review and refine the Message Bus Abstraction design, including the markdownlint warnings.
>
> Doug: Ask me questions anytime.

I've read the full document. Here are my questions before refining: 

Starting: *Update core contracts code block* (3/9)

Now for the large code block replacement: 

Completed: *Update Task 2 and Task 6 in implementation plan* (9/9)

All changes applied. Here's a summary of what was updated in message-bus-abstraction.md:

**Package family**

- Removed the non-package "separate adapter packages" row from the table; replaced with a prose sentence after the table (markdownlint fix)
- Updated `Aiel.MessageBus.Abstractions` purpose cell to be more concise

**Package ownership**

- Added a stabilization note to the `TenantIdentity` dependency bullet

**Core contracts code block**

- Added `MessageTypeAttribute` with XML doc comment
- Added convenience overload to `IMessagePublisher` (both overloads documented)
- Changed `SerializedMessage.Body` from `byte[]` to `ReadOnlyMemory<byte>`
- Added XML doc to `IMessageTypeRegistry.GetName<TMessage>()` explaining the default/override behaviour

**Why this shape fits Aiel**

- Added three new bullets covering `[MessageType]`, the convenience publisher overload, and `ReadOnlyMemory<byte>`

**Actor propagation**

- Noted that `IActor` carries a `StrongId` with no defined wire representation
- Explained why `ActorIdentifier` uses a plain `string`
- Added the application responsibility for the StrongId → string conversion

**Registration and Dependency Model**

- Replaced the `AddAielMessageBusCore()` extension approach with the `AielDependencyConfigurator` module-graph pattern (`MessageBusAbstractionsDependency`)
- Updated the adapter registration section to show the `[DependsOn]` pattern for adapters

**Decisions**

- Added **D9** (abstractions package participates in the module graph via `AielDependencyConfigurator`)
- Added **D10** (`MessageTypeName` defaults to CLR type name, overridable by `[MessageType]`)

**Completion criteria** — three new items added:

- `IMessageTypeRegistry` defaults to CLR type name / overridable by attribute
- Convenience `IMessagePublisher` overload
- `MessageBusAbstractionsDependency : AielDependencyConfigurator` subclass

**Implementation plan**

- Task 2: added `MessageTypeAttribute` to the contracts list; added `[MessageType]` to the test gate
- Task 3: added the convenience publisher overload to the rules
- Task 6: replaced the bare extension-method bullet with `MessageBusAbstractionsDependency : AielDependencyConfigurator` as the primary entry point

> Doug: Using MassTransit, NServiceBus and Rebus as the references, how well does the current message bus abstraction design cover their features. A summary table would be sufficient.

I have the full document. Now I'll produce the analysis from knowledge of those three frameworks against what the design defines.

## Feature Coverage vs. MassTransit, NServiceBus, and Rebus

**Key:**
- ✅ — first-class / fully covered
- 🔶 — partial or seam-only
- — — intentionally out of scope (Aiel: per D6; frameworks: not available)

| Area | Feature | Aiel Abstraction | MassTransit | NServiceBus | Rebus |
|---|---|:---:|:---:|:---:|:---:|
| **Core** | Publish integration messages | ✅ | ✅ | ✅ | ✅ |
| | Typed consumer handler | ✅ `IMessageHandler<T>` | ✅ `IConsumer<T>` | ✅ `IHandleMessages<T>` | ✅ `IHandleMessages<T>` |
| | Message type constraint / marker | ✅ `IIntegrationMessage` | 🔶 convention | ✅ `IMessage` hierarchy | 🔶 convention |
| **Routing** | Subscription publish (fan-out) | ✅ (implied) | ✅ | ✅ | ✅ |
| | Send to specific endpoint (command routing) | — | ✅ | ✅ | ✅ |
| | Request / response (RPC) | — | ✅ | ✅ | — |
| **Metadata & Propagation** | Message ID + Correlation ID | ✅ first-class | ✅ | ✅ | ✅ |
| | Causation tracking | ✅ `CausationMessageId` | ✅ `InitiatorId` | ✅ | 🔶 manual |
| | Tenant identity propagation | ✅ first-class | — | — | — |
| | Actor / security context | ✅ first-class | — | — | — |
| | Stable message type name | ✅ `[MessageType]` | 🔶 namespace convention | 🔶 FQN default | 🔶 FQN default |
| | Multi-version message dispatch | 🔶 stable name only | ✅ | ✅ | 🔶 |
| **Reliability** | Retry / exception handling | — | ✅ | ✅ | ✅ |
| | Dead-letter / error queues | — | ✅ | ✅ | ✅ |
| | Delayed / scheduled delivery | — | ✅ | ✅ | ✅ |
| | Outbox | 🔶 contracts ready; impl deferred | ✅ | ✅ | ✅ |
| **Serialization** | Pluggable serializer contract | ✅ `IMessageSerializer` | ✅ | ✅ | ✅ |
| | Type registry / name resolution | ✅ `IMessageTypeRegistry` | ✅ | ✅ | 🔶 |
| **Transport** | Pluggable transport adapters | 🔶 seams defined; no adapters yet | ✅ | ✅ | ✅ |
| | Endpoint topology / queue provisioning | — | ✅ | ✅ | ✅ |
| **Pipeline** | Consumer middleware / behavior pipeline | — | ✅ | ✅ | ✅ |
| | Batch consumers | — | ✅ | — | — |
| **Sagas** | Saga / workflow orchestration | — | ✅ | ✅ | 🔶 |
| **Testing** | Fakes / recording publisher | ✅ planned (`Aiel.MessageBus.Testing`) | ✅ | ✅ | ✅ |
| **DI** | Module graph / DI integration | ✅ `AielDependencyConfigurator` + `[DependsOn]` | ✅ | ✅ | ✅ |

---

### Where Aiel leads

**Tenant identity and actor/security context** are first-class fields in `MessageMetadata`. None of the three reference frameworks has this built in — all three fall back to raw custom header dictionaries. For a multi-tenant framework this is a genuine design advantage, not over-engineering.

**Typed envelope separation** (`MessageEnvelope<T>` vs `InboundMessageContext<T>`) is more explicit than MassTransit's `ConsumeContext<T>` or Rebus's raw `IMessageContext`. Causation, correlation, and client-instance propagation have named fields rather than string-keyed header bags.

**Module graph integration** (`MessageBusAbstractionsDependency` + `[DependsOn]`) makes the transport wiring statically visible. All three reference frameworks rely on opaque extension-method chains.

---

### Where the "out of scope" decisions are sound

Excluding retry, dead-letter, scheduling, sagas, endpoint addressing, and topology (D6) is consistent with how the reference frameworks position their own thin-abstraction layers. All three push those features into transport-specific configuration. Aiel's `—` entries here are intentional boundaries, not gaps.

---

### Genuine gaps worth noting

| Gap | Risk | Notes |
|---|---|---|
| **No publish vs send distinction** | Medium | All three frameworks distinguish fan-out publish (events) from point-to-point send (commands). Aiel only has `PublishAsync`. If command routing is needed, the `IMessagePublisher` contract will need a `SendAsync` overload without a breaking change. |
| **No consumer middleware pipeline** | Medium | MassTransit and NServiceBus both support behavior pipelines for cross-cutting concerns (idempotency filters, transaction scopes, tracing). Aiel adapters will have to implement these entirely within the transport layer with no framework seam. This may cause adapter inconsistency. |
| **Outbox contracts-ready but not implemented** | Low (by design) | The serialization contracts and `IMessageSerializer` / `IMessageTypeRegistry` are sufficient to build an outbox. The risk is in the deferral, not the design. |
| **Multi-version message dispatch** | Low-Medium | `[MessageType]` gives stable broker names but no strategy for consuming both `OrderPlacedV1` and `OrderPlacedV2` from the same handler or for forwarding to a versioned handler. MassTransit and NServiceBus have explicit versioning support. |
| **No real transport adapters** | High (practical) | The seams are well-shaped (`IMessagePublisher`, `IMessageSerializer`, header mapping rules) but nothing exercises them. The adapter authoring contract (Task 7) needs to clarify how `MessageMetadata` maps to native transport headers before the first real adapter is written. |

> Doug: Are we leaving out a lot, or is it "close enough"?

**Close enough** — with one exception worth addressing proactively.

### Why the gaps aren't really gaps

The design's stated purpose is a **narrow contract boundary**, not a broker framework. Look at what the `—` entries in the table actually are:

- Retry, dead-letter, scheduling, sagas, topology — these live in the transport adapter, exactly as they do in MassTransit and Rebus adapters. The frameworks don't provide those through their thin abstraction interfaces either; they provide them through transport-specific configuration fluent APIs.
- No consumer middleware pipeline — correct, but the abstraction gives adapters `InboundMessageContext<T>` with a fully reconstructed `IExecutionContext`, which is what Aiel middleware would care about. Adapter-internal pipelines can still exist; they just don't go through an Aiel seam.
- Multi-version dispatch — genuinely niche at this stage. `[MessageType]` gives stable names; whether the same queue name routes to different CLR types is a transport concern.

The "out of scope" decisions hold up.

### The one thing worth acting on now

**Publish vs Send.** This is not a transport feature — it's a messaging *pattern* concern that belongs at the abstraction level:

- **Publish** = broadcast event, fan-out, broker decides who consumes it
- **Send** = point-to-point command, one explicit receiver

All three reference frameworks make this distinction in their core publish contract, not in adapters. If Aiel doesn't, two bad things happen:

1. Adapters have to guess or add their own `SendAsync` outside the contract, creating inconsistency.
2. Application code that today calls `PublishAsync` for a command will silently break if the adapter switches it from topic routing to queue routing (or vice versa).

The fix is small — add `SendAsync` to `IMessagePublisher` (or a companion `IMessageSender`) and a corresponding `[MessageDestination]` attribute or typed destination concept. It doesn't require adding endpoint topology; it just makes the caller's intent unambiguous so adapters can map it correctly.

Everything else in the design is well-suited to Phase 1.

> Lets switch back to the Message Bus design. In the "Feature Coverage vs. MassTransit, NServiceBus, and Rebus" table the only significant feature missing from our design is Saga support. The first application that will be built on Aiel explicitly needs Sagas and would benefit from an Aiel native implementation of Outbox.  Could you update the current specification to add Sagas now, and extention points/seams where we could add Outbox and Pipeline in vNext?

