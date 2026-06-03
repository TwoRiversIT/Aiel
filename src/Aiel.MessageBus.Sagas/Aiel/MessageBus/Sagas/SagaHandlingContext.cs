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

using Aiel.MessageBus;

namespace Aiel.MessageBus.Sagas;

/// <summary>
/// Context passed to <see cref="ISagaMessageHandler{TSagaState, TMessage}.HandleAsync"/> for every
/// inbound message. Carries the loaded saga state, the full inbound message context, and a publisher
/// for emitting messages from within the saga.
/// </summary>
/// <remarks>
/// <b>v1 atomicity note:</b> <see cref="Publisher"/> is backed directly by <see cref="IMessagePublisher"/>.
/// State save and message publish are <b>not atomic</b> in v1. The <c>Aiel.MessageBus.Outbox</c> package
/// (vNext) achieves atomicity by replacing the backing implementation with <c>IOutboxWriter</c> when
/// the outbox is registered.
/// </remarks>
public sealed record SagaHandlingContext<TSagaState, TMessage>(
    TSagaState State,
    InboundMessageContext<TMessage> MessageContext,
    IMessagePublisher Publisher)
    where TSagaState : SagaState
    where TMessage : IIntegrationMessage;
