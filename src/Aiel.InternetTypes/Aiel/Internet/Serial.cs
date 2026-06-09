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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Aiel.InternetTypes;

public readonly partial struct Serial : IEquatable<Serial>, IComparable<Serial>
{
    private readonly Int32 _stableHashCode;

    private readonly Int32 _year;
    private readonly Int32 _month;
    private readonly Int32 _day;
    private readonly Int32 _sequence;

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

    public static Serial NewSerial(DateTimeOffset instant)
    {
        return new Serial(instant.Year, instant.Month, instant.Day, 0);
    }

    public static Serial NewSerial(UInt32 serial)
    {
        var year = Convert.ToInt32(serial / 1000000);
        var month = Convert.ToInt32((serial - (year * 1000000)) / 10000);
        var day = Convert.ToInt32((serial - (year * 1000000) - (month * 10000)) / 100);
        var seq = Convert.ToInt32(serial - (year * 1000000) - (month * 10000) - (day * 100));

        return new Serial(year, month, day, seq);
    }

    private static UInt32 ToUInt32(Int32 year, Int32 month, Int32 day, Int32 sequence) => Convert.ToUInt32((year * 1000000u) + (month * 10000u) + (day * 100u) + sequence);

    [SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Readability")]
    public Serial Increment(DateTimeOffset instant)
    {
        var serial = NewSerial(instant);

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

    public override Int32 GetHashCode() => _stableHashCode;

    public override Boolean Equals(Object? obj) => obj is Serial other && _year == other._year && _month == other._month && _day == other._day && _sequence == other._sequence;
    public Boolean Equals(Serial other) => _year == other._year && _month == other._month && _day == other._day && _sequence == other._sequence;

    public static Boolean operator ==(Serial a, Serial b) => a._year == b._year && a._month == b._month && a._day == b._day && a._sequence == b._sequence;
    public static Boolean operator !=(Serial a, Serial b) => !(a == b);
    public static Boolean operator <=(Serial a, Serial b) => a.ToUInt32() <= b.ToUInt32();
    public static Boolean operator >=(Serial a, Serial b) => a.ToUInt32() >= b.ToUInt32();
    public static Boolean operator <(Serial a, Serial b) => a.ToUInt32() < b.ToUInt32();
    public static Boolean operator >(Serial a, Serial b) => a.ToUInt32() > b.ToUInt32();

    public static implicit operator Serial(UInt32 n) => NewSerial(n);
    public static Serial FromUInt32(UInt32 n) => NewSerial(n);
    public static implicit operator UInt32(Serial serial) => ToUInt32(serial._year, serial._month, serial._day, serial._sequence);
    public UInt32 ToUInt32() => ToUInt32(_year, _month, _day, _sequence);

    public static implicit operator String(Serial serial) => serial.ToString();

    public static Serial Parse(String s)
    {
        var normalized = SerialRgx().Replace(s, String.Empty);
        if (String.IsNullOrWhiteSpace(s))
        {
            throw new ArgumentNullException(nameof(s));
        }

        return NewSerial(UInt32.Parse(normalized, CultureInfo.InvariantCulture));
    }

    public Int32 CompareTo(Serial other) => ToUInt32().CompareTo(other.ToUInt32());

    public String ToFormattedString() => $"{_year}-{_month.ToString("00", CultureInfo.InvariantCulture)}-{_day.ToString("00", CultureInfo.InvariantCulture)}-{_sequence.ToString("00", CultureInfo.InvariantCulture)}";

    public override String ToString() => ToUInt32().ToString(CultureInfo.InvariantCulture);
    [GeneratedRegex("[^0-9]")]
    private static partial Regex SerialRgx();
}
