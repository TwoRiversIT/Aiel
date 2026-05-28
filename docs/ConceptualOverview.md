# A Conceptual Overview of the Emerging Aiel Application Framework

This document summarizes the architectural philosophy, core values, and major design decisions that have emerged through our exploration of modern .NET domain modeling. It captures the direction, the reasoning, and the unifying mental model that guides the framework.

---

## Philosophy

Most of these principles apply both to the framework itself and to applications built on top of it. The framework must be a model of the architectural principles it promotes.

- Guide developers to the “right” choice  
- Be opinionated, but not too opinionated  
- Always minimize technical debt
- Prioritize **readability**: clarity over cleverness
- Long‑term maintainability over short‑term speed of development
- Make the good path obvious and the bad path inconvenient
- Explicit is better than implicit: Avoid hidden magic and implicit behavior
- Design for deterministic and predictable behavior
- Traceability is a first class feature
- Errors must be modeled explicitly in both the API and Domain
- Prefer composition over inheritance and follow SOLID principles by default
- Design APIs to be small, focused, and predictable, with clear naming conventions and immutability by default
- Embrace strong typing to prevent invalid states and improve readability
- Use source generators to reduce boilerplate and enforce consistency
- Use Analyzers to guide to guide the developer, not beat them into submission
- Async first. Just because something is not asynchronous today does not mean it will never need to be. This also means passing `CancellationToken` everywhere, even if it is not used today.

### Greenfield Posture

Aiel is a greenfield framework for greenfield applications.

> **Greenfield Posture** is not merely a risk-reduction justification (i.e., "no deployments yet, so we can break things"). It is a **quality commitment**. When a shortcut would introduce technical debt — even a small one — the correct answer is to stop, back out the shortcut, and implement it properly. The absence of backward-compatibility pressure is an *opportunity* to do the right thing, not a license to cut corners.

That means early architectural decisions must be evaluated primarily on long-term correctness, maintainability, and clarity, not on migration cost, time pressure, or backward-compatibility with weak existing patterns.

Breaking changes throughout the v1.x lifecycle are acceptable when they reduce ambiguity, eliminate complexity, or move the framework closer to a cleaner design.

vNext MAY be a clean break, or it MAY be a gradual evolution. The guiding principle is to prefer the refactor that improves the architecture over the patch that preserves it.

### Guided by Design

Aiel should guide developers toward maintainable code and widely accepted industry practices.

The framework is intentionally opinionated in areas where ambiguity tends to create technical debt.

Examples include:

- strong identifiers instead of primitive IDs
- explicit contracts instead of convention-heavy magic
- clear dependency direction between layers
- explicit domain boundaries and error modeling

The goal is not to expose endless knobs. The goal is to make the good path obvious and the bad path inconvenient.

### No Hidden Magic

Aiel MUST avoid framework behavior that is difficult to see, reason about, or debug.

In particular, prefer approaches that avoid:

- dynamic proxy requirements
- implicit runtime interception
- hidden IL mutation as part of normal framework behavior
- conventions that materially change behavior without being visible in code

Framework features MUST be understandable by reading the code and the generated output.

### Zero Technical Debt

**Refactor Early, Refactor Often!** Technical debt compounds. Correctness and design quality outweigh backward compatibility at this stage.

> Prefer the refactor that improves the architecture over the patch that preserves it.

When choosing between a minimal fix with low impact or a deeper refactor that improves the overall design, prefer the refactor. The framework is still in a greenfield stage, so breaking changes are acceptable when they improve correctness, clarity, or long‑term maintainability.

Technical debt is minimized by:

- Keep modules focused and minimize dependencies  
- Keep dependencies inward: **Presentation → Application → Domain → Shared/Common**  
  - Domain **must not** depend on Application, Infrastructure, or Presentation  
  - Infrastructure **may** depend on Application and Domain only to implement contracts  
- Identify cross‑cutting concerns early and abstract them out  
- Favoring strong typing and explicitness  
- Avoiding ambiguous or error‑prone constructs  
- Prefer composition over inheritance  

### Coding Style

- Follow **SOLID** principles  
- **Avoid `null` returns** — use `Aiel.Results.Result<T>` or value objects  
- **No tuples** — even private; use value objects  
- No **magic strings** or **magic numbers** — use `const` or strongly typed wrappers  
- Avoid **primitive obsession** — use strongly typed identifiers and value objects  

