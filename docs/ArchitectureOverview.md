# Architectural Overview

> The key words "MUST", "MUST NOT", "REQUIRED", "SHALL", "SHALL NOT", "SHOULD", "SHOULD NOT", "RECOMMENDED", "MAY", and "OPTIONAL" in this document are to be interpreted as described in [RFC 2119](https://www.ietf.org/rfc/rfc2119.txt).
>
> That said, the author notes that the word **“should”** is spelled strangely, conveys ambiguous obligation strength, and is generally an unpleasant lexical artifact. Its use SHOULD be minimized, avoided, or ideally eliminated entirely. The author acknowledges the irony.

---

## Layered Architecture

### Domain Layer

The heart of the system. Pure, isolated, and free of infrastructure concerns.

- Contains business rules, invariants, and value objects  
- No dependencies on Application, Infrastructure, or Presentation  
- No external libraries except those essential for domain modeling  

### Application Layer

Coordinates use cases and orchestrates domain behavior.

- Defines commands, queries, and handlers  
- Contains interfaces for infrastructure implementations  
- No UI or persistence logic  

### Infrastructure Layer

Implements contracts defined in the Application layer.

- Persistence  
- External services  
- File systems, messaging, caching  
- Replaceable without modifying Domain or Application  

### Presentation Layer

User‑facing layer.

- Blazor components  
- API endpoints  
- UI‑specific models and mapping  

### Dependency Direction

```
Presentation → Application → Domain → Shared/Common
```

Infrastructure sits beside Application and Domain, depending only on their contracts.

---

## Coding Style and Conventions

### Strong Typing Everywhere

Avoid primitive obsession.

Use value objects for:

- Identifiers  
- Email addresses  
- Domain names  
- Money  
- Dates and ranges  
- Any concept with rules or invariants  

If an interface accepts or returns an intrinsic type, consider if it should be a value object instead.

### No `null` Returns

Use `Result<T>` or value objects to enforce explicit error handling.

### No Tuples

Tuples obscure meaning and encourage positional thinking. Prefer:

- Value objects  
- Records  
- Named types  

### No Magic Strings or Numbers

If a value participates in control flow, it must be named.

### SOLID by Default

Every module, class, and method should naturally follow SOLID principles.

---

## Error Handling Philosophy

### Explicit, Structured, and Intentional

Errors are part of the domain and must be modeled.

- Use `Result<T>` for all public APIs. See [Aiel.Results](../src/Aiel.Results/README.md) for details.
- Prefer domain‑specific error types  
- Do not use exceptions for control flow
- Exceptions are reserved for truly exceptional conditions  

### Fail Fast, but Fail Clearly

When something goes wrong:

- Stop immediately  
- Provide a clear, actionable message  
- Avoid cascading failures  

Operational clarity matters too: failures should be easy to correlate through structured logs, traces, and emitted events using the same correlation metadata.

---

## API Design Principles

### Small, Focused, and Predictable

APIs should be:

- Easy to read  
- Hard to misuse  
- Consistent across modules  
- Explicit about side effects  

### Naming Conventions

- Commands end with `Command`  
- Queries end with `Query`  
- Handlers end with `Handler`  
- Value objects are nouns  
- Services are verbs or verb phrases  

### Immutability by Default

- Prefer immutable types unless mutability is required for clarity, performance, or external constraints.
- Immutable types are easier to reason about, thread‑safe, and align with functional programming principles.
- When mutability is necessary, it should be explicit and well‑encapsulated to avoid unintended side effects.
- Consider using `record` types for immutable data structures, and classes with private setters or builder patterns for mutable types.

---

## Build, Packaging, and CI Philosophy

### Deterministic Builds

Builds must be reproducible across machines and environments.

- Locked dependency versions  
- SourceLink enabled  
- XML docs included  
- Strong naming or signing where appropriate  

### Gold‑Standard NuGet Packaging

Packages should include:

- XML documentation  
- SourceLink  
- Deterministic builds  
- Clear versioning strategy  
- Minimal dependencies  

### CI as a First‑Class Citizen

CI should enforce:

- Consistent formatting (`.editorconfig`, `dotnet format`)
- Static analysis  
- Unit tests  
- Deterministic builds  
- Package validation  

---

## Cross‑Cutting Concerns

### Traceability by Default

Aiel should make end-to-end traceability a default capability rather than an afterthought.

Every command or request should execute inside an explicit execution context carrying correlation metadata.

Client-facing applications should also carry a client instance ID so activity can be grouped by client session and then broken down into individual correlation scopes.

That metadata should flow through execution contexts, message contexts, outbox records, integration messages, and structured log scopes rather than being embedded in every business payload type.

`CorrelationId` ties the whole end-to-end flow together.

`CausationId` points to the immediate parent operation that directly caused the current operation.

`OperationId` identifies the current execution scope so downstream work has something precise to reference as its cause.

This supports:

- structured logging systems such as Seq
- searching all activity for a single use case execution
- grouping multiple requests from the same client instance
- troubleshooting distributed flows without relying on brittle text searches

These IDs are technical tracing identifiers, not business identifiers.

They exist to improve diagnostics, observability, and operational clarity.

### Logging

- Structured logging  
- No string concatenation  
- Domain events logged at appropriate boundaries  

### Validation

- Domain invariants enforced in constructors  
- Application‑level validation for commands and queries  
- No UI‑driven validation leaking into domain logic  

### Configuration

- Strongly typed configuration objects  
- No magic configuration keys  
- Fail fast on missing configuration  

---

## Testing Philosophy

### Test Behavior, Not Implementation

Tests should validate:

- Domain invariants  
- Application workflows  
- Error handling paths  
- Boundary conditions  

Avoid brittle tests tied to internal structure.

### Prefer Unit Tests, Add Integration Tests Where Needed

- Domain: 100% unit tested  
- Application: unit tests + integration tests for orchestrations  
- Infrastructure: integration tests for persistence and external services  

---

## UI and Frontend Philosophy

TBD

---
