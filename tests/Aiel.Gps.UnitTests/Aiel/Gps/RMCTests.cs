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

public class RMCTests
{
    [Fact]
    public void Can_parse_well_formed_sentence()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPRMC,063321.803,A,5234.906,N,01318.184,E,4948.6,043.5,171118,,W*47\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var rmc = new RMC().Parse(buffer) as RMC;

        rmc.Should().NotBeNull();
        rmc.FixTime.Should().Be(new TimeOnly(6, 33, 21, 803));
        rmc.Status.Should().Be('A');
        rmc.Latitude.Should().Be(52.58176666666667d);
        rmc.Longitude.Should().Be(13.303066666666666d);
        rmc.SpeedOverGround.Should().Be(4948.6);
        rmc.TrackAngle.Should().Be(43.5);
        rmc.Date.Should().Be(new DateOnly(2018, 11, 17));
        rmc.MagneticVariation.Should().Be(Double.NaN);
        rmc.Direction.Should().Be('W');
        rmc.Checksum.Should().Be(0x47);
    }

    [Fact]
    public void UBlox_G70xx_happy_path()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPRMC,182630.00,A,4955.65790,N,11926.34845,W,0.045,,051218,,,D*64\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var rmc = new RMC().Parse(buffer) as RMC;

        rmc.Should().NotBeNull();
        rmc.FixTime.Should().Be(new TimeOnly(18, 26, 30));
        rmc.Status.Should().Be('A');
        rmc.Latitude.Should().Be(49.92763166666667d);
        rmc.Longitude.Should().Be(-119.43914083333334d);
        rmc.SpeedOverGround.Should().Be(0.045d);
        rmc.TrackAngle.Should().Be(Double.NaN);
        rmc.Date.Should().Be(new DateOnly(2018, 12, 5));
        rmc.MagneticVariation.Should().Be(Double.NaN);
        rmc.Direction.Should().Be(Char.MinValue);
        rmc.Mode.Should().Be('D');
        rmc.Checksum.Should().Be(0x64);
    }

    [Fact]
    public void UBlox_G70xx_incomplete_fix()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPRMC,174114.00,V,,,,,,,051218,,,N*74\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var rmc = new RMC().Parse(buffer) as RMC;

        rmc.Should().NotBeNull();
        rmc.FixTime.Should().Be(new TimeOnly(17, 41, 14));
        rmc.Status.Should().Be('V');
        rmc.Latitude.Should().Be(Double.NaN);
        rmc.Longitude.Should().Be(Double.NaN);
        rmc.SpeedOverGround.Should().Be(Double.NaN);
        rmc.TrackAngle.Should().Be(Double.NaN);
        rmc.Date.Should().Be(new DateOnly(2018, 12, 5));
        rmc.MagneticVariation.Should().Be(Double.NaN);
        rmc.Direction.Should().Be(Char.MinValue);
        rmc.Mode.Should().Be('N');
        rmc.Checksum.Should().Be(0x74);
    }

    [Fact]
    public void NovAtel_happy_path()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPRMC,144326.00,A,5107.0017737,N,11402.3291611,W,0.080,323.3,210307,0.0,E,A*20\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var rmc = new RMC().Parse(buffer) as RMC;

        rmc.Should().NotBeNull();
        rmc.FixTime.Should().Be(new TimeOnly(14, 43, 26));
        rmc.Status.Should().Be('A');
        rmc.Latitude.Should().Be(51.11669622833333d);
        rmc.Longitude.Should().Be(-114.03881935166666d);
        rmc.SpeedOverGround.Should().Be(0.080d);
        rmc.TrackAngle.Should().Be(323.3);
        rmc.Date.Should().Be(new DateOnly(2007, 03, 21));
        rmc.MagneticVariation.Should().Be(0.0);
        rmc.Direction.Should().Be('E');
        rmc.Mode.Should().Be('A');
        rmc.Checksum.Should().Be(0x20);
    }
}
