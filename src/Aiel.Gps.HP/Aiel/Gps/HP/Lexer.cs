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
using System.Text;

namespace Aiel.Gps.HP;

// ToDo: Add ReadWhile(Func<>) and ReadUntil(Func<>) to improve readability and mental model of the parsing code. This would allow us to replace a lot of the while loops with more descriptive calls like ReadWhile(IsDigit) or ReadUntil(IsSeparator).
// ToDo: Add or overalod IsDigit, IsLetter, etc. to automatically use _currentByte and also take advantage of Span<char> to avoid the byte-to-char conversions in those methods.
//       This would also allow us to use built-in char methods like char.IsDigit, char.IsLetter, etc., which are well-optimized and handle a wide range of Unicode characters correctly.
//       We would just need to ensure that we are correctly interpreting the bytes as UTF-8 characters when doing these checks.

public ref struct Lexer
{
    // ToDo: Is this the right way to represent these constants? Is there a Span<> alternative that would be better?
    private const Byte Asterisk = (Byte)'*';
    //private const Byte Period = (Byte)'.';
    //private const Byte Colon = (Byte)':';
    private const Byte Slash = (Byte)'/';
    private const Byte Comma = (Byte)',';
    private const Byte Dollar = (Byte)'$';

    private readonly ReadOnlySpan<Byte> _span;
    private Int32 _pos;

    // Don't use Auto Property for these... don't want the Property overhead.
    private Byte _currentByte;
    private Boolean _eol = false;

    public Lexer(ReadOnlySpan<Byte> sequence)
    {
        _span = sequence;
        _pos = 0;
        Start();
    }

    /// <summary>
    /// Gets a value indicating whether the end of the line (checksum marker) has been reached.
    /// </summary>
    public readonly Boolean EOL => _eol;

    /// <summary>
    /// Initializes the lexer to the start of the sentence and consumes the initial separator.
    /// </summary>
    public void Start()
    {
        _pos = 0;
        _currentByte = ByteAt(_pos);
        ConsumeSeparator();
    }

    /// <summary>
    /// Consumes a string field without parsing it.
    /// </summary>
    public void ConsumeString()
    {
        var span = _span;
        var i = _pos;

        while (i < span.Length && span[i] != Comma)
        {
            i++;
        }

        // Move past the comma
        _pos = i + 1;
        _currentByte = ByteAt(_pos);
    }

    /// <summary>
    /// Creates an exception representing a parse error at the current position.
    /// </summary>
    /// <param name="expected">Optional description of what was expected.</param>
    /// <returns>An exception describing the parse error.</returns>
    public readonly Exception Error(String? expected = null)
    {
        var message = $"Parse error at position {_pos + 1}: unexpected '{(Char)_currentByte}' (0x{_currentByte:X2})";

        if (expected != null)
        {
            message += $", expected {expected}";
        }

        return new InvalidOperationException(message);
    }
    internal readonly Exception ZeroLength(String fieldType = "token")
    {
        var message = $"Parse error at position {_pos + 1}: {fieldType} with zero length";

        return new InvalidOperationException(message);
    }

    private void Advance()
    {
        _pos++;
        _currentByte = ByteAt(_pos);
        if (_currentByte == Asterisk)
        {
            _eol = true;
        }
    }

    private readonly Byte ByteAt(Int32 index)
    {
        if (index < 0 || index >= _span.Length)
        {
            return 0;
        }

        return _span[index];
    }

    public readonly Byte Peek()
    {
        var peekPos = _pos + 1;
        return ByteAt(peekPos);
    }

    /// <summary>
    /// Peeks at the sentence identifier without advancing the lexer position.
    /// </summary>
    /// <returns>The sentence identifier as a byte span (e.g., "GPGLL" from "$GPGLL,...").</returns>
    /// <remarks>
    /// The identifier is the text between the '$' and the first comma.
    /// This method does not advance the lexer position.
    /// </remarks>
    public readonly ReadOnlySpan<Byte> PeekIdentifier()
    {
        // Find the start: skip past '$' if present
        var start = 0;
        if (_span.Length > 0 && _span[0] == Dollar)
        {
            start = 1;
        }

        // Find the end: first comma after start
        var end = start;
        while (end < _span.Length && _span[end] != Comma)
        {
            end++;
        }

        return _span[start..end];
    }

    /// <summary>
    /// Parses and returns the next character field from the sentence.
    /// </summary>
    /// <returns>The parsed character, or Char.MinValue if the field is empty.</returns>
    public Char NextChar()
    {
        ConsumeWhiteSpace();
        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return Char.MinValue;
        }

        var c = (Char)_currentByte;
        ConsumeSeparator();
        return c;
    }

    /// <summary>
    /// Parses and returns the next string field from the sentence.
    /// </summary>
    /// <returns>The parsed string, or String.Empty if the field is empty.</returns>
    public String NextString()
    {
        ConsumeWhiteSpace();

        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return String.Empty;
        }

        var start = _pos;
        while (_currentByte > 0 && !IsSeparator(_currentByte))
        {
            Advance();
        }

        var slice = _span[start.._pos];
        if (slice.Length == 0)
        {
            // This should never happen
            throw ZeroLength();
        }

        ConsumeSeparator();
        return Encoding.UTF8.GetString(slice);
    }

    [Obsolete("Use NextString(). This will be removed shortly.")]
    public void SkipString()
    {
        ConsumeWhiteSpace();

        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return;
        }

        while (_currentByte > 0 && !IsSeparator(_currentByte))
        {
            Advance();
        }

        ConsumeSeparator();
    }

    /// <summary>
    /// Parses and returns the next double-precision floating-point field from the sentence.
    /// </summary>
    /// <returns>The parsed double value, or Double.NaN if the field is empty.</returns>
    public Double NextDouble()
    {
        ConsumeWhiteSpace();

        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return Double.NaN;
        }

        var start = _pos;
        while (_currentByte > 0 && IsNumber(_currentByte))
        {
            Advance();
        }

        var slice = _span[start.._pos];
        if (slice.Length == 0)
        {
            // This should never happen
            throw ZeroLength();
        }

        if (slice.TryParse(out Double value))
        {
            ConsumeSeparator();
            return value;
        }

        throw Error();
    }

    /// <summary>
    /// Parses and returns the next integer field from the sentence.
    /// </summary>
    /// <returns>The parsed integer value, or 0 if the field is empty.</returns>
    public Int32 NextInteger()
    {
        ConsumeWhiteSpace();
        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return 0;
        }

        var start = _pos;
        while (_currentByte > 0 && IsNumber(_currentByte))
        {
            Advance();
        }

        var slice = _span[start.._pos];
        if (slice.Length == 0)
        {
            // This should never happen
            throw ZeroLength();
        }

        if (slice.TryParse(out Int32 value))
        {
            ConsumeSeparator();
            return value;
        }

        throw Error();
    }

    /// <summary>
    /// Parses and returns the next hexadecimal integer field from the sentence.
    /// </summary>
    /// <returns>The parsed hexadecimal value as an integer, or 0 if the field is empty.</returns>
    public Int32 NextHexadecimal()
    {
        ConsumeWhiteSpace();
        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return 0;
        }

        var start = _pos;
        while (_currentByte > 0 && (IsDigit(_currentByte) || IsLetter(_currentByte)))
        {
            Advance();
        }

        var slice = _span[start.._pos];
        if (slice.Length == 0)
        {
            // This should never happen
            throw ZeroLength();
        }

        if (slice.TryParseHex(out var value))
        {
            ConsumeSeparator();
            return value;
        }

        throw Error();
    }

    /// <summary>
    /// Parses and returns the next latitude field from the sentence.
    /// </summary>
    /// <returns>The parsed latitude in decimal degrees. Positive values are North, negative values are South. Returns Double.NaN if the field is empty.</returns>
    public Double NextLatitude()
    {
        ConsumeWhiteSpace();
        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();

            // Next up should be a Direction
            if (IsLetter(Peek()))
            {
                // Consume
                Advance();
            }

            // And then the next separator...
            if (IsSeparator(_currentByte))
            {
                ConsumeSeparator();
            }
            else
            {
                throw Error();
            }

            return Double.NaN;
        }

        var start = _pos;
        while (_currentByte > 0 && IsNumber(_currentByte))
        {
            Advance();
        }

        var slice = _span[start.._pos];
        if (slice.Length < 3)
        {
            return Double.NaN;
        }

        if (slice[..2].TryParse(out Int32 i))
        {
            if (slice[2..].TryParse(out Double d))
            {
                ConsumeSeparator();
                var c = NextChar();  // Also consumes next separator
                if (c == 'N')
                {
                    return i + (d / 60);
                }
                else if (c == 'S')
                {
                    return (i + (d / 60)) * -1;
                }
            }
        }

        throw Error();
    }

    /// <summary>
    /// Parses and returns the next longitude field from the sentence.
    /// </summary>
    /// <returns>The parsed longitude in decimal degrees. Positive values are East, negative values are West. Returns Double.NaN if the field is empty.</returns>
    public Double NextLongitude()
    {
        ConsumeWhiteSpace();
        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            // Next up should be a Direction
            if (IsLetter(Peek()))
            {
                // Consume
                Advance();
            }
            // And then the next separator...
            if (IsSeparator(_currentByte))
            {
                ConsumeSeparator();
            }
            else
            {
                throw Error();
            }

            return Double.NaN;
        }

        var start = _pos;
        while (_currentByte > 0 && IsNumber(_currentByte))
        {
            Advance();
        }

        var slice = _span[start.._pos];
        if (slice.Length < 4)
        {
            return Double.NaN;
        }

        if (slice[..3].TryParse(out Int32 i))
        {
            if (slice[3..].TryParse(out Double d))
            {
                ConsumeSeparator();
                var c = NextChar();  // Also consumes next separator
                if (c == 'E')
                {
                    return i + (d / 60);
                }
                else if (c == 'W')
                {
                    return (i + (d / 60)) * -1;
                }
            }
        }

        throw Error();
    }

    /// <summary>
    /// Parses and returns the next date field from the sentence.
    /// </summary>
    /// <returns>The parsed date, or DateOnly.MinValue if the field is empty.</returns>
    public DateOnly NextDate()
    {
        ConsumeWhiteSpace();
        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return DateOnly.MinValue;
        }

        // Expect either DDMMYY or YYYYMMDD, with optional separators between. So we need to
        // consume until we hit a separator, and then branch based on the length of what we
        // consumed.
        // 6: DDMMYY
        // 8: DD/MM/YY OR YYYYMMDD
        // 10: YYYY/MM/DD
        // Anything else is an error

        var start = _pos;
        while (_currentByte > 0 && (IsDigit(_currentByte) || _currentByte == Slash))
        {
            Advance();
        }

        var slice = _span[start.._pos];
        if (slice.Length == 0)
        {
            // This should never happen
            throw ZeroLength();
        }

        Span<Byte> buffer = stackalloc Byte[slice.Length];
        slice.CopyTo(buffer);
        var span = buffer;

        if (span.Length == 6)
        {
            if (!AreDigits(span, 0, 6))
            {
                throw Error();
            }

            var day = ParseTwoDigits(span[..2]);
            var month = ParseTwoDigits(span.Slice(2, 2));
            var twoDigitYear = ParseTwoDigits(span.Slice(4, 2));

            if (IsSeparator(_currentByte))
            {
                ConsumeSeparator();
            }

            return new DateOnly(CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(twoDigitYear), month, day);
        }

        if (span.Length == 8)
        {
            if (span[2] == '/' && span[5] == '/')
            {
                if (!AreDigits(span, 0, 2) || !AreDigits(span, 3, 2) || !AreDigits(span, 6, 2))
                {
                    throw Error();
                }

                var day = ParseTwoDigits(span[..2]);
                var month = ParseTwoDigits(span.Slice(3, 2));
                var twoDigitYear = ParseTwoDigits(span.Slice(6, 2));

                ConsumeSeparatorIfPresent();

                return new DateOnly(CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(twoDigitYear), month, day);
            }

            if (!AreDigits(span, 0, 8))
            {
                throw Error();
            }

            var year = ParseFourDigits(span[..4]);
            var monthValue = ParseTwoDigits(span.Slice(4, 2));
            var dayValue = ParseTwoDigits(span.Slice(6, 2));

            var result = new DateOnly(year, monthValue, dayValue);
            ConsumeSeparatorIfPresent();
            return result;
        }

        if (span.Length == 10)
        {
            if (span[4] != '/' || span[7] != '/')
            {
                throw Error();
            }

            if (!AreDigits(span, 0, 4) || !AreDigits(span, 5, 2) || !AreDigits(span, 8, 2))
            {
                throw Error();
            }

            var year = ParseFourDigits(span[..4]);
            var monthValue = ParseTwoDigits(span.Slice(5, 2));
            var dayValue = ParseTwoDigits(span.Slice(8, 2));

            var result = new DateOnly(year, monthValue, dayValue);
            ConsumeSeparatorIfPresent();
            return result;
        }

        throw Error();
    }

    /// <summary>
    /// Parses and returns the next time field from the sentence.
    /// </summary>
    /// <returns>The parsed time, or TimeOnly.MinValue if the field is empty.</returns>
    public TimeOnly NextTime()
    {
        ConsumeWhiteSpace();

        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return TimeOnly.MinValue;
        }

        // Read the entire time field, which may be in the format HHMMSS, HH:MM:SS, HHMMSS.sss, or HH:MM:SS.sss, or even HHMMSS.
        // So we need to consume until we hit a separator, and then branch based on the length of what we consumed.
        var start = _pos;
        while (_currentByte > 0 && (IsDigit(_currentByte) || IsTimeSeparator(_currentByte)))
        {
            Advance();
        }

        var slice = _span[start.._pos];
        if (slice.Length == 0)
        {
            // This should never happen
            throw ZeroLength();
        }

        if (slice.Length < 6)
        {
            if (slice.TryParse(out Int32 value))
            {
                ConsumeSeparator();
                var hour = value / 10000;
                var minutes = value / 100 % 100;
                var seconds = value % 100;
                return new TimeOnly(hour, minutes, seconds);
            }

            throw Error();
        }

        var time = slice.Length switch
        {
            // 0 2 4
            // HHMMSS
            6 => ParseTime(ref slice, 2, 4),
            // 01234567
            // HH:MM:SS
            8 => ParseTime(ref slice, 3, 6),
            // 012345678
            // HHMMSS.ss
            9 => ParseTime(ref slice, 2, 4, 7),
            // 0123456789
            // HHMMSS.sss
            10 => ParseTime(ref slice, 2, 4, 7),
            // 0123456789
            // HH:MM:SS.ss
            11 => ParseTime(ref slice, 3, 6, 9),
            // 0123456789
            // HH:MM:SS.sss
            12 => ParseTime(ref slice, 3, 6, 9),
            _ => throw Error()
        };

        ConsumeSeparator();
        return time;
    }

    /// <summary>
    /// Parses and returns the next date and time fields from the sentence.
    /// </summary>
    /// <returns>The parsed date and time combined into a DateTime value.</returns>
    public DateTime NextDateTime()
    {
        var date = NextDate();
        ConsumeWhiteSpace();
        var time = NextTime();

        return date.ToDateTime(time);
    }

    /// <summary>
    /// Advances to the checksum field and parses it as a hexadecimal value.
    /// </summary>
    /// <returns>The parsed checksum value.</returns>
    public Int32 NextChecksum()
    {
        ConsumeToChecksum();
        return NextHexadecimal();
    }

    private readonly TimeOnly ParseTime(ref ReadOnlySpan<Byte> slice, Int32 minutesOffset, Int32 secondsOffset, Int32? millisecondsOffset = null)
    {
        var hour = ParseTwoDigits(slice[..2]);
        var minutes = ParseTwoDigits(slice.Slice(minutesOffset, 2));
        var seconds = ParseTwoDigits(slice.Slice(secondsOffset, 2));

        var time = new TimeOnly(hour, minutes, seconds);
        if (millisecondsOffset.HasValue)
        {
            time = time.Add(ParseMilliseconds(slice[millisecondsOffset.Value..]));
        }

        return time;
    }

    private readonly TimeSpan ParseMilliseconds(ReadOnlySpan<Byte> slice)
    {
        if (slice.Length is < 1 or > 3)
        {
            throw Error();
        }

        var value = ParseDigits(slice);

        var millis = slice.Length switch
        {
            1 => value * 100,
            2 => value * 10,
            3 => value,
            _ => throw Error()
        };

        return new TimeSpan(0, 0, 0, 0, millis);
    }

    private readonly Int32 ParseDigits(ReadOnlySpan<Byte> span)
    {
        Span<Byte> buffer = stackalloc Byte[(Int32)span.Length];
        span.CopyTo(buffer);

        return span.Length switch
        {
            1 => buffer[0] - '0',
            2 => ((buffer[0] - '0') * 10) + (buffer[1] - '0'),
            3 => ((buffer[0] - '0') * 100) + ((buffer[1] - '0') * 10) + (buffer[2] - '0'),
            4 => ((buffer[0] - '0') * 1000) + ((buffer[1] - '0') * 100) + ((buffer[2] - '0') * 10) + (buffer[3] - '0'),
            _ => throw Error()
        };
    }

    [SuppressMessage("Style", "IDE0078:Use pattern matching", Justification = "Readability")]
    private static Boolean AreDigits(ReadOnlySpan<Byte> span, Int32 start, Int32 length)
    {
        for (var i = 0; i < length; i++)
        {
            var value = span[start + i];
            if (value < '0' || value > '9')
            {
                return false;
            }
        }

        return true;
    }

    private static Int32 ParseTwoDigits(ReadOnlySpan<Byte> span)
    {
        return ((span[0] - '0') * 10) + (span[1] - '0');
    }

    private static Int32 ParseFourDigits(ReadOnlySpan<Byte> span)
    {
        return ((span[0] - '0') * 1000) + ((span[1] - '0') * 100) + ((span[2] - '0') * 10) + (span[3] - '0');
    }

