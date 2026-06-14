# Aiel.MessageBus.Sagas

Saga orchestration contracts for Aiel-based applications. For more information, see the [Aiel.MessageBus](https://github.com/TwoRiversIT/Aiel/blob/main/src/Aiel.MessageBus/README.md) documentation.

Sagas are durable, correlated, long-running coordination workflows backed by `Aiel.MessageBus.Abstractions`.

## Key contracts

- `SagaState`: abstract base class for saga state bags; pure data holder
- `IAmStartedByMessage<T>` / `IHandleSagaMessage<T>`: explicit lifecycle markers on orchestrator classes
- `ICorrelateMessage<TSagaState, TMessage>`: maps inbound messages to saga instances (type-safe, no convention scanning)
- `ISagaMessageHandler<TSagaState, TMessage>`: the handler contract for orchestrators
- `ISagaRepository<TSagaState>`: persistence seam; must be registered by infrastructure; no default in-memory implementation
- `SagasDependency`: module-graph entry point

## Atomicity note (v1)

In v1, `SagaHandlingContext.Publisher` is backed directly by `IMessagePublisher`. State save and message publish are **not atomic**. The `Aiel.MessageBus.Outbox` package (vNext) resolves this by routing publication through `IOutboxWriter` within the same transaction.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
