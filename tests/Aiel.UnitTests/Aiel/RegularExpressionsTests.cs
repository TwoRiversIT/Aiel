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

namespace Aiel;

public class RegularExpressionsTests
{
    [Fact]
    public void PortNumber()
    {
        Rgx.PortNumber().IsMatch("0").Should().BeTrue();
        Rgx.PortNumber().IsMatch("1").Should().BeTrue();
        Rgx.PortNumber().IsMatch("9").Should().BeTrue();
        Rgx.PortNumber().IsMatch("10").Should().BeTrue();
        Rgx.PortNumber().IsMatch("99").Should().BeTrue();
        Rgx.PortNumber().IsMatch("100").Should().BeTrue();
        Rgx.PortNumber().IsMatch("999").Should().BeTrue();
        Rgx.PortNumber().IsMatch("1000").Should().BeTrue();
        Rgx.PortNumber().IsMatch("9999").Should().BeTrue();
    }

    [Fact]
    public void IPv4()
    {
        Rgx.IPv4().IsMatch("0.0.0.0").Should().BeTrue();
        Rgx.IPv4().IsMatch("00.00.00.00").Should().BeTrue();
        Rgx.IPv4().IsMatch("000.000.000.000").Should().BeTrue();
        Rgx.IPv4().IsMatch("1.1.1.1").Should().BeTrue();
        Rgx.IPv4().IsMatch("01.01.01.01").Should().BeTrue();
        Rgx.IPv4().IsMatch("001.001.001.001").Should().BeTrue();

        // Netmask
        Rgx.IPv4().IsMatch("255.255.255.255").Should().BeTrue();

        // Private networks
        Rgx.IPv4().IsMatch("10.10.10.10").Should().BeTrue();
        Rgx.IPv4().IsMatch("172.16.0.0").Should().BeTrue();
        Rgx.IPv4().IsMatch("192.168.0.0").Should().BeTrue();

        // Bad IPs - First Octet
        Rgx.IPv4().IsMatch("355.255.255.255").Should().BeFalse();
        Rgx.IPv4().IsMatch("265.255.255.255").Should().BeFalse();
        Rgx.IPv4().IsMatch("256.255.255.255").Should().BeFalse();

        // Bad IPs - Second Octet
        Rgx.IPv4().IsMatch("255.355.255.255").Should().BeFalse();
        Rgx.IPv4().IsMatch("255.265.255.255").Should().BeFalse();
        Rgx.IPv4().IsMatch("255.256.255.255").Should().BeFalse();

        // Bad IPs - Third Octet
        Rgx.IPv4().IsMatch("255.255.355.255").Should().BeFalse();
        Rgx.IPv4().IsMatch("255.255.265.255").Should().BeFalse();
        Rgx.IPv4().IsMatch("255.255.256.255").Should().BeFalse();

        // Bad IPs - Fourth Octet
        Rgx.IPv4().IsMatch("255.255.255.355").Should().BeFalse();
        Rgx.IPv4().IsMatch("255.255.255.265").Should().BeFalse();
        Rgx.IPv4().IsMatch("255.255.255.256").Should().BeFalse();
    }

    [Fact]
    public void IPv4AndPort()
    {
        Rgx.IPv4AndPort().IsMatch("10.10.10.10:0").Should().BeTrue();
        Rgx.IPv4AndPort().IsMatch("10.10.10.10:1").Should().BeTrue();
        Rgx.IPv4AndPort().IsMatch("10.10.10.10:10").Should().BeTrue();
        Rgx.IPv4AndPort().IsMatch("10.10.10.10:100").Should().BeTrue();
        Rgx.IPv4AndPort().IsMatch("10.10.10.10:1000").Should().BeTrue();
        Rgx.IPv4AndPort().IsMatch("10.10.10.10:10000").Should().BeTrue();
        Rgx.IPv4AndPort().IsMatch("10.10.10.10:65535").Should().BeTrue();

        Rgx.IPv4AndPort().IsMatch("10.10.10.10:65536").Should().BeFalse();
        Rgx.IPv4AndPort().IsMatch("10.10.10.10:65545").Should().BeFalse();
        Rgx.IPv4AndPort().IsMatch("10.10.10.10:65635").Should().BeFalse();
        Rgx.IPv4AndPort().IsMatch("10.10.10.10:66535").Should().BeFalse();
        Rgx.IPv4AndPort().IsMatch("10.10.10.10:75535").Should().BeFalse();
        Rgx.IPv4AndPort().IsMatch("10.10.10.10:100000").Should().BeFalse();
    }

    [Fact]
    public void IPv4AndOptionalPort()
    {
        Rgx.IPv4AndOptionalPort().IsMatch("0.0.0.0").Should().BeTrue();
        Rgx.IPv4AndOptionalPort().IsMatch("10.10.10.10").Should().BeTrue();
        Rgx.IPv4AndOptionalPort().IsMatch("255.255.255.255").Should().BeTrue();

        Rgx.IPv4AndOptionalPort().IsMatch("10.10.10.10:0").Should().BeTrue();
        Rgx.IPv4AndOptionalPort().IsMatch("10.10.10.10:1").Should().BeTrue();
        Rgx.IPv4AndOptionalPort().IsMatch("10.10.10.10:65535").Should().BeTrue();

        Rgx.IPv4AndOptionalPort().IsMatch("256.255.255.255").Should().BeFalse();
        Rgx.IPv4AndOptionalPort().IsMatch("255.255.255.255:65536").Should().BeFalse();
    }
}
