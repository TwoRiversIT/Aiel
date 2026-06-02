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
using Aiel.MultiTenancy;

namespace Aiel.MessageBus;

public sealed class MessageEnvelopeFactoryTests
{
    private sealed record TestEvent(String Value) : IIntegrationMessage;

    private readonly DefaultMessageEnvelopeFactory _factory = new(new DefaultMessageTypeRegistry());

    [Fact]
    public void Create_CopiesCorrelationIdFromContext()
    {
        var context = DefaultExecutionContext.CreateRoot(SystemActor.Instance);

        var envelope = _factory.Create(new TestEvent("hello"), context);

        envelope.Metadata.CorrelationId.Should().Be(context.CorrelationId);
    }

    [Fact]
    public void Create_CopiesClientInstanceIdFromContext()
    {
        var clientId = Guid.NewGuid();
        var context = DefaultExecutionContext.CreateRoot(SystemActor.Instance, clientInstanceId: clientId);

        var envelope = _factory.Create(new TestEvent("hello"), context);

        envelope.Metadata.ClientInstanceId.Should().Be(clientId);
    }

    [Fact]
    public void Create_SetsProducerOperationId()
    {
        var context = DefaultExecutionContext.CreateRoot(SystemActor.Instance);

        var envelope = _factory.Create(new TestEvent("hello"), context);

        envelope.Metadata.ProducerOperationId.Should().Be(context.OperationId);
    }

    [Fact]
    public void Create_AcceptsNullTenant()
    {
        var context = DefaultExecutionContext.CreateRoot(SystemActor.Instance);

        var envelope = _factory.Create(new TestEvent("hello"), context, tenant: null);

        envelope.Metadata.Tenant.Should().BeNull();
    }

    [Fact]
    public void Create_PropagatesTenant()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var tenant = new TenantIdentity(tenantId);
        var context = DefaultExecutionContext.CreateRoot(SystemActor.Instance);

        var envelope = _factory.Create(new TestEvent("hello"), context, tenant: tenant);

        envelope.Metadata.Tenant.Should().Be(tenant);
    }

    [Fact]
    public void Create_GeneratesUniqueMessageIdPerCall()
    {
        var context = DefaultExecutionContext.CreateRoot(SystemActor.Instance);
        var message = new TestEvent("hello");

        var envelope1 = _factory.Create(message, context);
        var envelope2 = _factory.Create(message, context);

        envelope1.Metadata.MessageId.Should().NotBe(envelope2.Metadata.MessageId);
    }

    [Fact]
    public void Create_SetsActorIdentifierFromAuditIdentity()
    {
        var context = DefaultExecutionContext.CreateRoot(SystemActor.Instance);

        var envelope = _factory.Create(new TestEvent("hello"), context);

        envelope.Metadata.Actor.Identifier.Value.Should().Be(SystemActor.Instance.AuditIdentity);
    }

    [Fact]
    public void Create_SagaCorrelationIdIsNull()
    {
        var context = DefaultExecutionContext.CreateRoot(SystemActor.Instance);

        var envelope = _factory.Create(new TestEvent("hello"), context);

        envelope.Metadata.SagaCorrelationId.Should().BeNull();
    }
}