---

## Architectural Principles

### Opinionated, with Purpose

Opinions exist to reduce ambiguity, prevent foot‑guns, and guide developers toward predictable, maintainable patterns. Choose approaches that:

- Reduce cognitive load  
- Improve readability  
- Minimize future refactoring  
- Align with established framework patterns  

Framework opinions MUST be introduced early when they define architectural direction rather than incidental implementation detail.

**Strong ID**s are a good example: Prioritizing them early helps prevent primitive obsession, inconsistent APIs, and expensive rework across persistence, application, and domain layers later.

### Readability and Explicitness

Readable code is maintainable code. Favor explicitness over cleverness, verbosity over ambiguity, and structure over shortcuts.

- Prefer named value objects over primitives: `EmailAddress` instead of `String`, `Money` instead of `Decimal`
- Prefer explicit method names over terse abstractions
- Prefer clear control flow over compact expressions  

### Deterministic and Reproducible

Every part of the system MUST be deterministic:

- Reproducible builds  
- Stable NuGet packaging  
- Predictable dependency graphs  
- No hidden side effects  

Predictability applies equally to architecture, runtime behavior, and developer experience.

---

## **1. Core Philosophy**

The framework is built on a small set of deeply held values:

### **1.1 Explicitness Over Magic**

Developers should always understand what the code is doing.  
There must be:

- no dynamic proxies  
- no runtime weaving  
- no hidden interception  
- no reflection-based dispatch  
- no “surprise” behaviors  

Everything is visible in the code or generated at compile time.

### **1.2 Strong Invariants, Strong Types**

Domain invariants are not optional. They define the valid shape of the model and must hold:

- after construction  
- after every mutation  
- after rehydration  

Strongly typed IDs enforce identity invariants and eliminate primitive obsession.

### **1.3 Unified Mental Model**

Even though aggregates may differ internally (state-based vs event-sourced), developers should interact with them through a **consistent API** and **consistent patterns**.

This reduces cognitive load and makes the system easier to reason about.

### **1.4 CQRS as a First-Class Concept**

Commands and queries are fundamentally different concerns.

- Writes are strongly consistent.  
- Reads are eventually consistent.  
- Read models are projections, not domain objects.  

This separation is explicit and intentional.

### **1.5 Read Models as Projections**

All queries—whether the aggregate is state-based or event-sourced—operate on **projection-based read models**.

This unifies the read side and gives developers a single mental model for querying.

### **1.6 Zero Coupling Between Domain and Persistence**

Aggregates know nothing about:

- EF Core  
- event stores  
- repositories  
- projections  

Persistence is an infrastructure concern.

---

## **2. Strongly Typed IDs**

Strong IDs are central to the framework’s identity model.

### **2.1 Why Strong IDs?**

- Prevent accidental misuse of identifiers  
- Enforce non-default invariants  
- Improve readability  
- Improve correctness  
- Make domain boundaries explicit  

### **2.2 Why No Base Class?**

A base class would restrict the shape of ID types and complicate value semantics.  
Instead, the design favors:

- a tiny marker interface (`IStrongId<TValue>`)  
- compile-time generated boilerplate (via source generators)  
- simple, explicit `record struct` types  

### **2.3 Why Source Generators?**

They allow:

- zero inheritance  
- zero runtime magic  
- explicit generated code  
- type-specific factories  
- type-specific converters  

This keeps strong IDs ergonomic without sacrificing clarity.

---

## **3. Two Aggregate Types**

The framework supports two distinct aggregate models:

### **3.1 State-Based Aggregates**

- Snapshot-based persistence  
- Ideal for simple, stable domains  
- Backed by EF Core or similar ORM  
- Mutate internal state directly  
- May raise domain events, but events are not the source of truth  

### **3.2 Event-Sourced Aggregates**

- Event stream is the source of truth  
- Rehydrated by replaying events  
- Commands produce events  
- State is derived, not stored  
- Requires an event store  

### **3.3 Why Both?**

Different domains have different needs.  
The framework supports both without forcing one model onto all aggregates.

---

## **4. Unified Write-Side Repository Model**

