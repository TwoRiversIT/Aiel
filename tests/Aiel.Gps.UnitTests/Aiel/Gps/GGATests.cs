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

public class GGATests
{
    [Fact]
    public void Can_parse_well_formed_sentence()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var gga = new GGA().Parse(buffer) as GGA;

        gga.Should().NotBeNull();
        gga.FixTime.Should().Be(new TimeOnly(23, 26, 8));
        gga.Latitude.Should().Be(50.953291666666665d);
        gga.Longitude.Should().Be(-111.58055333333333d);
        gga.Quality.Should().Be(FixQuality.DgpsFix);
        gga.NumberOfSatellites.Should().Be(8);
        gga.Hdop.Should().Be(1.06);
        gga.Altitude.Should().Be(781.7);
        gga.AltitudeUnits.Should().Be('M');
        gga.HeightOfGeoid.Should().Be(-18.1);
        gga.HeightOfGeoidUnits.Should().Be('M');
        gga.TimeSinceLastDgpsUpdate.Should().Be(TimeOnly.MinValue);
        gga.DgpsStationId.Should().Be(0);
        gga.Checksum.Should().Be(0x62);
    }
}
