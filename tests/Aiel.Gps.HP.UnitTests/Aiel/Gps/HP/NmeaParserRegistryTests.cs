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

using System.Text;

namespace Aiel.Gps.HP;

[Trait("Category", "Unit")]
[Trait("Category", "Registry")]
public class NmeaParserRegistryTests
{
    // A custom message type for testing
    public struct CustomTestMessage
    {
        public Int32 Value1;
        public String Value2;
    }

    // A custom parser for testing
    public sealed class CustomTestParser : ICustomNmeaParser
    {
        public ReadOnlySpan<Byte> Identifier => "CUSTOM"u8;

        public Object Parse(ref Lexer lexer)
        {
            // Skip the sentence identifier (like built-in parsers do)
            lexer.ConsumeString();

            var value1 = lexer.NextInteger();
            var value2 = lexer.NextString();

            return new CustomTestMessage
            {
                Value1 = value1,
                Value2 = value2
            };
        }
    }

    [Fact]
    public void Register_CustomParser_CanBeRetrieved()
    {
        // Arrange
        var registry = new NmeaParserRegistry();
        var parser = new CustomTestParser();

        // Act
        registry.Register(parser);

        // Assert
        registry.TryGetParser("CUSTOM"u8, out var retrieved).Should().BeTrue();
        retrieved.Should().BeSameAs(parser);
    }

    [Fact]
    public void TryGetParser_UnregisteredIdentifier_ReturnsFalse()
    {
        // Arrange
        var registry = new NmeaParserRegistry();

        // Act
        var found = registry.TryGetParser("UNKNOWN"u8, out var parser);

        // Assert
        found.Should().BeFalse();
        parser.Should().BeNull();
    }

    [Fact]
    public void Register_DuplicateIdentifier_ThrowsException()
    {
        // Arrange
        var registry = new NmeaParserRegistry();
        var parser1 = new CustomTestParser();
        var parser2 = new CustomTestParser();

        registry.Register(parser1);

        // Act & Assert
        var act = () => registry.Register(parser2);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*CUSTOM*already registered*");
    }

    [Fact]
    public void Unregister_RegisteredParser_RemovesIt()
    {
        // Arrange
        var registry = new NmeaParserRegistry();
        var parser = new CustomTestParser();
        registry.Register(parser);

        // Act
        var removed = registry.Unregister("CUSTOM"u8);

        // Assert
        removed.Should().BeTrue();
        registry.TryGetParser("CUSTOM"u8, out _).Should().BeFalse();
    }

    [Fact]
    public void Unregister_UnregisteredParser_ReturnsFalse()
    {
        // Arrange
        var registry = new NmeaParserRegistry();

        // Act
        var removed = registry.Unregister("UNKNOWN"u8);

        // Assert
        removed.Should().BeFalse();
    }

    [Fact]
    public void RegisteredIdentifiers_ReturnsAllRegisteredIds()
    {
        // Arrange
        var registry = new NmeaParserRegistry();
        registry.Register(new CustomTestParser());

        // Act
        var identifiers = registry.RegisteredIdentifiers.ToList();

        // Assert
        identifiers.Should().Contain("CUSTOM");
    }

    [Fact]
    public void Parse_WithCustomParser_ReturnsCorrectMessage()
    {
        // Arrange
        var registry = new NmeaParserRegistry();
        registry.Register(new CustomTestParser());

        var sentence = "$CUSTOM,42,HelloWorld*00\r\n"u8;
        var lexer = new Lexer(sentence);

        registry.TryGetParser("CUSTOM"u8, out var parser);

        // Act
        var result = parser!.Parse(ref lexer);

        // Assert
        result.Should().BeOfType<CustomTestMessage>();
        var message = (CustomTestMessage)result;
        message.Value1.Should().Be(42);
        message.Value2.Should().Be("HelloWorld");
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Registry")]
public class NmeaBatchReaderWithRegistryTests
{
    // Same custom types as above
    public struct CustomTestMessage
    {
        public Int32 Value1;
        public String Value2;
    }

    public sealed class CustomTestParser : ICustomNmeaParser
    {
        public ReadOnlySpan<Byte> Identifier => "CUSTOM"u8;

        public Object Parse(ref Lexer lexer)
        {
            // Skip the sentence identifier (like built-in parsers do)
            lexer.ConsumeString();

            var value1 = lexer.NextInteger();
            var value2 = lexer.NextString();

            return new CustomTestMessage
            {
                Value1 = value1,
                Value2 = value2
            };
        }
    }

    [Fact]
    public async Task ReadAsync_WithRegistry_ParsesCustomMessages()
    {
        // Arrange
        var registry = new NmeaParserRegistry();
        registry.Register(new CustomTestParser());

        var data = Encoding.UTF8.GetBytes(
            "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n" +
            "$CUSTOM,42,HelloWorld*00\r\n");

        await using var stream = new MemoryStream(data);
        var reader = new NmeaBatchReader(stream, registry: registry);

        // Act - read both streams concurrently
        var messages = new List<NmeaMessage>();
        var customMessages = new List<Object>();

        // Start reading custom messages in parallel
        var customTask = Task.Run(async () =>
        {
            await foreach (var custom in reader.ReadCustomMessagesAsync(TestContext.Current.CancellationToken))
            {
                customMessages.Add(custom);
            }
        }, TestContext.Current.CancellationToken);

        // Read built-in messages
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        // Wait for custom messages to complete
        await customTask;

        // Assert
        messages.Should().HaveCount(1);
        messages[0].Type.Should().Be(NmeaMessageType.GLL);

        customMessages.Should().HaveCount(1);
        customMessages[0].Should().BeOfType<CustomTestMessage>();
        ((CustomTestMessage)customMessages[0]).Value1.Should().Be(42);
    }

    [Fact]
    public async Task Statistics_WithCustomMessages_CountsCorrectly()
    {
        // Arrange
        var registry = new NmeaParserRegistry();
        registry.Register(new CustomTestParser());

        var data = Encoding.UTF8.GetBytes(
            "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n" +
            "$CUSTOM,42,HelloWorld*00\r\n" +
            "$UNKNOWN,1,2,3*00\r\n");

        await using var stream = new MemoryStream(data);
        var reader = new NmeaBatchReader(stream, registry: registry);

        // Act - consume all streams concurrently
        var customTask = Task.Run(async () =>
        {
            await foreach (var _ in reader.ReadCustomMessagesAsync(TestContext.Current.CancellationToken))
            {
            }
        }, TestContext.Current.CancellationToken);

        await foreach (var _ in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
        }

        await customTask;

        // Assert
        reader.Statistics.TotalSentences.Should().Be(3);
        reader.Statistics.ParsedMessages.Should().Be(1);  // Built-in GLL
        reader.Statistics.CustomMessages.Should().Be(1);   // Custom CUSTOM
        reader.Statistics.Errors.Should().Be(1);           // Unknown UNKNOWN
    }

    [Fact]
    public async Task ReadAsync_WithoutRegistry_SkipsCustomSentences()
    {
        // Arrange - no registry provided
        var data = Encoding.UTF8.GetBytes(
            "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n" +
            "$CUSTOM,42,HelloWorld*00\r\n");

        await using var stream = new MemoryStream(data);
        var reader = new NmeaBatchReader(stream);

        // Act
        var messages = new List<NmeaMessage>();
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        // Assert - only built-in GLL should be parsed
        messages.Should().HaveCount(1);
        messages[0].Type.Should().Be(NmeaMessageType.GLL);
    }
}
