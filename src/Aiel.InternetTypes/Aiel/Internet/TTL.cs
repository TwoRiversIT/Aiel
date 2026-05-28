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

namespace Aiel.Internet;

public readonly struct TTL : IEquatable<TTL>, IComparable<TTL>, IComparable
{
    private readonly Int32 _ttl;

    public TTL(Int32 value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        _ttl = value;
    }

    public static TTL Parse(String v)
    {
        if (Int32.TryParse(v, out var result))
        {
            return result;
        }

        throw new ArgumentException($"Don't know how to convert \"{v}\" into a TTL.");
    }

    public static implicit operator Int32(TTL ttl) => ttl._ttl;
    public Int32 ToInt32() => _ttl;

    public static implicit operator TTL(Int32 n) => new(n);
    public static TTL FromInt32(Int32 n) => new(n);

    public static Boolean operator <=(TTL a, TTL b) => a._ttl <= b._ttl;
    public static Boolean operator >=(TTL a, TTL b) => a._ttl >= b._ttl;
    public static Boolean operator <(TTL a, TTL b) => a._ttl < b._ttl;
    public static Boolean operator >(TTL a, TTL b) => a._ttl > b._ttl;

    public static Boolean operator !=(TTL a, TTL b) => !(a == b);

    public static Boolean operator ==(TTL a, TTL b) => a._ttl == b._ttl;

    public static implicit operator String(TTL ttl) => ttl._ttl.ToString(CultureInfo.InvariantCulture);

    public Int32 CompareTo(TTL other) => _ttl.CompareTo(other._ttl);

    public Int32 CompareTo(Object? obj) => (obj is TTL ttl) ? CompareTo(ttl) : 1;

    public Boolean Equals(TTL other) => _ttl == other._ttl;

    public override Boolean Equals(Object? obj) => obj is TTL ttl && Equals(ttl);

    public override Int32 GetHashCode() => _ttl.GetHashCode();

    public override String ToString() => _ttl.ToString(CultureInfo.InvariantCulture);

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
}
