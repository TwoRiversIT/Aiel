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

/// <summary>
/// Represents a North American Numbering Plan (NANP) phone number.
/// </summary>
public sealed partial class PhoneNumber : IEquatable<PhoneNumber>, IComparable<PhoneNumber>
{
    /// <summary>
    /// Gets an empty <see cref="PhoneNumber"/> instance, representing a phone number with no area code, exchange, subscriber number, or extension.
    /// </summary>
    public static readonly PhoneNumber Empty = new();

    /// <summary>
    /// Gets the area code of the phone number.
    /// </summary>
    public String AreaCode { get; } = String.Empty;

    /// <summary>
    /// Gets the exchange code of the phone number.
    /// </summary>
    public String Exchange { get; } = String.Empty;
    /// <summary>
    /// Gets the subscriber number of the phone number.
    /// </summary>
    public String SubscriberNumber { get; } = String.Empty;
    /// <summary>
    /// Gets the extension of the phone number.
    /// </summary>
    public String Extension { get; } = String.Empty;

    /// <summary>
    /// Gets the hyphenated representation of the phone number.
    /// </summary>
    public String Hyphenated => $"{AreaCode}-{Exchange}-{SubscriberNumber}" + Ext;
    /// <summary>
    /// Gets the digits-only representation of the phone number.
    /// </summary>
    public String Digits => $"{AreaCode}{Exchange}{SubscriberNumber}{Extension}";
    /// <summary>
    /// Gets the E.164 representation of the phone number.
    /// </summary>
    public String E164 => $"+1{Digits}";
    /// <summary>
    /// Gets the national representation of the phone number.
    /// </summary>
    public String National => $"({AreaCode}) {Exchange}-{SubscriberNumber}";
    /// <summary>
    /// Gets the dashed representation of the phone number.
    /// </summary>
    public String Dashes => $"{AreaCode}-{Exchange}-{SubscriberNumber}" + (String.IsNullOrWhiteSpace(Extension) ? String.Empty : $"-{Extension}");
    /// <summary>
    /// Gets the RFC 3966 representation of the phone number.
    /// </summary>
    public String RFC3966 => $"tel:+1-{AreaCode}-{Exchange}-{SubscriberNumber}";

    private String Ext => String.IsNullOrWhiteSpace(Extension) ? String.Empty : $" ext {Extension}";

    /// <summary>
    /// Gets a value indicating whether the phone number is valid.
    /// </summary>
    public Boolean IsValid { get; set; }

    private PhoneNumber()
    {

    }

    private PhoneNumber(String area, String exchange, String subscriber, String extension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(area);
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriber);

        AreaCode = area.Trim();
        Exchange = exchange.Trim();
        SubscriberNumber = subscriber.Trim();
        Extension = extension?.Trim() ?? String.Empty;
    }

    /// <summary>
    /// Attempts to parse the specified input string into a <see cref="PhoneNumber"/> instance.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <param name="number">When this method returns, contains the parsed <see cref="PhoneNumber"/> if the parsing succeeded, or <c>null</c> if the parsing failed.</param>
    /// <returns><c>true</c> if the input string was successfully parsed; otherwise, <c>false</c>.</returns>
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

        var ext = match.Groups.Count > 4 ? match.Groups[4].Value : String.Empty;
        number = new PhoneNumber(
            match.Groups[1].Value,
            match.Groups[2].Value,
            match.Groups[3].Value,
            ext
        );

        return true;
    }

    /// <summary>
    /// Parses the specified input string into a <see cref="PhoneNumber"/> instance.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <returns>The parsed <see cref="PhoneNumber"/> instance.</returns>
    /// <exception cref="FormatException">Thrown if the input string is not a valid phone number.</exception>
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

    /// <summary>
    /// Determines whether the specified <see cref="PhoneNumber"/> is equal to the current <see cref="PhoneNumber"/>.
    /// </summary>
    /// <param name="other">The phone number to compare with the current phone number.</param>
    /// <returns><c>true</c> if the specified phone number is equal to the current phone number; otherwise, <c>false</c>.</returns>
    public Boolean Equals(PhoneNumber? other)
    {
        if (other is null)
        {
            return false;
        }

        return Digits == other.Digits;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="PhoneNumber"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current phone number.</param>
    /// <returns><c>true</c> if the specified object is equal to the current phone number; otherwise, <c>false</c>.</returns>
    public override Boolean Equals(Object? obj)
        => obj is PhoneNumber other && Equals(other);

    /// <inheritdoc/>
    public override Int32 GetHashCode() => Digits.GetHashCode();

    /// <inheritdoc/>
    public static Boolean operator ==(PhoneNumber? left, PhoneNumber? right)
        => Equals(left, right);

    /// <inheritdoc/>
    public static Boolean operator !=(PhoneNumber? left, PhoneNumber? right)
        => !Equals(left, right);

    // -------------------------------
    // Comparison
    // -------------------------------

    /// <summary>
    /// Compares the current <see cref="PhoneNumber"/> with another <see cref="PhoneNumber"/> and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other <see cref="PhoneNumber"/>.
    /// </summary>
    /// <param name="other">The phone number to compare with the current phone number.</param>
    /// <returns>A value that indicates the relative order of the phone numbers being compared.</returns>
    public Int32 CompareTo(PhoneNumber? other)
    {
        if (other is null)
        {
            return 1;
        }

        return String.CompareOrdinal(Digits, other.Digits);
    }

    /// <inheritdoc/>
    public static Boolean operator <(PhoneNumber left, PhoneNumber right)
        => left.CompareTo(right) < 0;

    /// <inheritdoc/>
    public static Boolean operator >(PhoneNumber left, PhoneNumber right)
        => left.CompareTo(right) > 0;

    /// <inheritdoc/>
    public static Boolean operator <=(PhoneNumber left, PhoneNumber right)
        => left.CompareTo(right) <= 0;

    /// <inheritdoc/>
    public static Boolean operator >=(PhoneNumber left, PhoneNumber right)
        => left.CompareTo(right) >= 0;

    // -------------------------------
    // String Conversion
    // -------------------------------

    /// <summary>
    /// Returns a string that represents the current <see cref="PhoneNumber"/> in hyphenated format.
    /// </summary>
    /// <returns>A string that represents the current phone number in hyphenated format.</returns>
    public override String ToString() => Hyphenated;

    /// <summary>
    /// Returns a string that represents the current <see cref="PhoneNumber"/> in the specified format.
    /// </summary>
    /// <param name="format">The format in which to represent the phone number.</param>
    /// <returns>A string that represents the current phone number in the specified format.</returns>
    /// <exception cref="FormatException">Thrown when an unknown format is specified.</exception>
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

    /// <inheritdoc/>
    public static implicit operator String(PhoneNumber number) => number.Hyphenated;

    [GeneratedRegex(@"^(?:\+?1)?\D*([2-9]\d{2})\D*([2-9]\d{2})\D*(\d{4})\D*(\d{3,6})?$", RegexOptions.Compiled)]
    private static partial Regex NanpRegex();
}
