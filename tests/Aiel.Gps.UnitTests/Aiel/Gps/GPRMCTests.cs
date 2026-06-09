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

public class GPRMCTests
{
    [Fact]
    public void Can_parse_well_formed_sentence()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPRMC,064659.803,A,5228.174,N,01318.771,E,038.9,144.8,171118,000.0,W,A*78\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var gsa = new RMC().Parse(buffer) as RMC;

        gsa.Should().NotBeNull();

        gsa.FixTime.Should().Be(new TimeOnly(6, 46, 59, 803));
        gsa.Status.Should().Be('A');
        gsa.Latitude.Should().BeApproximately(52.46956666666667, 0.000001);
        gsa.Longitude.Should().BeApproximately(13.31285, 0.000001);
        gsa.SpeedOverGround.Should().BeApproximately(38.9, 0.1);
        gsa.TrackAngle.Should().BeApproximately(144.8, 0.1);
        gsa.Date.Should().Be(new DateOnly(2018, 11, 17));
        gsa.MagneticVariation.Should().BeApproximately(0.0, 0.1);
        gsa.Direction.Should().Be('W');
        gsa.Mode.Should().Be('A');
        gsa.Checksum.Should().Be(0x78);
    }
}
