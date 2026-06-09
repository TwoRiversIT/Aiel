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

using Aiel.Resources;

namespace Aiel.Gps;

[Collection("Parsing")]
public sealed class RealWorldDataTests
{
    [Fact]
    public async Task ProcessesTrack1Data()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new RMC(), new GSA(), new GSV(), new GLL(), new VTG(), new GFDTA());

        await using var stream = RH.GetStream<RealWorldDataTests>("TestData.track1.nmea");
        await using var reader = new NmeaReader(streamReader, stream);

        var messages = new List<NmeaMessage>();
        var errors = new List<ParseError>();

        var readMessagesTask = Task.Run(async () =>
        {
            await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
            {
                if (message is null)
                {
                    throw new Exception();
                }

                messages.Add(message);
            }
        }, TestContext.Current.CancellationToken);

        var readErrorsTask = Task.Run(async () =>
        {
            await foreach (var error in reader.ReadErrorsAsync(TestContext.Current.CancellationToken))
            {
                errors.Add(error);
            }
        }, TestContext.Current.CancellationToken);

        await Task.WhenAll(readMessagesTask, readErrorsTask);

        // If there were parse errors, fail the test with details
        if (errors.Count > 0)
        {
            var errorDetails = String.Join(Environment.NewLine, errors.Select(e => $"  Line {e.LineNumber}: {e.Exception.Message} - {e.RawPayload}"));
            throw new Exception($"Found {errors.Count} parse error(s):{Environment.NewLine}{errorDetails}");
        }

        messages.OfType<GGA>().Should().HaveCount(4490);
        messages.OfType<RMC>().Should().HaveCount(4490);
        messages.OfType<GSA>().Should().HaveCount(4490);
        messages.OfType<GSV>().Should().HaveCount(0);
        messages.OfType<GLL>().Should().HaveCount(0);
        messages.OfType<VTG>().Should().HaveCount(0);
        messages.OfType<GFDTA>().Should().HaveCount(0);
    }

    [Fact]
    public async Task ProcessesTrack2Data()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new RMC(), new GSA(), new GSV(), new GLL(), new VTG(), new GFDTA());

        await using var stream = RH.GetStream<RealWorldDataTests>("TestData.track2.nmea");
        await using var reader = new NmeaReader(streamReader, stream);

        var messages = new List<NmeaMessage>();
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        var ggaCount = messages.OfType<GGA>().Count();
        var rmcCount = messages.OfType<RMC>().Count();
        var gsaCount = messages.OfType<GSA>().Count();
        var gsvCount = messages.OfType<GSV>().Count();
        var gllCount = messages.OfType<GLL>().Count();
        var vtgCount = messages.OfType<VTG>().Count();
        var gfdtaCount = messages.OfType<GFDTA>().Count();

        messages.Should().HaveCount(ggaCount + rmcCount + gsaCount + gsvCount + gllCount + vtgCount + gfdtaCount,
            $"all parsed messages should be counted (GGA:{ggaCount}, RMC:{rmcCount}, GSA:{gsaCount}, GSV:{gsvCount}, GLL:{gllCount}, VTG:{vtgCount}, GFDTA:{gfdtaCount})");

        ggaCount.Should().Be(600);
        rmcCount.Should().Be(600);
        gsaCount.Should().Be(600);
        gsvCount.Should().Be(2313);
        gllCount.Should().Be(0);
        vtgCount.Should().Be(0);
        gfdtaCount.Should().Be(370);
    }

    [Fact]
    public async Task ProcessesTrack3Data()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new RMC(), new GSA(), new GSV(), new GLL(), new VTG(), new GFDTA());

        await using var stream = RH.GetStream<RealWorldDataTests>("TestData.track3.nmea");
        await using var reader = new NmeaReader(streamReader, stream);

        var messages = new List<NmeaMessage>();
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        messages.Should().HaveCount(343);
        messages.OfType<GGA>().Should().HaveCount(39);
        messages.OfType<RMC>().Should().HaveCount(39);
        messages.OfType<GSA>().Should().HaveCount(39);
        messages.OfType<GSV>().Should().HaveCount(147);
        messages.OfType<GLL>().Should().HaveCount(40);
        messages.OfType<VTG>().Should().HaveCount(39);
        messages.OfType<GFDTA>().Should().HaveCount(0);
    }

    [Fact]
    public async Task ProcessesGasFinderData()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new RMC(), new GSA(), new GSV(), new GLL(), new VTG(), new GFDTA());

        await using var stream = RH.GetStream<RealWorldDataTests>("TestData.gf.nmea");
        await using var reader = new NmeaReader(streamReader, stream);

        var messages = new List<NmeaMessage>();
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messages.Add(message);
        }

        messages.Should().HaveCount(121);
        messages.OfType<GGA>().Should().HaveCount(0);
        messages.OfType<RMC>().Should().HaveCount(0);
        messages.OfType<GSA>().Should().HaveCount(0);
        messages.OfType<GSV>().Should().HaveCount(0);
        messages.OfType<GLL>().Should().HaveCount(0);
        messages.OfType<VTG>().Should().HaveCount(0);
        messages.OfType<GFDTA>().Should().HaveCount(121);
    }

    [Fact]
    public async Task Track1DataContainsValidGpsPositions()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA());

        await using var stream = RH.GetStream<RealWorldDataTests>("TestData.track1.nmea");
        await using var reader = new NmeaReader(streamReader, stream);

        var validPositions = 0;
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            if (message is GGA gga && gga.Latitude != 0.0 && gga.Longitude != 0.0)
            {
                validPositions++;
            }
        }

        validPositions.Should().BeGreaterThan(0, "real-world data should contain valid GPS positions");
    }

    [Fact]
    public async Task Track2DataContainsMixedMessageTypes()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new GSV(), new GFDTA());

        await using var stream = RH.GetStream<RealWorldDataTests>("TestData.track2.nmea");
        await using var reader = new NmeaReader(streamReader, stream);

        var messageTypes = new HashSet<Type>();
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messageTypes.Add(message.GetType());
        }

        messageTypes.Should().Contain(typeof(GGA), "track2 contains GGA sentences");
        messageTypes.Should().Contain(typeof(GSV), "track2 contains GSV sentences");
        messageTypes.Should().Contain(typeof(GFDTA), "track2 contains GFDTA sentences");
    }

    [Fact]
    public async Task Track3DataContainsAllStandardMessageTypes()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new RMC(), new GSA(), new GSV(), new GLL(), new VTG());

        await using var stream = RH.GetStream<RealWorldDataTests>("TestData.track3.nmea");
        await using var reader = new NmeaReader(streamReader, stream);

        var messageTypes = new HashSet<Type>();
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            messageTypes.Add(message.GetType());
        }

        messageTypes.Should().Contain(typeof(GGA), "track3 contains GGA sentences");
        messageTypes.Should().Contain(typeof(RMC), "track3 contains RMC sentences");
        messageTypes.Should().Contain(typeof(GSA), "track3 contains GSA sentences");
        messageTypes.Should().Contain(typeof(GSV), "track3 contains GSV sentences");
        messageTypes.Should().Contain(typeof(GLL), "track3 contains GLL sentences");
        messageTypes.Should().Contain(typeof(VTG), "track3 contains VTG sentences");
    }

    [Fact]
    public async Task LargeDataSetDoesNotExceedBufferCapacity()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new RMC(), new GSA());

        await using var stream = RH.GetStream<RealWorldDataTests>("TestData.track1.nmea");
        await using var reader = new NmeaReader(streamReader, stream);

        var processedCount = 0;
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            processedCount++;
        }

        // 4490 GGA + 4490 RMC + 4490 GSA = 13470 total messages in track1.nmea
        processedCount.Should().Be(13470, "all messages should be processed without buffer overflow");
    }

    [Fact]
    public async Task ReadNextAsyncWorksWithLargeDataSet()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA(), new RMC(), new GSA());

        await using var stream = RH.GetStream<RealWorldDataTests>("TestData.track1.nmea");
        await using var reader = new NmeaReader(streamReader, stream);

        var firstMessage = await reader.ReadNextAsync(TestContext.Current.CancellationToken);
        var secondMessage = await reader.ReadNextAsync(TestContext.Current.CancellationToken);

        firstMessage.Should().NotBeNull("first message should be read successfully");
        secondMessage.Should().NotBeNull("second message should be read successfully");
    }

    [Fact]
    public async Task SlowConsumerDoesNotLoseMessages()
    {
        var streamReader = new NmeaStreamReader()
            .Register(new GGA());

        await using var stream = RH.GetStream<RealWorldDataTests>("TestData.track3.nmea");
        await using var reader = new NmeaReader(streamReader, stream);

        var messageCount = 0;
        await foreach (var message in reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            await Task.Delay(1, TestContext.Current.CancellationToken);
            messageCount++;
        }

        messageCount.Should().Be(39, "all GGA messages should be processed despite slow consumption");
    }
}
