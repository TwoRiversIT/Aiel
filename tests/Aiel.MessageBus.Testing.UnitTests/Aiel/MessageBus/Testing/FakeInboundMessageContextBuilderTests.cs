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

public sealed class FakeInboundMessageContextBuilderTests
{
    private sealed record TestEvent(String Value) : IIntegrationMessage;

    private static MessageEnvelope<TestEvent> MakeEnvelope(TestEvent message)
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

        return new MessageEnvelope<TestEvent>(new MessageTypeName("TestEvent"), message, metadata);
    }

    [Fact]
    public void Build_WithRequiredFields_ReturnsContext()
    {
        var envelope = MakeEnvelope(new TestEvent("hello"));
        var context = DefaultExecutionContext.CreateRoot(SystemActor.Instance);

        var inbound = new FakeInboundMessageContextBuilder<TestEvent>()
            .WithEnvelope(envelope)
            .WithExecutionContext(context)
            .Build();

        inbound.Envelope.Should().Be(envelope);
        inbound.ExecutionContext.Should().Be(context);
        inbound.Transport.TransportName.Should().Be("fake");
    }

    [Fact]
    public void Build_WithCustomTransport_UsesIt()
    {
        var envelope = MakeEnvelope(new TestEvent("hello"));
        var context = DefaultExecutionContext.CreateRoot(SystemActor.Instance);
        var transport = new TransportContext("rabbitmq", "native-id-123", 2);

        var inbound = new FakeInboundMessageContextBuilder<TestEvent>()
            .WithEnvelope(envelope)
            .WithExecutionContext(context)
            .WithTransport(transport)
            .Build();

        inbound.Transport.Should().Be(transport);
    }

    [Fact]
    public void Build_WithoutEnvelope_Throws()
    {
        var context = DefaultExecutionContext.CreateRoot(SystemActor.Instance);

        var act = () => new FakeInboundMessageContextBuilder<TestEvent>()
            .WithExecutionContext(context)
            .Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Envelope*");
    }

    [Fact]
    public void Build_WithoutExecutionContext_Throws()
    {
        var envelope = MakeEnvelope(new TestEvent("hello"));

        var act = () => new FakeInboundMessageContextBuilder<TestEvent>()
            .WithEnvelope(envelope)
            .Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ExecutionContext*");
    }
}
