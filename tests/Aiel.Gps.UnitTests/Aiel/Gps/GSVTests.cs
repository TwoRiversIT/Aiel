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

public class GSVTests
{
    [Fact]
    public void Can_parse_well_formed_sentence()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPGSV,3,1,11,03,03,111,00,04,15,270,00,06,01,010,00,13,06,292,00*74\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var gsv = new GSV().Parse(buffer) as GSV;

        gsv.Should().NotBeNull();
        gsv.TotalMessages.Should().Be(3);
        gsv.MessageNumber.Should().Be(1);
        gsv.SatellitesInView.Should().Be(11);

        gsv.SV1.PRN.Should().Be(3);
        gsv.SV1.Elevation.Should().Be(3);
        gsv.SV1.Azimuth.Should().Be(111);
        gsv.SV1.SNR.Should().Be(0);

        gsv.SV2!.PRN.Should().Be(4);
        gsv.SV2.Elevation.Should().Be(15);
        gsv.SV2.Azimuth.Should().Be(270);
        gsv.SV2.SNR.Should().Be(0);

        gsv.SV3!.PRN.Should().Be(6);
        gsv.SV3.Elevation.Should().Be(1);
        gsv.SV3.Azimuth.Should().Be(10);
        gsv.SV3.SNR.Should().Be(0);

        gsv.SV4!.PRN.Should().Be(13);
        gsv.SV4.Elevation.Should().Be(6);
        gsv.SV4.Azimuth.Should().Be(292);
        gsv.SV4.SNR.Should().Be(0);

        gsv.Checksum.Should().Be(0x74);
    }

    [Fact]
    public void Can_handle_short_sentence()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPGSV,4,4,13,31,02,340,24*4A\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var gsv = new GSV().Parse(buffer) as GSV;

        gsv.Should().NotBeNull();
        gsv.TotalMessages.Should().Be(4);
        gsv.MessageNumber.Should().Be(4);
        gsv.SatellitesInView.Should().Be(13);

        gsv.SV1.PRN.Should().Be(31);
        gsv.SV1.Elevation.Should().Be(2);
        gsv.SV1.Azimuth.Should().Be(340);
        gsv.SV1.SNR.Should().Be(24);

        gsv.SV2.Should().BeNull();
        gsv.SV3.Should().BeNull();
        gsv.SV4.Should().BeNull();

        gsv.Checksum.Should().Be(0x4A);
    }
}
