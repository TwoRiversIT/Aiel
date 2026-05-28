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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Aiel.Extensions;

public static class Strings
{
    public const String Ellipsis = " ...";
}

/// <summary>
/// Extension methods for <see cref="String"/> type.
/// </summary>
[DebuggerNonUserCode]
public static partial class StringExtensions
{
    /// <summary>
    /// Returns the leftmost <paramref name="length"/> characters from the input string.
    /// </summary>
    /// <param name="input">The input string. If null, returns null.</param>
    /// <param name="length">The number of characters to return from the left.</param>
    /// <returns>The leftmost <paramref name="length"/> characters, or the entire string if it is shorter than <paramref name="length"/>.</returns>
    [SuppressMessage("Style", "IDE0057:Convert to conditional expression", Justification = "Readability")]
    public static String Left(this String input, Int32 length)
    {
        if (input is null)
        {
            return input!;
        }
        else if (String.IsNullOrWhiteSpace(input))
        {
            return String.Empty;
        }
        else
        {
            return input.Substring(0, input.Length < length ? input.Length : length);
        }
    }

    /// <summary>
    /// Formats the string using the specified arguments and the invariant culture.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <returns>A copy of format in which the format items have been replaced by the string representation of the corresponding objects.</returns>
    internal static String Format(this String format, params Object[] args)
        => Format(format, CultureInfo.InvariantCulture, args);

    /// <summary>
    /// Formats the string using the specified arguments and format provider.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <returns>A copy of format in which the format items have been replaced by the string representation of the corresponding objects.</returns>
    internal static String Format(this String format, IFormatProvider formatProvider, params Object[] args)
        => String.Format(formatProvider, format, args);

    /// <summary>
    /// Ensures the string is enclosed in double quotes.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The input string enclosed in double quotes if not already quoted.</returns>
    public static String Quoted(this String input)
        => input.StartsWith('"') && input.EndsWith('"') ? input : $"\"{input}\"";

    /// <summary>
    /// Converts a string representation of a GUID to a <see cref="Guid"/>.
    /// </summary>
    /// <param name="input">A string representation of a GUID.</param>
    /// <returns>A <see cref="Guid"/> parsed from the input string.</returns>
    /// <exception cref="ArgumentException">Thrown when the input cannot be parsed as a GUID.</exception>
    public static Guid ToGuid(this String input)
    {
        if (Guid.TryParse(input, out var guid))
        {
            return guid;
        }

        throw new ArgumentException($"Don't know how to parse '{input}' into a GUID.");
    }

    /// <summary>
    /// Truncates the input string to the specified maximum length and appends an ellipsis (...).
    /// </summary>
    /// <param name="input">The string to be truncated.</param>
    /// <param name="maxLength">The maximum length of the returned string, including the ellipsis.</param>
    /// <returns>The truncated string with an ellipsis appended, or an empty string if maxLength is less than or equal to zero.</returns>
    public static String Truncate(this String input, Int32 maxLength)
        => String.IsNullOrEmpty(input) || maxLength <= 0
            ? String.Empty
            : input.Length > maxLength - Strings.Ellipsis.Length
                ? String.Concat(input.AsSpan(0, maxLength - Strings.Ellipsis.Length), Strings.Ellipsis)
                : input;

    /// <summary>
    /// Converts a pascal case or camel case string into proper case (space-separated).
    /// Abbreviations are returned unmodified.
    /// </summary>
    /// <param name="input">The input string in pascal or camel case.</param>
    /// <returns>A proper case version of the input string with spaces between words.</returns>
    /// <example>
    /// <code>
    /// "HelloWorld".ToProperCase() // Returns "Hello World"
    /// "BBC".ToProperCase()        // Returns "BBC"
    /// "IPAddress".ToProperCase()  // Returns "IP Address"
    /// </code>
    /// </example>
    public static String ToProperCase(this String input)
    {
        // If there are 0 or 1 characters, just return the string.
        if (input == null)
        {
            return input!;
        }

        if (input.Length < 2)
        {
            return input.ToUpper();
        }
        //return as is if the input is just an abbreviation
        if (Rgx.AllCaps().IsMatch(input))
        {
            return input;
        }

        // Start with the first character.
        var result = input[..1].ToUpper();

        // Add the remaining characters.
        for (var i = 1; i < input.Length; i++)
        {
            if (Char.IsUpper(input[i]) && i + 1 < input.Length && Char.IsLower(input[i + 1]))
            {
                result += " ";
            }

            result += input[i];
        }

        return result;
    }

    /// <summary>
    /// Truncates <paramref name="input"/> to <paramref name="length"/> and appends <see cref="DefaultEllipsis"/> to the end.
    /// </summary>
    /// <param name="input">value to be truncated</param>
    /// <param name="length">length to return including the ellipsis (...), defaults to 100. 0 returns the original string unmodified.</param>
    /// <param name="firstLineOnly"></param>
    /// <param name="ellipsis"></param>
    public static String Truncate(this String? input, Int32 length = 100, Boolean firstLineOnly = false, String ellipsis = Strings.Ellipsis)
    {
        if (String.IsNullOrWhiteSpace(input))
        {
            return String.Empty;
        }

        input = input.Trim();

        if (firstLineOnly)
        {
            var index = input.IndexOf('\n');
            if (index > 0)
            {
                input = input[..index].Trim();
            }
        }

        if (length == 0)
        {
            return input;
        }

        if (length - ellipsis.Length < 1)
        {
            length = ellipsis.Length + 1;
        }

        if (input.Length <= length - ellipsis.Length)
        {
            return input;
        }

        var truncated = input[..(length - ellipsis.Length)];
        var lastSpaceIndex = truncated.LastIndexOf(' ');

        if (lastSpaceIndex > 0)
        {
            truncated = truncated[..lastSpaceIndex];
        }

        return truncated + ellipsis;
    }