Despite their internal differences, both aggregate types are persisted through the same interface:

```csharp
IAggregateRepository<TAggregate, TId>
```

### **4.1 Why a Unified Repository?**

- Developers interact with aggregates the same way  
- Commands do not need to know whether an aggregate is event-sourced or state-based  
- Reduces cognitive load  
- Keeps the domain layer clean  

### **4.2 Why Not Skip Repositories?**

Repositories provide:

- a clear persistence boundary  
- a consistent abstraction  
- a unified mental model  
- a place to enforce optimistic concurrency  
- a place to integrate with outbox patterns  

Skipping them would fragment the developer experience.

---

## **5. Unified Read-Side Model**

This is one of the most important design decisions.

### **5.1 All Read Models Are Projections**

Regardless of aggregate type:

- Event-sourced aggregates → projections from events  
- State-based aggregates → projections from state changes  

This means:

- All queries use the same repository  
- All queries use the same specification pattern  
- All read models are eventually consistent  
- All read models are denormalized and optimized for queries  

### **5.2 Why Give State-Based Aggregates Projections?**

To unify the mental model.

If event-sourced aggregates require projections, but state-based aggregates do not, developers must remember two different query paths. That is unnecessary complexity.

By giving *every* aggregate a projection:

- the read side becomes consistent  
- the architecture becomes symmetrical  
- the developer experience becomes simpler  

---

## **6. Eventual Consistency as an Explicit Concept**

The framework does not hide eventual consistency. It embraces it.

### **6.1 Why Explicit?**

Because hiding it leads to:

- subtle bugs  
- incorrect assumptions  
- inconsistent behavior  
- developer confusion  

### **6.2 How It Is Modeled**

- Read models are updated asynchronously  
- Projection checkpoints track progress  
- Projection lag is measurable  
- Commands never read from projections  
- Queries always read from projections  

This makes the system predictable and honest.

---

## **7. Projection Pipeline**

Projections are the bridge between the write side and the read side.

### **7.1 Event-Sourced Projections**

Driven by:

- event store subscriptions  
- asynchronous handlers  
- replayable pipelines  

### **7.2 State-Based Projections**

Driven by:

- domain events  
- EF Core interceptors  
- outbox patterns  

### **7.3 Why Asynchronous Projections?**

Because real-world projections can be:

- slow  
- external  
- computationally heavy  
- cross-aggregate  
- cross-system  

Running them inline with command processing would harm performance and reliability.

---

## **8. Specification Pattern for Queries**

All read-side queries use:

```csharp
IQuerySpecification<TReadModel>
```

### **8.1 Why Specifications?**

- Encapsulate query logic  
- Reusable  
- Testable  
- Composable  
- Work across EF Core, document stores, and search indexes  

### **8.2 Why Only on the Read Side?**

Specifications do not make sense for event-sourced aggregates (no IQueryable).  
By using projections for all queries, specifications become universally applicable.

### **8.3 What Specifications Do Not Do**

Query specifications define filtering and composition rules for read models.

They should not be stretched to also own:

- pagination
- sorting
- transport-specific search parameter binding

Those concerns belong at the application boundary, where a query message can carry explicit paging and sorting models without polluting the Specification pattern itself.

This keeps the mental model clean:

- query specifications express which read models match
- application query contracts express how a result should be shaped for the caller

---

## **9. What This Architecture Achieves**

### **9.1 A Unified Developer Experience**

- Same repository interface for all aggregates  
- Same read repository for all queries  
- Same specification pattern  
- Same projection pipeline  
- Same consistency model  

### **9.2 A Clean Separation of Concerns**

- Domain layer: invariants and behavior  
- Write side: aggregate persistence  
- Read side: projections and queries  
- Infrastructure: event store, EF Core, messaging  

### **9.3 Zero Magic**

Everything is explicit, visible, and predictable.

### **9.4 A Modern, Scalable CQRS Architecture**

Without the ceremony or complexity of many existing frameworks.

---

## **10. What This Is Not**

- It is not a CRUD framework  
- It is not a heavy event-sourcing framework  
- It is not an ORM wrapper  
- It is not a magic abstraction layer  

It is a **domain modeling framework** that:

- respects invariants  
- respects developer cognition  
- respects architectural clarity  
- respects explicitness  
