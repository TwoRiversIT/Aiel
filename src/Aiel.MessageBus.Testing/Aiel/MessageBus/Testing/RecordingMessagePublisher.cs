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

using Aiel.Actions;
using Aiel.MultiTenancy;

namespace Aiel.MessageBus.Testing;

/// <summary>
/// In-memory <see cref="IMessagePublisher"/> that records all published envelopes for assertion.
/// Designed for use in unit tests without a real broker or DI container.
/// </summary>
/// <remarks>
/// Initializes a new recording publisher. If <paramref name="factory"/> is not provided,
/// a <see cref="DefaultMessageEnvelopeFactory"/> backed by <see cref="DefaultMessageTypeRegistry"/>
/// is used for the convenience publish overload.
/// </remarks>
public sealed class RecordingMessagePublisher(IMessageEnvelopeFactory? factory = null) : IMessagePublisher
{
    private readonly IMessageEnvelopeFactory _factory = factory ?? new DefaultMessageEnvelopeFactory(new DefaultMessageTypeRegistry());
    private readonly List<Object> _published = [];

    /// <summary>Returns all envelopes published as <typeparamref name="TMessage"/>.</summary>
    public IReadOnlyList<MessageEnvelope<TMessage>> GetPublished<TMessage>()
        where TMessage : IIntegrationMessage
    {
        return _published.OfType<MessageEnvelope<TMessage>>().ToList();
    }

    /// <summary>Returns the total number of published envelopes across all message types.</summary>
    public Int32 PublishedCount => _published.Count;

    /// <summary>Removes all recorded envelopes.</summary>
    public void Clear() => _published.Clear();

    /// <inheritdoc />
    public ValueTask PublishAsync<TMessage>(
        MessageEnvelope<TMessage> envelope,
        CancellationToken cancellationToken = default)
        where TMessage : IIntegrationMessage
    {
        _published.Add(envelope);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask PublishAsync<TMessage>(
        TMessage message,
        IExecutionContext executionContext,
        TenantIdentity? tenant = null,
        CancellationToken cancellationToken = default)
        where TMessage : IIntegrationMessage
    {
        var envelope = _factory.Create(message, executionContext, tenant);
        _published.Add(envelope);
        return ValueTask.CompletedTask;
    }
}