#pragma warning disable IDE0078 // Use pattern matching
    private static Boolean IsWhiteSpace(Byte b) => b == ' ' || b == '\t';
    private static Boolean IsSeparator(Byte b) => b == ',' || b == '*' || b == '$' || b == '\r' || b == '\n' || b == 0;
    private static Boolean IsDigit(Byte b) => b >= '0' && b <= '9';
    private static Boolean IsNumber(Byte b) => (b >= '0' && b <= '9') || b == '-' || b == '.';
    private static Boolean IsLetter(Byte b) => (b >= 'A' && b <= 'Z') || (b >= 'a' && b <= 'z');
    //private static Boolean IsDate(Byte b) => IsNumber(b) || IsWhiteSpace(b) || b == '/' || b == ':';
    /// <summary>
    /// : or .
    /// </summary>
    private static Boolean IsTimeSeparator(Byte b) => b == ':' || b == '.';

#pragma warning restore IDE0078 // Use pattern matching

    private void ConsumeWhiteSpace()
    {
        while (_currentByte != 0 && IsWhiteSpace(_currentByte))
        {
            Advance();
        }
    }

    private void ConsumeSeparator()
    {
        // Consume up to the separator
        while (!IsSeparator(_currentByte))
        {
            Advance();
        }
        // Consume the separator
        Advance();
    }

    private void ConsumeSeparatorIfPresent()
    {
        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
        }
    }

    private void ConsumeToChecksum()
    {
        Start();
        while (_currentByte is not 0 and not Asterisk)
        {
            Advance();
        }
        // Consume the asterisk
        Advance();
    }
}

