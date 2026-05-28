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

namespace Aiel.Net;

/// <summary>
/// Compares IPv4 addresses represented as strings by comparing each octet numerically.
/// </summary>
public class IPAddressComparer : IComparer<String>
{
    /// <summary>
    /// Compares two IPv4 address strings.
    /// </summary>
    /// <param name="left">The first IPv4 address string to compare.</param>
    /// <param name="right">The second IPv4 address string to compare.</param>
    /// <returns>A value indicating the relative order of the addresses.</returns>
    Int32 IComparer<String>.Compare(String? left, String? right)
        => Compare(left, right);

    /// <summary>
    /// Compares two IPv4 address strings.
    /// </summary>
    /// <param name="left">The first IPv4 address string to compare.</param>
    /// <param name="right">The second IPv4 address string to compare.</param>
    /// <returns>A value indicating the relative order of the addresses.</returns>
    /// <remarks>
    /// Null values are considered less than non-null values.
    /// IP addresses are compared octet-by-octet from left to right.
    /// </remarks>
    public static Int32 Compare(String? left, String? right)
    {
        if (left == null && right == null)
        {
            return 0;
        }

        if (left == null)
        {
            return -1;
        }

        if (right == null)
        {
            return 1;
        }

        var l = Split(left);
        var r = Split(right);

        if (l[0] == r[0])
        {
            if (l[1] == r[1])
            {
                if (l[2] == r[2])
                {
                    return l[3].CompareTo(r[3]);
                }

                return l[2].CompareTo(r[2]);
            }

            return l[1].CompareTo(r[1]);
        }

        return l[0].CompareTo(r[0]);
    }

    /// <summary>
    /// Splits an IPv4 address string into four octets as integers.
    /// </summary>
    /// <param name="value">An IPv4 address string in dotted-quad notation.</param>
    /// <returns>An array of four integers representing the octets.</returns>
    private static Int32[] Split(String value)
        => value.Split('.').Select(Int32.Parse).ToArray();
}
