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

using System.Reflection;

namespace Aiel.MessageBus;

public sealed class ContractSurfaceTests
{
    private static readonly Assembly AbstractionsAssembly = typeof(IIntegrationMessage).Assembly;

    [Fact]
    public void AbstractionsAssembly_HasNoBrokerSdkReference()
    {
        var referencedAssemblies = AbstractionsAssembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? String.Empty)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        referencedAssemblies.Should().NotContain("Rebus");
        referencedAssemblies.Should().NotContain("MassTransit");
        referencedAssemblies.Should().NotContain("NServiceBus");
        referencedAssemblies.Should().NotContain("RabbitMQ.Client");
        referencedAssemblies.Should().NotContain("Azure.Messaging.ServiceBus");
        referencedAssemblies.Should().NotContain("Confluent.Kafka");
    }

    [Fact]
    public void AbstractionsAssembly_ExposesExpectedPublicContracts()
    {
        var exportedTypeNames = AbstractionsAssembly
            .GetExportedTypes()
            .Select(t => t.Name)
            .ToHashSet(StringComparer.Ordinal);

        exportedTypeNames.Should().Contain(nameof(IIntegrationMessage));
        exportedTypeNames.Should().Contain(nameof(MessageTypeAttribute));
        exportedTypeNames.Should().Contain(nameof(MessageTypeName));
        exportedTypeNames.Should().Contain(nameof(MessageMetadata));
        exportedTypeNames.Should().Contain("MessageEnvelope`1");
        exportedTypeNames.Should().Contain(nameof(TransportContext));
        exportedTypeNames.Should().Contain(nameof(IMessagePublisher));
        exportedTypeNames.Should().Contain("IMessageHandler`1");
        exportedTypeNames.Should().Contain(nameof(IMessageTypeRegistry));
        exportedTypeNames.Should().Contain(nameof(IMessageEnvelopeFactory));
        exportedTypeNames.Should().Contain(nameof(MessageBusAbstractionsDependency));
    }
}
