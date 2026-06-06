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
[Trait("Category", "BatchReader")]
public class NmeaBatchReaderTests
{
    [Fact]
    public async Task ReadAsync_SingleGLLSentence_ReturnsOneMessage()
    {
        // Arrange
        var data = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8.ToArray();
        await using var stream = new MemoryStream(data);
        var reader = new NmeaBatchReader(stream);

        // Act
        var messages = new List<NmeaMessage>();
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        // Assert
        messages.Should().HaveCount(1);
        messages[0].Type.Should().Be(NmeaMessageType.GLL);
    }

    [Fact]
    public async Task ReadAsync_MultipleSentences_ReturnsAllMessages()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes(
            "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n" +
            "$GFDTA,7.2,98,3.0,16380,2019/01/22 16:04:58,CH4AB-1047,1*40\r\n" +
            "$GPGLL,5057.1975,N,11134.8332,W,232608,A,*00\r\n");
        await using var stream = new MemoryStream(data);
        var reader = new NmeaBatchReader(stream);

        // Act
        var messages = new List<NmeaMessage>();
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        // Assert
        messages.Should().HaveCount(3);
        messages[0].Type.Should().Be(NmeaMessageType.GLL);
        messages[1].Type.Should().Be(NmeaMessageType.GFDTA);
        messages[2].Type.Should().Be(NmeaMessageType.GLL);
    }

    [Fact]
    public async Task ReadAsync_EmptyStream_ReturnsNoMessages()
    {
        // Arrange
        await using var stream = new MemoryStream([]);
        var reader = new NmeaBatchReader(stream);

        // Act
        var messages = new List<NmeaMessage>();
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        // Assert
        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadAsync_UnknownSentence_SkipsAndContinues()
    {
        // Arrange - unknown sentence in the middle
        var data = Encoding.UTF8.GetBytes(
            "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n" +
            "$UNKNOWN,1,2,3*00\r\n" +
            "$GPGLL,5057.1975,N,11134.8332,W,232608,A,*00\r\n");
        await using var stream = new MemoryStream(data);
        var reader = new NmeaBatchReader(stream);

        // Act
        var messages = new List<NmeaMessage>();
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        // Assert - should have 2 GLL messages, unknown was skipped
        messages.Should().HaveCount(2);
    }

    [Fact]
    public async Task ReadErrorsAsync_UnknownSentence_ReturnsError()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes(
            "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n" +
            "$UNKNOWN,1,2,3*00\r\n");
        await using var stream = new MemoryStream(data);
        var reader = new NmeaBatchReader(stream);

        // Act - start reading to trigger parsing
        var messagesTask = Task.Run(async () =>
        {
            var list = new List<NmeaMessage>();
            await foreach (var msg in reader.ReadAsync())
            {
                list.Add(msg);
            }

            return list;
        });

        var errors = new List<ParseError>();
        await foreach (var error in reader.ReadErrorsAsync(TestContext.Current.CancellationToken))
        {
            errors.Add(error);
        }

        await messagesTask;

        // Assert
        errors.Should().HaveCount(1);
        errors[0].Sentence.Should().Contain("UNKNOWN");
    }

    [Fact]
    public async Task Statistics_AfterReading_ReportsCorrectCounts()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes(
            "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n" +
            "$GFDTA,7.2,98,3.0,16380,2019/01/22 16:04:58,CH4AB-1047,1*40\r\n" +
            "$UNKNOWN,1,2,3*00\r\n");
        await using var stream = new MemoryStream(data);
        var reader = new NmeaBatchReader(stream);

        // Act
        await foreach (var _ in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            // Consume all messages
        }

        // Assert
        reader.Statistics.TotalSentences.Should().Be(3);
        reader.Statistics.ParsedMessages.Should().Be(2);
        reader.Statistics.Errors.Should().Be(1);
        reader.Statistics.BytesRead.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ReadAsync_WithCancellation_StopsReading()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes(
            "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n" +
            "$GPGLL,5057.1975,N,11134.8332,W,232608,A,*00\r\n" +
            "$GPGLL,3751.65,S,14507.36,E,081836,A,*00\r\n");
        await using var stream = new MemoryStream(data);
        var reader = new NmeaBatchReader(stream);
        using var cts = new CancellationTokenSource();

        // Act
        var messages = new List<NmeaMessage>();
        await foreach (var message in reader.ReadAsync(cts.Token))
        {
            messages.Add(message);
            if (messages.Count == 1)
            {
                await cts.CancelAsync();
                break;
            }
        }

        // Assert
        messages.Should().HaveCount(1);
    }

    [Fact]
    public async Task ReadAsync_PartialSentenceAtEnd_HandlesGracefully()
    {
        // Arrange - data ends mid-sentence (no \r\n)
        var data = Encoding.UTF8.GetBytes(
            "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n" +
            "$GPGLL,5057.1975,N");  // Incomplete
        await using var stream = new MemoryStream(data);
        var reader = new NmeaBatchReader(stream);

        // Act
        var messages = new List<NmeaMessage>();
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        // Assert - should only parse the complete sentence
        messages.Should().HaveCount(1);
    }

    [Fact]
    public async Task ReadAsync_SentenceWithoutDollarSign_SkipsGarbage()
    {
        // Arrange - garbage data before valid sentence
        var data = Encoding.UTF8.GetBytes(
            "garbage data here\r\n" +
            "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n");
        await using var stream = new MemoryStream(data);
        var reader = new NmeaBatchReader(stream);

        // Act
        var messages = new List<NmeaMessage>();
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        // Assert
        messages.Should().HaveCount(1);
        messages[0].Type.Should().Be(NmeaMessageType.GLL);
    }

    [Fact]
    public async Task ReadAsync_LargeFile_ProcessesEfficiently()
    {
        // Arrange - simulate a larger file with many sentences
        var sb = new StringBuilder();
        for (var i = 0; i < 1000; i++)
        {
            sb.AppendLine("$GPGLL,4916.45,N,12311.12,W,225444,A,*1D");
        }

        var data = Encoding.UTF8.GetBytes(sb.ToString());
        await using var stream = new MemoryStream(data);
        var reader = new NmeaBatchReader(stream);

        // Act
        var count = 0;
        await foreach (var _ in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            count++;
        }

        // Assert
        count.Should().Be(1000);
        reader.Statistics.ParsedMessages.Should().Be(1000);
    }
}
