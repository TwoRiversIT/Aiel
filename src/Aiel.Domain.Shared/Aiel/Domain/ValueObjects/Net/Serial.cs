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
using System.Text.RegularExpressions;

namespace Aiel.Domain.Net;

/// <summary>
/// Represents a serial number in the format YYYYMMDDSS, where YYYY is the year, MM is the month, DD is the day, and SS is a sequence number. This struct provides methods for creating, incrementing, and comparing serial numbers.
/// </summary>
public readonly partial struct Serial : IEquatable<Serial>, IComparable<Serial>
{
    private readonly Int32 _stableHashCode;

    private readonly Int32 _year;
    private readonly Int32 _month;
    private readonly Int32 _day;
    private readonly Int32 _sequence;

    /// <summary>
    /// Gets the epoch serial number, which represents January 1, 1970, with a sequence number of 0.
    /// </summary>
    public static readonly Serial Epoch = new(1970, 1, 1, 0);

    private Serial(Int32 year, Int32 month, Int32 day, Int32 sequence)
    {
        _year = year;
        _month = month;
        _day = day;
        _sequence = sequence;

        _stableHashCode = -1221578130;
        _stableHashCode = (_stableHashCode * -1521134295) + _year.GetHashCode();
        _stableHashCode = (_stableHashCode * -1521134295) + _month.GetHashCode();
        _stableHashCode = (_stableHashCode * -1521134295) + _day.GetHashCode();
        _stableHashCode = (_stableHashCode * -1521134295) + _sequence.GetHashCode();
    }

    /// <summary>
    /// Increments the serial number based on the specified <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="utc">The date and time for which to increment the serial number.</param>
    /// <returns>A new <see cref="Serial"/> instance with the incremented value.</returns>
    [SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Readability")]
    public Serial Increment(DateTimeOffset utc)
    {
        var serial = From(utc);

        if (serial > this)
        {
            return serial;
        }

        if (_sequence + 1 > 99)
        {
            if (_day + 1 > DateTime.DaysInMonth(_year, _month))
            {
                if (_month + 1 > 12)
                {
                    serial = new Serial(_year + 1, 1, 1, 0);
                }
                else
                {
                    serial = new Serial(_year, _month + 1, 1, 0);
                }
            }
            else
            {
                serial = new Serial(_year, _month, _day + 1, 0);
            }
        }
        else
        {
            serial = new Serial(_year, _month, _day, _sequence + 1);
        }

        return serial;
    }

    private static UInt32 ToUInt32(Int32 year, Int32 month, Int32 day, Int32 sequence)
        => Convert.ToUInt32((year * 1000000u) + (month * 10000u) + (day * 100u) + sequence);

    /// <inheritdoc />
    public override Int32 GetHashCode() => _stableHashCode;

    /// <inheritdoc />
    public override Boolean Equals(Object? obj) => obj is Serial other && _year == other._year && _month == other._month && _day == other._day && _sequence == other._sequence;

    /// <inheritdoc />
    public Boolean Equals(Serial other) => _year == other._year && _month == other._month && _day == other._day && _sequence == other._sequence;

    /// <summary>
    /// Compares the current <see cref="Serial"/> instance with another <see cref="Serial"/> instance and returns an integer that indicates their relative order. A value less than zero indicates that the current instance precedes the other in the sort order, a value of zero indicates that they are equal, and a value greater than zero indicates that the current instance follows the other in the sort order.
    /// </summary>
    /// <param name="other">The other <see cref="Serial"/> instance to compare with.</param>
    /// <returns>An integer that indicates the relative order of the instances.</returns>
    public Int32 CompareTo(Serial other) => ToUInt32().CompareTo(other.ToUInt32());

    /// <summary>
    /// Returns a string representation of the serial number based on the specified format. The format "F" returns the serial number in the format "YYYY-MM-DD-SS", while any other format returns the serial number as a UInt32 string.
    /// </summary>
    /// <param name="format">The format specifier.</param>
    /// <returns>A string representation of the serial number.</returns>
    public String ToString(String format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);
        return format switch
        {
            "F" => $"{_year}-{_month.ToString("00", CultureInfo.InvariantCulture)}-{_day.ToString("00", CultureInfo.InvariantCulture)}-{_sequence.ToString("00", CultureInfo.InvariantCulture)}",
            _ => ToUInt32().ToString(CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Returns a string representation of the serial number in the default format "F" (YYYY-MM-DD-SS).
    /// </summary>
    /// <returns>A string representation of the serial number.</returns>
    public override String ToString() => ToString("F");

    /// <summary>
    /// Converts the current <see cref="Serial"/> instance to a <see cref="UInt32"/> representation.
    /// </summary>
    /// <returns>A <see cref="UInt32"/> representation of the serial number.</returns>
    public UInt32 ToUInt32() => ToUInt32(_year, _month, _day, _sequence);

    /// <summary>
    /// Attempts to parse a string representation of a serial number and returns a boolean indicating whether the parsing was successful.
    /// The input string can contain non-numeric characters, which will be ignored during parsing. If the parsing is successful, the
    /// resulting <see cref="Serial"/> instance is returned through the out parameter.
    /// </summary>
    /// <param name="candidate">The string representation of the serial number.</param>
    /// <param name="serial">When this method returns, contains the parsed <see cref="Serial"/> instance if the parsing was successful; otherwise, the default value.</param>
    /// <returns><c>true</c> if the parsing was successful; otherwise, <c>false</c>.</returns>
    public static Boolean TryParse(String candidate, out Serial serial)
    {
        var normalized = SerialRgx().Replace(candidate ?? String.Empty, String.Empty);
        if (UInt32.TryParse(normalized, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
        {
            serial = From(value);
            return true;
        }

        serial = default;
        return false;
    }

    /// <summary>
    /// Parses a string representation of a serial number and returns a new <see cref="Serial"/> instance. The input string can contain non-numeric characters, which will be ignored during parsing.
    /// </summary>
    /// <param name="candidate">The string representation of the serial number.</param>
    /// <returns>A new <see cref="Serial"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input string is null or whitespace.</exception>
    public static Serial Parse(String candidate)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(candidate);

        if (TryParse(candidate, out var serialt))
        {
            return serialt;
        }

        throw new ArgumentException($"Invalid serial number: {candidate}", nameof(candidate));
    }

    /// <summary>
    /// Creates a new <see cref="Serial"/> instance for the specified <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="utc">The date and time for which to create the serial number.</param>
    /// <returns>A new <see cref="Serial"/> instance.</returns>
    public static Serial From(DateTimeOffset utc)
        => new(utc.Year, utc.Month, utc.Day, 0);

    /// <summary>
    /// Creates a new <see cref="Serial"/> instance from the specified unsigned integer.
    /// </summary>
    /// <param name="serial">The unsigned integer representing the serial number.</param>
    /// <returns>A new <see cref="Serial"/> instance.</returns>
    public static Serial From(UInt32 serial)
    {
        var year = Convert.ToInt32(serial / 1000000);
        var month = Convert.ToInt32((serial - (year * 1000000)) / 10000);
        var day = Convert.ToInt32((serial - (year * 1000000) - (month * 10000)) / 100);
        var seq = Convert.ToInt32(serial - (year * 1000000) - (month * 10000) - (day * 100));

        return new Serial(year, month, day, seq);
    }

    /// <summary>
    /// Defines an implicit conversion from a <see cref="Serial"/> to a <see cref="String"/>. This allows you to assign a Serial directly to a String variable, and it will automatically convert the Serial instance to its string representation.
    /// </summary>
    /// <param name="serial">The Serial instance to convert.</param>
    /// <returns>A string representation of the serial number.</returns>
    public static implicit operator String(Serial serial) => serial.ToString();

    /// <summary>
    /// Defines an implicit conversion from a <see cref="UInt32"/> to a <see cref="Serial"/>. This allows you to assign a UInt32 directly to a Serial variable, and it will automatically create a new Serial instance.
    /// </summary>
    /// <param name="serial">The UInt32 value to convert.</param>
    public static implicit operator Serial(UInt32 serial) => From(serial);

    /// <summary>
    /// Defines an implicit conversion from a <see cref="Serial"/> to a <see cref="UInt32"/>. This allows you to assign a Serial directly to a UInt32 variable, and it will automatically convert the Serial instance to a UInt32.
    /// </summary>
    /// <param name="serial">The Serial instance to convert.</param>
    /// <returns>A UInt32 representation of the serial number.</returns>
    public static implicit operator UInt32(Serial serial) => serial.ToUInt32();

    /// <inheritdoc />
    public static Boolean operator ==(Serial a, Serial b) => a._year == b._year && a._month == b._month && a._day == b._day && a._sequence == b._sequence;
    /// <inheritdoc />
    public static Boolean operator !=(Serial a, Serial b) => !(a == b);
    /// <inheritdoc />
    public static Boolean operator <=(Serial a, Serial b) => a.ToUInt32() <= b.ToUInt32();
    /// <inheritdoc />
    public static Boolean operator >=(Serial a, Serial b) => a.ToUInt32() >= b.ToUInt32();
    /// <inheritdoc />
    public static Boolean operator <(Serial a, Serial b) => a.ToUInt32() < b.ToUInt32();
    /// <inheritdoc />
    public static Boolean operator >(Serial a, Serial b) => a.ToUInt32() > b.ToUInt32();

    [GeneratedRegex("[^0-9]")]
    private static partial Regex SerialRgx();
}
