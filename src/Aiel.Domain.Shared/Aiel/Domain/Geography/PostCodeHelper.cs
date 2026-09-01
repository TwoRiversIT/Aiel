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

namespace Aiel.Domain.Geography;

public static partial class PostCodeHelper
{
    private static readonly String[] InvalidCases = ["00000", "11111", "33333", "66666", "77777", "88888", "99999"];

    /// <summary>
    /// Returns <b>true</b> if <i>code</i> is a valid Canadian Postal Code or American Zip Code; otherwise <b>false</b>.
    /// </summary>
    /// <param name="code">the string to validate</param>
    /// <returns><b>true</b> if <i>code</i> is a valid Canadian Postal Code or American Zip Code; otherwise <b>false</b>.</returns>
    public static Boolean IsValidPostCode(String code)
        => !String.IsNullOrWhiteSpace(code) && ((Char.IsDigit(code[0]) && IsValidZipCode(code)) || (Char.IsLetter(code[0]) && IsValidPostalCode(code)));

    /// <summary>
    /// Returns <b>true</b> if <i>code</i> is a valid Canadian Postal Code; otherwise <b>false</b>.
    /// </summary>
    /// <param name="postalCode">the string to validate</param>
    /// <returns><b>true</b> if <i>code</i> is a valid Canadian Postal Code; otherwise <b>false</b>.</returns>
    public static Boolean IsValidPostalCode(this PostalCode postalCode)
    {
        if (postalCode is null)
        {
            return false;
        }

        return PostalCode.IsMatch(postalCode.Code);
    }

    /// <summary>
    /// Returns <b>true</b> if <i>code</i> is a valid Canadian Postal Code; otherwise <b>false</b>.
    /// </summary>
    /// <param name="value">the string to validate</param>
    /// <returns><b>true</b> if <i>code</i> is a valid Canadian Postal Code; otherwise <b>false</b>.</returns>
    public static Boolean IsValidPostalCode(this String value)
        => !String.IsNullOrWhiteSpace(value) && PostalCode.IsMatch(value);

    public static Boolean IsValidPartialPostalCode(String code)
    {
        if (String.IsNullOrEmpty(code) || Char.IsDigit(code[0]))
        {
            return false;
        }

        // Is it a partial Canadian match?
        if (code.Length is < 4 and > 0)
        {
            var test = code + "V1V1V1"[code.Length..];
            if (IsValidPostalCode(test))
            {
                return true;
            }
        }

        return false;
    }

    public static Boolean IsValidFsaCode(String code)
        => !String.IsNullOrWhiteSpace(code) && FsaCode.IsMatch(code);

    /// <summary>
    /// Returns <b>true</b> if <i>code</i> is a valid American Zip Code; otherwise <b>false</b>.
    /// </summary>
    /// <param name="code">the string to validate</param>
    /// <returns><b>true</b> if <i>code</i> is a valid American Zip Code; otherwise <b>false</b>.</returns>
    public static Boolean IsValidZipCode(this String code)
        => !String.IsNullOrWhiteSpace(code) && ZipCode.IsMatch(code) && !IsInvalidCase(code);

    public static Boolean IsValidPartialZipCode(String code)
    {
        if (String.IsNullOrWhiteSpace(code) || !Char.IsDigit(code[0]))
        {
            return false;
        }

        // Is it a partial American match?
        if (code.Length < 4)
        {
            var local = code + "00000"[code.Length..];
            if (IsValidZipCode(local) || local == "00000")
            {
                return true;
            }
        }

        return false;
    }

    private static Boolean IsInvalidCase(String code)
        => InvalidCases.Any(c => c == code);

    public static Boolean IsValidScfCode(String code)
        => !String.IsNullOrWhiteSpace(code) && ScfCode.IsMatch(code);

    public static String FormatCodeSlow(String code)
    {
        if (String.IsNullOrWhiteSpace(code))
        {
            return code;
        }

        if (IsValidPostalCode(code))
        {
            var s = AlphaNumeric.Replace(code.ToUpper(CultureInfo.InvariantCulture), String.Empty);
            return $"{s.AsSpan()[..3]} {s.AsSpan(3, 3)}";
        }

        if (IsValidZipCode(code))
        {
            var s = Numeric.Replace(code.ToUpper(CultureInfo.InvariantCulture), String.Empty);
            return s.Length == 5 ? s : s[..5] + "+" + s[5..];
        }

        return code;
    }

    /// <summary>
    /// Formats Canadian Postal Codes
    /// </summary>
    /// <param name="code"></param>
    /// <returns>String</returns>
    /// <remarks>
    /// <para>Canadian Postal Code Format is: L#L #L#</para>
    /// <para>If the input is not a valid Canadian Postal Code returned unmodified.</para>
    /// </remarks>
    public static String FormatPostalCode(this String code)
    {
        ArgumentNullException.ThrowIfNull(code);

        code = code.Trim().ToUpperInvariant();
        var raw = Normalize.Replace(code, String.Empty);
        return raw.Length == 6
            ? $"{raw.AsSpan(0, 3)} {raw.AsSpan(3, 3)}"
            : raw;
    }

