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

using Aiel;
using Aiel.Dependencies;
using Aiel.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aiel.MessageBus;

/// <summary>
/// Module-graph entry point for the message bus abstractions package.
/// Registers <see cref="IMessageEnvelopeFactory"/> and <see cref="IMessageTypeRegistry"/>.
/// Does <b>not</b> register <see cref="IMessagePublisher"/> — transport adapters must
/// register it. Resolving <see cref="IMessagePublisher"/> without a registered adapter
/// fails at composition.
/// </summary>
[DependsOn(typeof(AielApplicationContracts))]
[DependsOn(typeof(AielMultiTenancy))]
public sealed class MessageBusAbstractionsDependency : AielDependency
{
    public override ValueTask ConfigureAsync(
        DependencyConfigurationContext context,
        CancellationToken cancellationToken = default)
    {
        context.Services.TryAddSingleton<IMessageTypeRegistry, DefaultMessageTypeRegistry>();
        context.Services.TryAddScoped<IMessageEnvelopeFactory, DefaultMessageEnvelopeFactory>();

        return ValueTask.CompletedTask;
    }
}
