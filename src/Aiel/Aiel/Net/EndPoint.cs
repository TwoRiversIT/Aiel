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

// TODO: Create an IPv6 version of this struct, and a version that can handle both IPv4 and IPv6.

/// <summary>
/// Represents a network endpoint consisting of an IPv4 address and optional port.
/// </summary>
public readonly struct EndPoint : IEquatable<EndPoint>, IComparable<EndPoint>
{
    /// <summary>
    /// Gets an empty <see cref="EndPoint"/> with no address or port.
    /// </summary>
    public static readonly EndPoint Empty = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EndPoint"/> struct with an address and no port.
    /// </summary>
    /// <param name="address">A valid IPv4 address.</param>
    /// <exception cref="ArgumentException"><paramref name="address"/> is not a valid IPv4 address.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="address"/> is <see langword="null"/>.</exception>
    public EndPoint(String address) : this(address, 0) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EndPoint"/> struct with an address and port.
    /// </summary>
    /// <param name="address">A valid IPv4 address.</param>
    /// <param name="port">A valid port number (0-65535).</param>
    /// <exception cref="ArgumentException"><paramref name="address"/> is not a valid IPv4 address.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="address"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within the range 0-65535.</exception>
    public EndPoint(String address, Int32 port)
    {
        if (String.IsNullOrWhiteSpace(address) || !Rgx.IPv4().IsMatch(address))
        {
            throw new ArgumentException($"'{address}' is not a valid IPv4 Address", nameof(address));
        }

        if (port is < 0 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), $"'{port}' is not a valid IPv4 Port");
        }

        Address = address ?? throw new ArgumentNullException(nameof(address));
        Port = port;
    }

    /// <summary>
    /// Gets the IPv4 address component of the endpoint.
    /// </summary>
    public String Address { get; }

    /// <summary>
    /// Gets the port component of the endpoint.
    /// </summary>
    public Int32 Port { get; }

    /// <summary>
    /// Returns the string representation of the endpoint in the format "address:port" or just "address" if port is 0.
    /// </summary>
    /// <returns>A string representation of the endpoint.</returns>
    public override String ToString() => Port == 0 ? Address : $"{Address}:{Port}";

    /// <summary>
    /// Compares the current endpoint with another endpoint.
    /// </summary>
    /// <param name="other">The endpoint to compare with.</param>
    /// <returns>A value indicating the relative order of the endpoints.</returns>
    /// <remarks>
    /// Endpoints are compared first by IP address, then by port.
    /// An empty endpoint is considered greater than any non-empty endpoint.
    /// </remarks>
    public Int32 CompareTo(EndPoint other)
    {
        // If other is Empty, this instance is greater.
        if (other.Equals(Empty))
        {
            return 1;
        }

        var result = IPAddressComparer.Compare(Address, other.Address);

        return result == 0 ? Port.CompareTo(other.Port) : result;
    }

    /// <summary>
    /// Determines whether the current endpoint is greater than another endpoint.
    /// </summary>
    /// <param name="x">The first endpoint.</param>
    /// <param name="y">The second endpoint.</param>
    /// <returns><see langword="true"/> if x is greater than y; otherwise, <see langword="false"/>.</returns>
    public static Boolean operator >(EndPoint x, EndPoint y) => x.CompareTo(y) == 1;

    /// <summary>
    /// Determines whether the current endpoint is less than another endpoint.
    /// </summary>
    /// <param name="x">The first endpoint.</param>
    /// <param name="y">The second endpoint.</param>
    /// <returns><see langword="true"/> if x is less than y; otherwise, <see langword="false"/>.</returns>
    public static Boolean operator <(EndPoint x, EndPoint y) => x.CompareTo(y) == -1;

    /// <summary>
    /// Determines whether the current endpoint is greater than or equal to another endpoint.
    /// </summary>
    /// <param name="x">The first endpoint.</param>
    /// <param name="y">The second endpoint.</param>
    /// <returns><see langword="true"/> if x is greater than or equal to y; otherwise, <see langword="false"/>.</returns>
    public static Boolean operator >=(EndPoint x, EndPoint y) => x.CompareTo(y) >= 0;

    /// <summary>
    /// Determines whether the current endpoint is less than or equal to another endpoint.
    /// </summary>
    /// <param name="x">The first endpoint.</param>
    /// <param name="y">The second endpoint.</param>
    /// <returns><see langword="true"/> if x is less than or equal to y; otherwise, <see langword="false"/>.</returns>
    public static Boolean operator <=(EndPoint x, EndPoint y) => x.CompareTo(y) <= 0;

    /// <summary>
    /// Returns the hash code for the endpoint.
    /// </summary>
    /// <returns>A hash code for the endpoint.</returns>
    public override Int32 GetHashCode() => (Address, Port).GetHashCode();

    /// <summary>
    /// Determines whether the current endpoint is equal to another endpoint.
    /// </summary>
    /// <param name="other">The endpoint to compare with.</param>
    /// <returns><see langword="true"/> if the endpoints are equal; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Two endpoints are considered equal if they have the same address and (either have the same port or one has port 0).
    /// </remarks>
    public Boolean Equals(EndPoint other) => Address == other.Address && (Port == 0 || other.Port == 0 || Port == other.Port);

    /// <summary>
    /// Determines whether the current endpoint is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns><see langword="true"/> if the object is an endpoint equal to this one; otherwise, <see langword="false"/>.</returns>
    public override Boolean Equals(Object? obj) => obj is EndPoint other && Equals(other);

    /// <summary>
    /// Determines whether two endpoints are equal.
    /// </summary>
    /// <param name="x">The first endpoint.</param>
    /// <param name="y">The second endpoint.</param>
    /// <returns><see langword="true"/> if the endpoints are equal; otherwise, <see langword="false"/>.</returns>
    public static Boolean operator ==(EndPoint x, EndPoint y) => x.Equals(y);

    /// <summary>
    /// Determines whether two endpoints are not equal.
    /// </summary>
    /// <param name="x">The first endpoint.</param>
    /// <param name="y">The second endpoint.</param>
    /// <returns><see langword="true"/> if the endpoints are not equal; otherwise, <see langword="false"/>.</returns>
    public static Boolean operator !=(EndPoint x, EndPoint y) => !x.Equals(y);

    /// <summary>
    /// Parses a string representation of an endpoint.
    /// </summary>
    /// <param name="value">A string in the format "address" or "address:port".</param>
    /// <returns>An <see cref="EndPoint"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> cannot be parsed as an endpoint.</exception>
    public static EndPoint Parse(String value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (TryParse(value, out var endPoint))
        {
            return endPoint;
        }

        throw new ArgumentException($"'{value}' cannot be parsed as an EndPoint", nameof(value));
    }

    /// <summary>
    /// Attempts to parse a string representation of an endpoint.
    /// </summary>
    /// <param name="value">A string in the format "address" or "address:port".</param>
    /// <param name="endPoint">When the method returns, contains the parsed <see cref="EndPoint"/>, or <see cref="Empty"/> if parsing failed.</param>
    /// <returns><see langword="true"/> if the string was successfully parsed; otherwise, <see langword="false"/>.</returns>
    public static Boolean TryParse(String value, out EndPoint endPoint)
    {
        if (!String.IsNullOrWhiteSpace(value))
        {
            var normalized = value.Trim();
            if (Rgx.IPv4AndOptionalPort().IsMatch(normalized))
            {
                var tokens = normalized.Split([':'], StringSplitOptions.RemoveEmptyEntries);
                var portNumber = tokens.Length == 2 ? Int32.Parse(tokens[1]) : 0;
                endPoint = new EndPoint(tokens[0], portNumber);
                return true;
            }
        }

        endPoint = Empty;
        return false;
    }
}