    /// <summary>
    /// Formats Canadian Postal Codes and American Zip Codes
    /// </summary>
    /// <param name="code"></param>
    /// <returns>String</returns>
    /// <remarks>
    /// <para>Canadian Postal Code Format is: L#L #L#</para>
    /// <para>American Zip Code format is: #####</para>
    /// <para>If the input is not a valid Canadian Postal Code or American Zip Code <see cref="String.Empty"/>.</para>
    /// <para>Zip+4 is not supported.</para>
    /// </remarks>
    public static String FormatCode(String? code)
    {
        if (String.IsNullOrWhiteSpace(code))
        {
            return String.Empty;
        }

        var normalized = AlphaNumeric.Replace(code, String.Empty).ToUpper(CultureInfo.InvariantCulture);
        if (ZipCode.IsMatch(normalized))
        {
            return normalized;
        }

        if (normalized.Length == 6 && Char.IsLetter(normalized[0]))
        {
            Int16 index = 0;
            var buffer = new Char[7];
            buffer[0] = Char.ToUpper(normalized[index++], CultureInfo.InvariantCulture);

            if (Char.IsLetter(normalized[index]))
            {
                return code;
            }

            buffer[1] = normalized[index++];

            if (Char.IsDigit(normalized[index]))
            {
                return code;
            }

            buffer[2] = Char.ToUpper(normalized[index++], CultureInfo.InvariantCulture);

            if (!Char.IsLetter(normalized[index]) && !Char.IsDigit(normalized[index]))
            {
                index++;
            }

            buffer[3] = ' ';

            if (Char.IsLetter(normalized[index]))
            {
                return code;
            }

            buffer[4] = normalized[index++];

            if (Char.IsDigit(normalized[index]))
            {
                return code;
            }

            buffer[5] = Char.ToUpper(normalized[index++], CultureInfo.InvariantCulture);

            if (Char.IsLetter(normalized[index]))
            {
                return code;
            }

            buffer[6] = normalized[index];

            var formatted = new String(buffer).Trim();

            if (PostalCode.IsMatch(formatted))
            {
                return formatted;
            }
        }

        return code;
    }

    public static Boolean TryFormatCode(String code, out String? formatted)
    {
        formatted = String.Empty;
        if (String.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        if (IsValidPostalCode(code))
        {
            var s = AlphaNumeric.Replace(code.ToUpper(CultureInfo.InvariantCulture), String.Empty);
            formatted = $"{s.AsSpan()[..3]} {s.AsSpan(3, 3)}";
            return true;
        }

        if (IsValidZipCode(code))
        {
            var s = Numeric.Replace(code.ToUpper(CultureInfo.InvariantCulture), String.Empty);
            formatted = s.Length == 5 ? s : s[..5] + "+" + s[5..];
            return false;
        }

        return false;
    }

    [GeneratedRegex("[^0-9A-Z]+", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex NormalizeRegex();
    private static readonly Regex Normalize = NormalizeRegex();

    [GeneratedRegex(@"[^0-9]", RegexOptions.Compiled)]
    private static partial Regex NumericRegex();
    private static readonly Regex Numeric = NumericRegex();

    [GeneratedRegex(@"[^a-zA-Z0-9]", RegexOptions.Compiled)]
    private static partial Regex AlphaNumericRegex();
    private static readonly Regex AlphaNumeric = AlphaNumericRegex();

    [GeneratedRegex(@"^[0-9]{5}$", RegexOptions.Compiled)]
    private static partial Regex ZipCodeRegex();
    private static readonly Regex ZipCode = ZipCodeRegex();

    //[GeneratedRegex(@"^[0-9]{5}\s*\+[0-9]{4}$", RegexOptions.Compiled)]
    //private static partial Regex ZipPlusFourCodeRegex();
    //private static readonly Regex ZipPlusFourCode = ZipPlusFourCodeRegex();

    [GeneratedRegex(@"^[0-9]{3}$", RegexOptions.Compiled)]
    private static partial Regex ScfCodeRegex();
    private static readonly Regex ScfCode = ScfCodeRegex();

    [GeneratedRegex(@"^[abceghjklmnprstvxy][0-9][abceghjklmnprstvwxyz](\s*|-?)[0-9][abceghjklmnprstvwxyz][0-9]$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled, "en-US")]
    private static partial Regex PostalCodeRegex();
    private static readonly Regex PostalCode = PostalCodeRegex();

    //[GeneratedRegex(@"^([abceghjklmnprstvxy][0-9][abceghjklmnprstvwxyz](\s|-)?[0-9][abceghjklmnprstvwxyz][0-9]|[0-9]{5})$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled, "en-US")]
    //private static partial Regex ZipOrPostalCodeRegex();
    //private static readonly Regex ZipOrPostalCode = ZipOrPostalCodeRegex();

    [GeneratedRegex(@"^[abceghjklmnprstvxy][0-9][abceghjklmnprstvwxyz]$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex FsaCodeRegex();
    private static readonly Regex FsaCode = FsaCodeRegex();
}
