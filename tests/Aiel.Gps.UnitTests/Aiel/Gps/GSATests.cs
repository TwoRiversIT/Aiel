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

using Aiel.GPS;
using System.Buffers;
using System.Text;

namespace Aiel.Gps;

public class GSATests
{
    [Fact]
    public void Can_parse_well_formed_sentence()
    {
        var bytes = Encoding.UTF8.GetBytes("$GPGSA,A,3,,,,,,16,18,,22,24,,,3.6,2.1,2.2*3C\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var gsa = new GSA().Parse(buffer) as GSA;

        gsa.Should().NotBeNull();
        gsa.FixMode.Should().Be('A');
        gsa.FixType.Should().Be(FixType.Fix3D);
        gsa.SV.Should().Contain(16);
        gsa.SV.Should().Contain(18);
        gsa.SV.Should().Contain(22);
        gsa.SV.Should().Contain(24);
        gsa.Pdop.Should().Be(3.6d);
        gsa.Hdop.Should().Be(2.1d);
        gsa.Vdop.Should().Be(2.2d);
        gsa.Checksum.Should().Be(0x3C);
    }
}
