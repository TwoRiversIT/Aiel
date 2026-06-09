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

using System.Buffers;
using System.Text;

namespace Aiel.Gps;

public class GGAErrorHandlingTests
{
    [Fact]
    public void Parse_instance_with_default_values_on_empty_sentence()
    {
        var bytes = Encoding.UTF8.GetBytes("\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var parsed = new GGA().Parse(buffer) as GGA;

        parsed.Should().NotBeNull();
        parsed.FixTime.Should().Be(default);
        parsed.Latitude.Should().Be(Double.NaN);
        parsed.Longitude.Should().Be(Double.NaN);
        parsed.Quality.Should().Be(default);
        parsed.NumberOfSatellites.Should().Be(default);
        parsed.Hdop.Should().Be(Double.NaN);
        parsed.Altitude.Should().Be(Double.NaN);
        parsed.AltitudeUnits.Should().Be(default);
        parsed.HeightOfGeoid.Should().Be(Double.NaN);
        parsed.HeightOfGeoidUnits.Should().Be(default);
        parsed.TimeSinceLastDgpsUpdate.Should().Be(default);
        parsed.DgpsStationId.Should().Be(default);
        parsed.Checksum.Should().Be(default);
    }

    [Fact]
    public void Parse_handles_truncated_sentence_gracefully()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPGGA,232608.000*62\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var gga = new GGA().Parse(buffer) as GGA;

        gga.Should().NotBeNull();
        gga!.FixTime.Should().Be(new TimeOnly(23, 26, 8));
    }

    [Fact]
    public void Parse_handles_empty_optional_fields()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,,*69\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var gga = new GGA().Parse(buffer) as GGA;

        gga.Should().NotBeNull();
        gga!.DgpsStationId.Should().Be(0);
    }

    [Fact]
    public void CanHandle_returns_false_for_wrong_sentence()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPRMC,081836,A,3751.65,S,14507.36,E,000.0,360.0,130998,011.3,E*62\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var canHandle = new GGA().CanHandle(buffer);

        canHandle.Should().BeFalse();
    }

    [Fact]
    public void CanHandle_returns_false_for_empty_sentence()
    {
        var buffer = new ReadOnlySequence<Byte>([]);

        var canHandle = new GGA().CanHandle(buffer);

        canHandle.Should().BeFalse();
    }

    [Fact]
    public void CanHandle_returns_true_for_valid_sentence()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var canHandle = new GGA().CanHandle(buffer);

        canHandle.Should().BeTrue();
    }
}