    /// <summary>
    /// Turn a pascal case, camel case, or snake_case string into proper case.
    /// If the input is an abbreviation, the input is returned unmodified.
    /// </summary>
    /// <param name="input"></param>
    /// <example>
    /// input : HelloWorld
    /// output : Hello World
    /// </example>
    /// <example>
    /// input : BBC
    /// output : BBC
    /// </example>
    /// <example>
    /// input : IPAddress
    /// output : IP Address
    /// </example>
    /// <example>
    /// input : cpca_member_number
    /// output : Cpca Member Number
    /// </example>
    [DebuggerStepThrough]
    public static String ToTitleCase(this String input)
    {
        if (input == null)
        {
            return String.Empty;
        }

        // If there are 0 or 1 characters, just return the string.
        if (input.Length < 2)
        {
            return input.ToUpper(CultureInfo.CurrentCulture);
        }

        // Replace underscores with spaces
        var normalized = input.Replace("_", " ");

        // If the input is just an abbreviation then return the original.
        if (AllCapsRegex().IsMatch(normalized.Replace(" ", "")))
        {
            return normalized.Trim();
        }

        // Ensure the first character is capitalized if it's a letter
        if (Char.IsLetter(normalized[0]) && !Char.IsUpper(normalized[0]))
        {
            normalized = Char.ToUpper(normalized[0], CultureInfo.CurrentCulture) + normalized[1..];
        }

        var result = new StringBuilder();
        var capitalizeNext = true;

        // Process each character
        for (var i = 0; i < normalized.Length; i++)
        {
            var current = normalized[i];
            var previous = i > 0 ? normalized[i - 1] : ' ';
            var next = i + 1 < normalized.Length ? normalized[i + 1] : ' ';

            // Handle whitespace - preserve it and mark next letter for capitalization
            if (Char.IsWhiteSpace(current))
            {
                result.Append(current);
                capitalizeNext = true;
                continue;
            }

            // If we should capitalize this character, do it
            if (capitalizeNext && Char.IsLetter(current))
            {
                result.Append(Char.ToUpper(current, CultureInfo.CurrentCulture));
                capitalizeNext = false;
                continue;
            }

            // Insert space before uppercase letter if:
            // - Current is uppercase
            // - Previous is lowercase or digit
            // - We're not already after a space
            if (Char.IsUpper(current) && (Char.IsLower(previous) || Char.IsDigit(previous)))
            {
                result.Append(' ');
                result.Append(current);
                capitalizeNext = false;
                continue;
            }

            // Insert space before uppercase letter followed by lowercase (handles acronyms like "IPAddress" -> "IP Address")
            if (Char.IsUpper(current) && Char.IsUpper(previous) && Char.IsLower(next))
            {
                result.Append(' ');
                result.Append(current);
                capitalizeNext = false;
                continue;
            }

            result.Append(current);
            capitalizeNext = false;
        }

        return result.ToString().Trim();
    }

    public static String ToHoursAndMinutes(this TimeSpan timeSpan)
    {
        var minutes = timeSpan.TotalMinutes % 60;
        var hours = (timeSpan.TotalMinutes - minutes) / 60;
        return $"{hours:00}h {minutes:00}m";
    }

    /// <summary>
    /// Removes any text enclosed by (), <>, or [], including the delimiters.
    /// Handles cases where delimiters are mismatched.
    /// </summary>
    /// <param name="input">The input string that may contain enclosed text.</param>
    /// <returns>A new string with enclosed text removed. If the input is <see langword="null"/> or consists only of
    /// whitespace, an empty string is returned.</returns>
    public static String RemoveEnclosedText(this String? input)
    {
        if (String.IsNullOrWhiteSpace(input))
        {
            return String.Empty;
        }

        // Replace all matches with an empty string.
        return EnclosedText().Replace(input, String.Empty).Trim();
    }

    public static String? ToLfLineEndings(this String self)
        => self == null ? null : NewLineRegExFactory().Replace(self, "\n");

    public static String UnlessNullOrWhiteSpace(this String? input, String unless = "")
    {
        if (String.IsNullOrWhiteSpace(input))
        {
            return unless;
        }

        return input.Trim();
    }

    public static String FormatErrors(this IEnumerable<String>? errors)
    {
        if (errors?.Any() != true)
        {
            return String.Empty;
        }

        var sb = new StringBuilder();

        sb.AppendLine("The following errors occured:");
        sb.AppendLine();
        foreach (var error in errors)
        {
            sb.AppendLine(CultureInfo.CurrentCulture, $"\t- {error}");
        }

        return sb.ToString();
    }

    [GeneratedRegex("^[0-9A-Z]+$", RegexOptions.Compiled)]
    private static partial Regex AllCapsRegex();

    [GeneratedRegex(@"\r?\n")]
    private static partial Regex NewLineRegExFactory();

    /// <summary>
    /// Regular expression to match text enclosed by (), <>, or []. Unfortunately it does not require delimiters to be matching.
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"([\(\[\<])[^\)\]\>]*[\)\]\>]", RegexOptions.Compiled)]
    private static partial Regex EnclosedText();
}
