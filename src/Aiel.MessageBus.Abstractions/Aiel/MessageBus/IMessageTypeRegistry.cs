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

/// <summary>
/// Resolves stable <see cref="MessageTypeName"/> values for message types and maps them back
/// to CLR types. Used by <see cref="IMessageEnvelopeFactory"/> and transport adapters.
/// </summary>
public interface IMessageTypeRegistry
{
    /// <summary>
    /// Returns the stable <see cref="MessageTypeName"/> for <typeparamref name="TMessage"/>.
    /// Defaults to the fully qualified CLR type name. Apply <see cref="MessageTypeAttribute"/>
    /// to override with a stable, broker-friendly name.
    /// </summary>
    MessageTypeName GetName<TMessage>()
        where TMessage : IIntegrationMessage;

    /// <summary>
    /// Resolves the CLR <see cref="Type"/> for the given <paramref name="messageTypeName"/>.
    /// </summary>
    Type Resolve(MessageTypeName messageTypeName);
}
