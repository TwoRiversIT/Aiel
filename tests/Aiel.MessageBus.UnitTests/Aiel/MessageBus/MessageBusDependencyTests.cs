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

using Microsoft.Extensions.DependencyInjection;

namespace Aiel.MessageBus;

public sealed class MessageBusDependencyTests
{
    private static ServiceProvider BuildWithAbstractionsOnly()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMessageTypeRegistry, DefaultMessageTypeRegistry>();
        services.AddScoped<IMessageEnvelopeFactory, DefaultMessageEnvelopeFactory>();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void IMessageTypeRegistry_IsRegistered()
    {
        var provider = BuildWithAbstractionsOnly();

        var registry = provider.GetService<IMessageTypeRegistry>();

        registry.Should().NotBeNull()
            .And.BeOfType<DefaultMessageTypeRegistry>();
    }

    [Fact]
    public void IMessageEnvelopeFactory_IsRegistered()
    {
        var provider = BuildWithAbstractionsOnly();

        var factory = provider.GetService<IMessageEnvelopeFactory>();

        factory.Should().NotBeNull()
            .And.BeOfType<DefaultMessageEnvelopeFactory>();
    }

    [Fact]
    public void IMessagePublisher_IsNotRegisteredByAbstractionsDependency()
    {
        var provider = BuildWithAbstractionsOnly();

        var publisher = provider.GetService<IMessagePublisher>();

        publisher.Should().BeNull("IMessagePublisher must not be registered by the abstractions dependency; transport adapters must register it");
    }
}
