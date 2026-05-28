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

public class NmeaStreamReaderTests
{
    [Fact]
    public async Task NmeaStreamReader_will_throw_if_no_parsers_have_been_added()
    {
        await using (var stream = new MemoryStream())
        {
            var nsr = new NmeaStreamReader();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await nsr.ParseStreamAsync(stream, _ => { }, TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task ParseStreamAsync_invokes_callback_for_each_message()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new RMC());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n" +
                   "$GPRMC,081836,A,3751.65,S,14507.36,E,000.0,360.0,130998,011.3,E*62\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        var messages = new List<NmeaMessage>();

        var exitReason = await streamReader.ParseStreamAsync(stream, messages.Add, TestContext.Current.CancellationToken);

        messages.Should().HaveCount(2);
        messages[0].Should().BeOfType<GGA>();
        messages[1].Should().BeOfType<RMC>();
        exitReason.Should().Be(ExitReason.StreamEnded);
    }

    [Fact]
    public async Task ParseStreamAsync_resets_unparsed_counter_on_successful_parse()
    {
        var streamReader = new NmeaStreamReader
        {
            AbortAfterUnparsedLines = 2
        }.Register(new GGA());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n" +
                   "INVALID LINE 1\r\n" +
                   "$GPGGA,232609.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*63\r\n" +
                   "INVALID LINE 2\r\n" +
                   "$GPGGA,232610.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*6B\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        var messages = new List<NmeaMessage>();

        var exitReason = await streamReader.ParseStreamAsync(stream, messages.Add, TestContext.Current.CancellationToken);

        messages.Should().HaveCount(3);
        exitReason.Should().Be(ExitReason.StreamEnded);
    }

    [Fact]
    public async Task ParseStreamAsync_handles_cancellation()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        using var cts = new CancellationTokenSource();

        await cts.CancelAsync();

        var messages = new List<NmeaMessage>();

        var exitReason = await streamReader.ParseStreamAsync(stream, messages.Add, cts.Token);

        exitReason.Should().Be(ExitReason.CancellationRequested);
    }

    [Fact]
    public async Task ParseStreamAsync_tracks_line_count()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new RMC());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n" +
                   "$GPRMC,081836,A,3751.65,S,14507.36,E,000.0,360.0,130998,011.3,E*62\r\n" +
                   "INVALID LINE\r\n";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        var messages = new List<NmeaMessage>();

        await streamReader.ParseStreamAsync(stream, messages.Add, TestContext.Current.CancellationToken);

        streamReader.LineCount.Should().Be(3);
    }

    [Fact]
    public async Task ParseStreamAsync_tracks_bytes_received()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA());

        const String data = "$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n";
        var bytes = Encoding.UTF8.GetBytes(data);

        await using var stream = new MemoryStream(bytes);

        var messages = new List<NmeaMessage>();

        await streamReader.ParseStreamAsync(stream, messages.Add, TestContext.Current.CancellationToken);

        streamReader.BytesReceived.Should().Be(bytes.Length);
    }

    [Fact]
    public async Task Register_throws_on_empty_array()
    {
        var streamReader = new NmeaStreamReader();

        var action = () => streamReader.Register([]);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*At least one parser must be provided*");
    }

    [Fact]
    public async Task Register_returns_self_for_chaining()
    {
        var streamReader = new NmeaStreamReader();

        var result = streamReader.Register(new GGA());

        result.Should().BeSameAs(streamReader);
    }
}

