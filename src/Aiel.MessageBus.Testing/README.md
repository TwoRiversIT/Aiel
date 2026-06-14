# Aiel.MessageBus.Testing

Test doubles and helpers for testing code that uses `Aiel.MessageBus`. For more information, see the [Aiel.MessageBus](https://github.com/TwoRiversIT/Aiel/blob/main/src/Aiel.MessageBus/README.md) documentation.

## Provided helpers

- `RecordingMessagePublisher` Ã¢â‚¬â€ implements `IMessagePublisher`; records published envelopes for assertion without a broker
- `FakeInboundMessageContextBuilder<TMessage>` Ã¢â‚¬â€ fluent builder for `InboundMessageContext<TMessage>` in consumer handler tests

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

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
