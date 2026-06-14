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

namespace Aiel.Gps;

public class NmeaReaderTests
{
    [Fact]
    public async Task ReadAsync_returns_messages_from_stream()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new RMC());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n" +
                   "$GPRMC,081836,A,3751.65,S,14507.36,E,000.0,360.0,130998,011.3,E*62\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        await using var reader = new NmeaReader(streamReader, stream);

        var messages = new List<NmeaMessage>();

        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        messages.Should().HaveCount(2);
        messages[0].Should().BeOfType<GGA>();
        messages[1].Should().BeOfType<RMC>();
    }

    [Fact]
    public async Task ReadAsync_handles_cancellation()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        await using var reader = new NmeaReader(streamReader, stream);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var messages = new List<NmeaMessage>();

        await foreach (var message in reader.ReadAsync(cts.Token))
        {
            messages.Add(message);
        }

        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadNextAsync_returns_messages_one_at_a_time()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new RMC());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n" +
                   "$GPRMC,081836,A,3751.65,S,14507.36,E,000.0,360.0,130998,011.3,E*62\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        await using var reader = new NmeaReader(streamReader, stream);

        var message1 = await reader.ReadNextAsync(TestContext.Current.CancellationToken);
        var message2 = await reader.ReadNextAsync(TestContext.Current.CancellationToken);
        var message3 = await reader.ReadNextAsync(TestContext.Current.CancellationToken);

        message1.Should().NotBeNull().And.BeOfType<GGA>();
        message2.Should().NotBeNull().And.BeOfType<RMC>();
        message3.Should().BeNull();
    }

    [Fact]
    public async Task ReadNextAsync_handles_cancellation()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        await using var reader = new NmeaReader(streamReader, stream);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var message = await reader.ReadNextAsync(cts.Token);

        message.Should().BeNull();
    }

    [Fact]
    public async Task Multiple_ReadAsync_calls_use_same_stream()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        await using var reader = new NmeaReader(streamReader, stream);

        var message1 = await reader.ReadNextAsync(TestContext.Current.CancellationToken);
        var message2 = await reader.ReadNextAsync(TestContext.Current.CancellationToken);

        message1.Should().NotBeNull();
        message2.Should().BeNull();
    }

    [Fact]
    public async Task Throws_when_different_cancellation_tokens_used()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n" +
                   "$GPGGA,232609.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*63\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        await using var reader = new NmeaReader(streamReader, stream);

        using var cts1 = new CancellationTokenSource();
        using var cts2 = new CancellationTokenSource();

        var message1 = await reader.ReadNextAsync(cts1.Token);
        message1.Should().NotBeNull();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await reader.ReadNextAsync(cts2.Token));
    }

    [Fact]
    public async Task DisposeAsync_stops_background_parsing()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        var reader = new NmeaReader(streamReader, stream);

        var message = await reader.ReadNextAsync(TestContext.Current.CancellationToken);

        await reader.DisposeAsync();

        var action = async () => await reader.ReadNextAsync(TestContext.Current.CancellationToken);

        await action.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task Dispose_stops_background_parsing()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        var reader = new NmeaReader(streamReader, stream);

        reader.Dispose();

        var action = async () => await reader.ReadNextAsync(TestContext.Current.CancellationToken);

        await action.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task ReadAsync_handles_empty_stream()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA());

        await using var stream = new MemoryStream();
        await using var reader = new NmeaReader(streamReader, stream);

        var messages = new List<NmeaMessage>();

        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadAsync_filters_messages_by_registered_parsers()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n" +
                   "$GPRMC,081836,A,3751.65,S,14507.36,E,000.0,360.0,130998,011.3,E*62\r\n" +
                   "$GPGGA,232609.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*63\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        await using var reader = new NmeaReader(streamReader, stream);

        var messages = new List<NmeaMessage>();

        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        messages.Should().HaveCount(2);
        messages.Should().AllBeOfType<GGA>();
    }

    [Fact]
    public async Task ReadAsync_processes_mixed_line_endings()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new RMC());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\n" +
                   "$GPRMC,081836,A,3751.65,S,14507.36,E,000.0,360.0,130998,011.3,E*62\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        await using var reader = new NmeaReader(streamReader, stream);

        var messages = new List<NmeaMessage>();

        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        messages.Should().HaveCount(2);
    }
}
