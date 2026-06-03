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

/// <summary>
/// Publishes integration messages through the configured transport adapter.
/// Not registered by <see cref="MessageBusAbstractionsDependency"/> — a transport adapter
/// must register an implementation. Resolving this interface without a registered adapter
/// fails at composition.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a pre-built envelope through the configured transport adapter.
    /// Use this overload when you need to inspect or mutate metadata before publication.
    /// </summary>
    ValueTask PublishAsync<TMessage>(
        MessageEnvelope<TMessage> envelope,
        CancellationToken cancellationToken = default)
        where TMessage : IIntegrationMessage;

    /// <summary>
    /// Creates an envelope via the registered <see cref="IMessageEnvelopeFactory"/> and publishes it.
    /// </summary>
    ValueTask PublishAsync<TMessage>(
        TMessage message,
        IExecutionContext executionContext,
        TenantIdentity? tenant = null,
        CancellationToken cancellationToken = default)
        where TMessage : IIntegrationMessage;
}
