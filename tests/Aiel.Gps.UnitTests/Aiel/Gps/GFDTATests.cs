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

public class GFDTATests
{
    [Fact]
    public void Can_parse_well_formed_sentence()
    {
        // Note the leading and trailing spaces in the second field. Not really valid for NMEA, but we should be able to parse it anyway.
        var bytes = Encoding.UTF8.GetBytes("$GFDTA, 5.0 ,98,2.0,12941,2004/10/29 18:49:55,CH4AB-1015,1*47\r\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var gcp = new GFDTA().Parse(buffer) as GFDTA;

        gcp.Should().NotBeNull();
        gcp.Concentration.Should().Be(5.0d);
        gcp.R2.Should().Be(98);
        gcp.Distance.Should().Be(2.0d);
        gcp.Light.Should().Be(12941);
        gcp.SerialNumber.AsString().Should().Be("CH4AB-1015");
        gcp.Status.AsString().Should().Be("1");
        gcp.DateTime.Should().Be(new DateTime(2004, 10, 29, 18, 49, 55));
        gcp.Checksum.Should().Be(0x47);
    }

    [Fact]
    public void Can_parse_another_well_formed_sentence()
    {
        var bytes = Encoding.UTF8.GetBytes("$GFDTA,     6.8,98,3.0,16296,2019/01/22 14:50:48, CH4AB-1047,1*41\n\n\n\n\n\n");
        var buffer = new ReadOnlySequence<Byte>(bytes);

        var gcp = new GFDTA().Parse(buffer) as GFDTA;

        gcp.Should().NotBeNull();
        gcp.Concentration.Should().Be(6.8d);
        gcp.R2.Should().Be(98);
        gcp.Distance.Should().Be(3.0d);
        gcp.Light.Should().Be(16296);
        gcp.SerialNumber.AsString().Should().Be("CH4AB-1047");
        gcp.Status.AsString().Should().Be("1");
        gcp.DateTime.Should().Be(new DateTime(2019, 01, 22, 14, 50, 48));
        gcp.Checksum.Should().Be(0x41);
    }
}
