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

namespace Aiel.MessageBus.Testing;

/// <summary>
/// Fluent builder for <see cref="InboundMessageContext{TMessage}"/> in consumer handler tests.
/// </summary>
public sealed class FakeInboundMessageContextBuilder<TMessage>
    where TMessage : IIntegrationMessage
{
    private MessageEnvelope<TMessage>? _envelope;
    private IExecutionContext? _executionContext;
    private TransportContext _transport = new("fake", null, 1);

    public FakeInboundMessageContextBuilder<TMessage> WithEnvelope(MessageEnvelope<TMessage> envelope)
    {
        _envelope = envelope;
        return this;
    }

    public FakeInboundMessageContextBuilder<TMessage> WithExecutionContext(IExecutionContext context)
    {
        _executionContext = context;
        return this;
    }

    public FakeInboundMessageContextBuilder<TMessage> WithTransport(TransportContext transport)
    {
        _transport = transport;
        return this;
    }

    public InboundMessageContext<TMessage> Build()
    {
        if (_envelope is null)
        {
            throw new InvalidOperationException("Envelope is required. Call WithEnvelope() before Build().");
        }

        if (_executionContext is null)
        {
            throw new InvalidOperationException("ExecutionContext is required. Call WithExecutionContext() before Build().");
        }

        return new InboundMessageContext<TMessage>(_envelope, _executionContext, _transport);
    }
}
