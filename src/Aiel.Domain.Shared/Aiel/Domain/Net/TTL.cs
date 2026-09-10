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

using System.Globalization;

namespace Aiel.Domain.Net;

/// <summary>
/// Represents a Time To Live (TTL) value, which is typically used in
/// networking contexts to specify the duration or lifetime of data packets,
/// cache entries, or other resources.
/// </summary>
/// <remarks>
/// The TTL value is expressed as an integer number of seconds and must be
/// non-negative. This struct provides methods for parsing, comparing, and
/// converting TTL values, as well as generating human-readable
/// representations of the TTL duration.
/// </remarks>
public readonly record struct TTL : IEquatable<TTL>, IComparable<TTL>, IComparable
{
    private readonly Int32 _ttl;

    /// <summary>
    /// Initializes a new instance of the <see cref="TTL"/> struct with the specified TTL value.
    /// </summary>
    /// <param name="value"></param>
    public TTL(Int32 value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        _ttl = value;
    }

    /// <summary>
    /// Converts the TTL value to its integer representation.
    /// </summary>
    /// <returns>an <see cref="Int32"/> representing the TTL value in seconds.</returns>
    public Int32 ToInt32() => _ttl;

    /// <inheritdoc />
    public override Int32 GetHashCode() => _ttl.GetHashCode();

    /// <summary>
    /// Returns a string representation of the TTL value.
    /// </summary>
    /// <returns>A <see cref="String"/> representing the TTL value.</returns>
    public override String ToString() => _ttl.ToString(CultureInfo.InvariantCulture);
    /// <inheritdoc />
    public Int32 CompareTo(TTL other) => _ttl.CompareTo(other._ttl);
    /// <inheritdoc />
    public Int32 CompareTo(Object? obj) => (obj is TTL ttl) ? CompareTo(ttl) : 1;
    /// <inheritdoc />
    public Boolean Equals(TTL other) => _ttl == other._ttl;

    /// <summary>
    /// Returns a human-readable string representation of the TTL value, breaking it down into weeks, days, hours, minutes, and seconds.
    /// </summary>
    /// <returns>A <see cref="String"/> representing the TTL value in a human-readable format.</returns>
    public String HumanReadable()
    {
        var weeks = Math.Floor(_ttl / 604800D);
        var days = Math.Floor((_ttl - (weeks * 604800D)) / 86400D);
        var hours = Math.Floor((_ttl - (weeks * 604800D) - (days * 86400D)) / 3600);
        var minutes = Math.Floor((_ttl - (weeks * 604800D) - (days * 86400D) - (hours * 3600)) / 60);
        var seconds = _ttl - (weeks * 604800D) - (days * 86400D) - (hours * 3600) - (minutes * 60);
        var time = "";
        if (weeks > 0)
        {
            time += weeks + "w";
        }

        if (days > 0)
        {
            time += days + "d";
        }

        if (hours > 0)
        {
            time += hours + "h";
        }

        if (minutes > 0)
        {
            time += minutes + "m";
        }

        if (seconds > 0)
        {
            time += seconds + "s";
        }

        return _ttl > 0 ? time : "0";
    }

    /// <summary>
    /// Tries to parse the specified string into a <see cref="TTL"/> value.
    /// </summary>
    /// <param name="value">The string representation of the TTL value.</param>
    /// <param name="ttl">When this method returns, contains the parsed TTL value if the parsing succeeded, or the default value if the parsing failed.</param>
    /// <returns><c>true</c> if the string was successfully parsed; otherwise, <c>false</c>.</returns>
    public static Boolean TryParse(String value, out TTL ttl)
    {
        if (String.IsNullOrWhiteSpace(value))
        {
            ttl = default;
            return false;
        }

        if (Int32.TryParse(value, out var result))
        {
            ttl = result;
            return true;
        }

        ttl = default;
        return false;
    }

    /// <summary>
    /// Parses the specified string into a <see cref="TTL"/> value.
    /// </summary>
    /// <param name="value">The string representation of the TTL value.</param>
    /// <returns>The parsed <see cref="TTL"/> value.</returns>
    /// <exception cref="ArgumentException">Thrown when the string cannot be parsed into a <see cref="TTL"/> value.</exception>
    public static TTL Parse(String value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (Int32.TryParse(value, out var result))
        {
            return result;
        }

        throw new ArgumentException($"Don't know how to convert \"{value}\" into a TTL.");
    }

    /// <summary>
    /// Creates a new <see cref="TTL"/> instance from the specified integer value representing seconds.
    /// </summary>
    /// <param name="seconds">The number of seconds for the TTL value.</param>
    /// <returns>A new <see cref="TTL"/> instance representing the specified number of seconds.</returns>
    public static TTL FromInt32(Int32 seconds) => new(seconds);

    /// <summary>
    /// Defines an implicit conversion from a <see cref="TTL"/> to an <see cref="Int32"/>. This allows you to use a TTL instance wherever an integer is expected, and it will automatically convert to the underlying integer value representing the TTL in seconds.
    /// </summary>
    /// <param name="ttl">The <see cref="TTL"/> instance to convert.</param>
    public static implicit operator Int32(TTL ttl) => ttl._ttl;

    /// <summary>
    /// Defines an implicit conversion from an <see cref="Int32"/> to a <see cref="TTL"/>. This allows you to assign an integer directly to a TTL variable, and it will automatically create a new TTL instance representing that number of seconds.
    /// </summary>
    /// <param name="seconds">The number of seconds to convert.</param>
    public static implicit operator TTL(Int32 seconds) => new(seconds);

    /// <summary>
    /// Defines an implicit conversion from a <see cref="TTL"/> to a <see cref="String"/>. This allows you to use a TTL instance wherever a string is expected, and it will automatically convert to the string representation of the underlying integer value representing the TTL in seconds.
    /// </summary>
    /// <param name="ttl">The <see cref="TTL"/> instance to convert.</param>
    public static implicit operator String(TTL ttl) => ttl._ttl.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public static Boolean operator <=(TTL a, TTL b) => a._ttl <= b._ttl;
    /// <inheritdoc />
    public static Boolean operator >=(TTL a, TTL b) => a._ttl >= b._ttl;
    /// <inheritdoc />
    public static Boolean operator <(TTL a, TTL b) => a._ttl < b._ttl;
    /// <inheritdoc />
    public static Boolean operator >(TTL a, TTL b) => a._ttl > b._ttl;
    /// <inheritdoc />
    public static TTL operator +(TTL a, TTL b) => new(a._ttl + b._ttl);
    /// <inheritdoc />
    public static TTL operator -(TTL a, TTL b) => a < b ? new TTL(0) : new(a._ttl - b._ttl);
}
