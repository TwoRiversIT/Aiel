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

using System.Net;

namespace Aiel.Extensions;

/// <summary>
/// Extension methods for IP address operations.
/// </summary>
public static class IPAddressExtensions
{
    // Modified from [Saeb Amini](https://stackoverflow.com/users/68080/saeb-amini) via [StackOverflow](https://stackoverflow.com/a/13350494/32588)

    /// <summary>
    /// Converts a valid IPv4 address string to a <see cref="UInt32"/>.
    /// </summary>
    /// <param name="ipAddress">A string that contains an IPv4 address in dotted-quad notation.</param>
    /// <returns>A <see cref="UInt32"/> representing the IP address.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ipAddress"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="ipAddress"/> is not a valid IPv4 address.</exception>
    /// <remarks>
    /// <para>IP addresses are in network byte order (big-endian), while <see cref="UInt32"/> values are stored in the system's native byte order.
    /// On little-endian systems (such as Windows), the bytes are reversed to ensure correct conversion.</para>
    /// <para>A <see cref="UInt32"/> is used because even for IPv4, a <see cref="Int32"/> cannot hold addresses larger than 127.255.255.255.</para>
    /// </remarks>
    public static UInt32 IpAddressToUInt32(this String ipAddress)
    {
        var bytes = IPAddress.Parse(ipAddress).GetAddressBytes();

        if (BitConverter.IsLittleEndian)
        {
            // Flip big-endian (Network Order) to little-endian
            Array.Reverse(bytes);
        }

        return BitConverter.ToUInt32(bytes, 0);
    }

    /// <summary>
    /// Converts a <see cref="UInt32"/> to an IPv4 address string.
    /// </summary>
    /// <param name="ipAddress">The <see cref="UInt32"/> value to convert.</param>
    /// <returns>A string representation of the IPv4 address.</returns>
    /// <remarks>
    /// This method reverses the byte conversion process performed by <see cref="IpAddressToUInt32"/>,
    /// handling endianness conversion as needed.
    /// </remarks>
    public static String UInt32ToIpAddress(this UInt32 ipAddress)
    {
        var bytes = BitConverter.GetBytes(ipAddress);

        if (BitConverter.IsLittleEndian)
        {
            // Flip little-endian to big-endian (Network Order)
            Array.Reverse(bytes);
        }

        return new IPAddress(bytes).ToString();
    }
}
