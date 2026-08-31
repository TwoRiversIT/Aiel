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

using System.Text.RegularExpressions;

namespace Aiel.Domain.Contacts;

public sealed partial class PhoneNumber : IEquatable<PhoneNumber>, IComparable<PhoneNumber>
{
    public static readonly PhoneNumber Empty = new();

    public String AreaCode { get; } = String.Empty;
    public String Exchange { get; } = String.Empty;
    public String SubscriberNumber { get; } = String.Empty;

    public String Hyphenated => $"{AreaCode}-{Exchange}-{SubscriberNumber}";
    public String Digits => $"{AreaCode}{Exchange}{SubscriberNumber}";
    public String E164 => $"+1{Digits}";
    public String National => $"({AreaCode}) {Exchange}-{SubscriberNumber}";
    public String Dashes => $"{AreaCode}-{Exchange}-{SubscriberNumber}";
    public String RFC3966 => $"tel:+1-{AreaCode}-{Exchange}-{SubscriberNumber}";

    public Boolean IsValid { get; set; }

    private PhoneNumber()
    {

    }

    private PhoneNumber(String area, String exchange, String subscriber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(area);
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriber);

        AreaCode = area;
        Exchange = exchange;
        SubscriberNumber = subscriber;
    }

    public static Boolean TryParse(String? input, [NotNullWhen(true)] out PhoneNumber number)
    {
        number = null!; // Initialize to null to satisfy the compiler, will be set if parsing succeeds.
        if (String.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var match = NanpRegex().Match(input);
        if (!match.Success)
        {
            return false;
        }

        number = new PhoneNumber(
            match.Groups[1].Value,
            match.Groups[2].Value,
            match.Groups[3].Value
        );

        return true;
    }

    public static PhoneNumber Parse(String input)
    {
        if (TryParse(input, out var number))
        {
            return number!;
        }

        throw new FormatException($"Invalid NANP phone number: {input}");
    }

    // -------------------------------
    // Equality + Hashing
    // -------------------------------

    public Boolean Equals(PhoneNumber? other)
    {
        if (other is null)
        {
            return false;
        }

        return Digits == other.Digits;
    }

    public override Boolean Equals(Object? obj)
        => obj is PhoneNumber other && Equals(other);

    public override Int32 GetHashCode() => Digits.GetHashCode();

    public static Boolean operator ==(PhoneNumber? left, PhoneNumber? right)
        => Equals(left, right);

    public static Boolean operator !=(PhoneNumber? left, PhoneNumber? right)
        => !Equals(left, right);

    // -------------------------------
    // Comparison
    // -------------------------------

    public Int32 CompareTo(PhoneNumber? other)
    {
        if (other is null)
        {
            return 1;
        }

        return String.CompareOrdinal(Digits, other.Digits);
    }

    public static Boolean operator <(PhoneNumber left, PhoneNumber right)
        => left.CompareTo(right) < 0;

    public static Boolean operator >(PhoneNumber left, PhoneNumber right)
        => left.CompareTo(right) > 0;

    public static Boolean operator <=(PhoneNumber left, PhoneNumber right)
        => left.CompareTo(right) <= 0;

    public static Boolean operator >=(PhoneNumber left, PhoneNumber right)
        => left.CompareTo(right) >= 0;

    // -------------------------------
    // String Conversion
    // -------------------------------

    public override String ToString() => Hyphenated;

    public String ToString(String format)
    {
        return format switch
        {
            "H" => Hyphenated,
            "N" => National,
            "D" => Dashes,
            "E" => E164,
            "R" => RFC3966,
            "G" => Digits,
            _ => throw new FormatException($"Unknown phone number format '{format}'.")
        };
    }

    [GeneratedRegex(@"^(?:\+?1)?\D*([2-9]\d{2})\D*([2-9]\d{2})\D*(\d{4})$", RegexOptions.Compiled)]
    private static partial Regex NanpRegex();
}
