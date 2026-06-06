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

namespace Aiel.MessageBus;

public sealed class MessageMetadataTests
{
    private static MessageMetadata ValidMetadata(Guid? messageId = null, Guid? correlationId = null)
        => new(
            MessageId: messageId ?? Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationMessageId: null,
            ProducerOperationId: null,
            ClientInstanceId: null,
            Actor: new MessageActorSnapshot(new ActorKind("test"), new ActorIdentifier("test-actor")),
            Tenant: null,
            SagaCorrelationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            Properties: new Dictionary<MessagePropertyName, String>());

    [Fact]
    public void MessageMetadata_WithValidIds_Constructs()
    {
        var act = () => ValidMetadata();

        act.Should().NotThrow();
    }

    [Fact]
    public void MessageMetadata_RejectsEmptyMessageId()
    {
        var act = () => ValidMetadata(messageId: Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("MessageId");
    }

    [Fact]
    public void MessageMetadata_RejectsEmptyCorrelationId()
    {
        var act = () => ValidMetadata(correlationId: Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("CorrelationId");
    }

    [Fact]
    public void MessageMetadata_PreservesAllFields()
    {
        var messageId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var metadata = new MessageMetadata(
            MessageId: messageId,
            CorrelationId: correlationId,
            CausationMessageId: causationId,
            ProducerOperationId: null,
            ClientInstanceId: null,
            Actor: new MessageActorSnapshot(new ActorKind("user"), new ActorIdentifier("user-123")),
            Tenant: null,
            SagaCorrelationId: null,
            OccurredAtUtc: now,
            Properties: new Dictionary<MessagePropertyName, String>());

        metadata.MessageId.Should().Be(messageId);
        metadata.CorrelationId.Should().Be(correlationId);
        metadata.CausationMessageId.Should().Be(causationId);
        metadata.OccurredAtUtc.Should().Be(now);
    }
}
