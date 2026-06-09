// MIT License
//
// Copyright 2026 Two Rivers Information Technology Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sub-license,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using Aiel.Execution;
using Aiel.MessageBus;

namespace Aiel.MessageBus.Testing;

public sealed class RecordingMessagePublisherTests
{
    private sealed record OrderPlacedEvent(Guid OrderId) : IIntegrationMessage;
    private sealed record ShipmentDispatchedEvent(Guid ShipmentId) : IIntegrationMessage;

    private static MessageEnvelope<T> MakeEnvelope<T>(T message) where T : IIntegrationMessage
    {
        var metadata = new MessageMetadata(
            MessageId: Guid.NewGuid(),
            CorrelationId: Guid.NewGuid(),
            CausationMessageId: null,
            ProducerOperationId: null,
            ClientInstanceId: null,
            Actor: new MessageActorSnapshot(new ActorKind("test"), new ActorIdentifier("test")),
            Tenant: null,
            SagaCorrelationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            Properties: new Dictionary<MessagePropertyName, String>());

        return new MessageEnvelope<T>(new MessageTypeName(typeof(T).Name), message, metadata);
    }

    [Fact]
    public async Task PublishAsync_WithEnvelope_RecordsIt()
    {
        var publisher = new RecordingMessagePublisher();
        var envelope = MakeEnvelope(new OrderPlacedEvent(Guid.NewGuid()));

        await publisher.PublishAsync(envelope, CancellationToken.None);

        publisher.GetPublished<OrderPlacedEvent>().Should().ContainSingle();
    }

    [Fact]
    public async Task PublishAsync_WithConvenienceOverload_RecordsIt()
    {
        var publisher = new RecordingMessagePublisher();
        var context = DefaultExecutionContext.CreateRoot(SystemActor.Instance);
        var message = new OrderPlacedEvent(Guid.NewGuid());

        await publisher.PublishAsync(message, context, cancellationToken: CancellationToken.None);

        publisher.GetPublished<OrderPlacedEvent>().Should().ContainSingle();
    }

    [Fact]
    public async Task PublishAsync_MultipleDifferentTypes_TrackedSeparately()
    {
        var publisher = new RecordingMessagePublisher();

        await publisher.PublishAsync(MakeEnvelope(new OrderPlacedEvent(Guid.NewGuid())), CancellationToken.None);
        await publisher.PublishAsync(MakeEnvelope(new OrderPlacedEvent(Guid.NewGuid())), CancellationToken.None);
        await publisher.PublishAsync(MakeEnvelope(new ShipmentDispatchedEvent(Guid.NewGuid())), CancellationToken.None);

        publisher.GetPublished<OrderPlacedEvent>().Should().HaveCount(2);
        publisher.GetPublished<ShipmentDispatchedEvent>().Should().ContainSingle();
        publisher.PublishedCount.Should().Be(3);
    }

    [Fact]
    public void GetPublished_WithNoMatchingType_ReturnsEmpty()
    {
        var publisher = new RecordingMessagePublisher();

        publisher.GetPublished<OrderPlacedEvent>().Should().BeEmpty();
    }

    [Fact]
    public async Task Clear_RemovesAllRecordedEnvelopes()
    {
        var publisher = new RecordingMessagePublisher();
        await publisher.PublishAsync(MakeEnvelope(new OrderPlacedEvent(Guid.NewGuid())), CancellationToken.None);

        publisher.Clear();

        publisher.PublishedCount.Should().Be(0);
        publisher.GetPublished<OrderPlacedEvent>().Should().BeEmpty();
    }
}
