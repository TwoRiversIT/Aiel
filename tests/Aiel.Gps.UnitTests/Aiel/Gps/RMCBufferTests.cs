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

public class RMCBufferTests
{
    [Fact]
    public void Parse_with_CRLF_ending()
    {
        // This is how the unit test does it
        var bytes = Encoding.UTF8.GetBytes("$GPRMC,064659.803,A,5228.174,N,01318.771,E,038.9,144.8,171118,000.0,W*78\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var rmc = new RMC().Parse(buffer) as RMC;

        rmc.Should().NotBeNull();
        rmc.Checksum.Should().Be(0x78);
    }

    [Fact]
    public void Parse_with_CR_ending_only()
    {
        // This is how the integration test provides it (sliced at LF)
        var bytes = Encoding.UTF8.GetBytes("$GPRMC,064659.803,A,5228.174,N,01318.771,E,038.9,144.8,171118,000.0,W*78\r");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var rmc = new RMC().Parse(buffer) as RMC;

        rmc.Should().NotBeNull();
        rmc.Checksum.Should().Be(0x78);
    }

    [Fact]
    public void Parse_without_line_endings()
    {
        // No line ending at all
        var bytes = Encoding.UTF8.GetBytes("$GPRMC,064659.803,A,5228.174,N,01318.771,E,038.9,144.8,171118,000.0,W*78");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var rmc = new RMC().Parse(buffer) as RMC;

        rmc.Should().NotBeNull();
        rmc.Checksum.Should().Be(0x78);
    }
}
