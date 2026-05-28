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

public class VTGTests
{
    [Fact]
    public void Can_parse_well_formed_sentence()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPVTG,054.7,T,034.4,M,005.5,N,010.2,K*48\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var vtg = new VTG().Parse(buffer) as VTG;

        vtg.Should().NotBeNull();
        vtg.TrueTrack.Should().Be(54.7);
        vtg.MagneticTrack.Should().Be(34.4);
        vtg.GroundSpeedN.Should().Be(5.5);
        vtg.GroundSpeedK.Should().Be(10.2);
        vtg.Checksum.Should().Be(0x48);
    }
}
