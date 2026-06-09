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

namespace Aiel.Extensions;

public class IPAddressExtensionsTests
{
    [Theory]
    [InlineData("0.0.0.0", 0u)]
    [InlineData("0.0.0.1", 1u)]
    [InlineData("255.255.255.255", 4294967295u)]
    [InlineData("10.30.0.11", 169738251u)]
    [InlineData("10.250.0.1", 184156161u)]
    [InlineData("208.73.58.199", 3494460103u)]
    public void IpAddressToUInt32(String input, UInt32 expected)
        => input.IpAddressToUInt32().Should().Be(expected);

    [Theory]
    [InlineData(0u, "0.0.0.0")]
    [InlineData(1u, "0.0.0.1")]
    [InlineData(4294967294u, "255.255.255.254")]
    [InlineData(169738251u, "10.30.0.11")]
    [InlineData(184156161u, "10.250.0.1")]
    [InlineData(3494460103u, "208.73.58.199")]
    public void UInt32ToIpAddress(UInt32 input, String expected)
        => input.UInt32ToIpAddress().Should().Be(expected);

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("a.b.c.d")]
    [InlineData("256.256.256.256")]
    public void IpAddressToUInt32_throws_FormatException_on_invalid_IPv4_Address(String input)
        => Assert.Throws<FormatException>(() => input.IpAddressToUInt32());
}
