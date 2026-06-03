# Aiel.MessageBus.Testing

Test doubles and helpers for testing code that uses `Aiel.MessageBus.Abstractions`.

## Provided helpers

- `RecordingMessagePublisher` — implements `IMessagePublisher`; records published envelopes for assertion without a broker
- `FakeInboundMessageContextBuilder<TMessage>` — fluent builder for `InboundMessageContext<TMessage>` in consumer handler tests

## Usage

```csharp
var publisher = new RecordingMessagePublisher();

// Execute code under test that calls IMessagePublisher
await myService.DoSomethingAsync(publisher, context, cancellationToken);

// Assert
var published = publisher.GetPublished<OrderPlacedEvent>();
published.Should().ContainSingle();
published[0].Message.OrderId.Should().Be(expectedOrderId);
```
