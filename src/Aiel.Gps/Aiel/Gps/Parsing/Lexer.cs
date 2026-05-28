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

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Aiel.Gps.Parsing;

public partial class Lexer : ILexer
{
    private static readonly ReadOnlySequence<Byte> Empty = ReadOnlySequence<Byte>.Empty;
    private const Byte Asterisk = (Byte)'*';

    private readonly ReadOnlySequence<Byte> _sequence;
    private Int64 _cursor;

    // Don't use Auto Property for these... don't want the Property overhead.
    private Byte _currentByte;
    private Boolean _eol = false;

#if DEBUG
    private Char _currentChar;
    private readonly String _sentence;
    private String _sentencePointer = String.Empty;
#endif

    public Lexer(ReadOnlySequence<Byte> sequence)
    {
        _sequence = sequence;
#if DEBUG
        _sentence = sequence.ToString(Encoding.UTF8);
#endif
        Start();
    }

    public Exception Error(String? expected = null)
    {
        var message = $"Parse error at position {_cursor + 1}: unexpected '{(Char)_currentByte}' (0x{_currentByte:X2})";

        if (expected != null)
        {
            message += $", expected {expected}";
        }

#if DEBUG
        message += $"\nSentence: {_sentence}\n{_sentencePointer}";
#endif

        return new InvalidOperationException(message);
    }

    internal Exception ZeroLength(String fieldType = "token")
    {
        var message = $"Parse error at position {_cursor + 1}: {fieldType} with zero length";

#if DEBUG
        message += $"\nSentence: {_sentence}\n{_sentencePointer}";
#endif

        return new InvalidOperationException(message);
    }

    private void Advance()
    {
        _cursor++;
        _currentByte = ByteAt(_cursor);
        if (_currentByte == Asterisk)
        {
            _eol = true;
        }
#if DEBUG
        _currentChar = (Char)_currentByte;
        _sentencePointer = new String(' ', (Int32)_cursor) + "^";
#endif
    }

    private void Advance(Func<Boolean> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        while (predicate())
        {
            Advance();
        }
    }

    private ReadOnlySequence<Byte> ReadWhile(Func<Byte, Boolean> predicate)
    {
        var start = _cursor;

        Advance(() => _currentByte > 0 && predicate(_currentByte));

        if (_cursor == start)
        {
            throw ZeroLength();
        }

        return _sequence.Slice(
            _sequence.GetPosition(start),
            _cursor - start);
    }

    private Byte ByteAt(Int64 index)
    {
        if (index > _sequence.Length - 1)
        {
            return 0;
        }

        var slice = _sequence.Slice(index);
        var candidate = slice.Slice(0, 1);

        return candidate.First.Span[0];
    }

    public Boolean EOL => _eol;

    public Byte Current => _currentByte;

    public Byte Peek()
    {
        var peekPos = _cursor + 1;
        return ByteAt(peekPos);
    }

    public void Start()
    {
        _cursor = 0L;
        _currentByte = ByteAt(_cursor);
        ConsumeSeparator();
    }

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

    public String NextString()
    {
        ConsumeWhiteSpace();

        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return String.Empty;
        }

        var start = _cursor;
        Advance(() => _currentByte > 0 && !IsSeparator(_currentByte));

        var slice = _sequence.Slice(_sequence.GetPosition(start), _cursor - start);
        if (slice.Length == 0)
        {
            // This should never happen
            throw ZeroLength();
        }

        ConsumeSeparator();
        return slice.ToString(Encoding.UTF8);
    }

    public ReadOnlySequence<Byte> NextStringSlice()
    {
        ConsumeWhiteSpace();

        if (IsSeparator(Current))
        {
            ConsumeSeparator();
            return ReadOnlySequence<Byte>.Empty;
        }

        var start = _cursor;
        Advance(() => Current > 0 && !IsSeparator(Current));

        var slice = _sequence.Slice(_sequence.GetPosition(start), _cursor - start);

        ConsumeSeparator();

        return slice;
    }

    public void SkipString()
    {
        ConsumeWhiteSpace();

        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return;
        }

        Advance(() => _currentByte > 0 && !IsSeparator(_currentByte));
        ConsumeSeparator();
    }

    public Double NextDouble()
    {
        ConsumeWhiteSpace();

        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return Double.NaN;
        }

        var start = _cursor;
        Advance(() => _currentByte > 0 && IsNumber(_currentByte));

        var slice = _sequence.Slice(_sequence.GetPosition(start), _cursor - start);
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

    public Int32 NextInteger()
    {
        ConsumeWhiteSpace();
        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return 0;
        }

        var start = _cursor;
        Advance(() => _currentByte > 0 && IsNumber(_currentByte));

        var slice = _sequence.Slice(_sequence.GetPosition(start), _cursor - start);
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

    public Int32 NextHexadecimal()
    {
        ConsumeWhiteSpace();
        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return 0;
        }

        var start = _cursor;
        Advance(() => _currentByte > 0 && (IsDigit(_currentByte) || IsLetter(_currentByte)));

        var slice = _sequence.Slice(_sequence.GetPosition(start), _cursor - start);
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

        var start = _cursor;
        Advance(() => _currentByte > 0 && IsNumber(_currentByte));

        var slice = _sequence.Slice(_sequence.GetPosition(start), _cursor - start);
        if (slice.Length < 3)
        {
            return Double.NaN;
        }

        if (slice.Slice(0, 2).TryParse(out Int32 i))
        {
            if (slice.Slice(2).TryParse(out Double d))
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

        var start = _cursor;
        Advance(() => _currentByte > 0 && IsNumber(_currentByte));

        var slice = _sequence.Slice(_sequence.GetPosition(start), _cursor - start);
        if (slice.Length < 4)
        {
            return Double.NaN;
        }

        if (slice.Slice(0, 3).TryParse(out Int32 i))
        {
            if (slice.Slice(3).TryParse(out Double d))
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

    public DateOnly NextDate()
    {
        ConsumeWhiteSpace();
        if (IsSeparator(_currentByte))
        {
            ConsumeSeparator();
            return DateOnly.MinValue;
        }

        void ConsumeSeparatorIfPresent()
        {
            if (IsSeparator(_currentByte))
            {
                ConsumeSeparator();
            }
        }

        // Expect either DDMMYY or YYYYMMDD, with optional separators between. So we need to
        // consume until we hit a separator, and then branch based on the length of what we
        // consumed.
        // 6: DDMMYY
        // 8: DD/MM/YY OR YYYYMMDD
        // 10: YYYY/MM/DD
        // Anything else is an error

        var start = _cursor;
        Advance(() => _currentByte > 0 && (IsDigit(_currentByte) || _currentByte == '/'));

        var slice = _sequence.Slice(_sequence.GetPosition(start), _cursor - start);
        if (slice.Length == 0)
        {
            // This should never happen
            throw ZeroLength();
        }

        Span<Byte> buffer = stackalloc Byte[(Int32)slice.Length];
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

            ConsumeSeparatorIfPresent();

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
        var start = _cursor;
        Advance(() => _currentByte > 0 && (IsDigit(_currentByte) || IsTimeSeparator(_currentByte)));
        var slice = _sequence.Slice(_sequence.GetPosition(start), _cursor - start);
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

    public DateTime NextDateTime()
    {
        var date = NextDate();
        ConsumeWhiteSpace();
        var time = NextTime();

        return date.ToDateTime(time);
    }

    public Int32 NextChecksum()
    {
        ConsumeToChecksum();
        return NextHexadecimal();
    }

    private TimeOnly ParseTime(ref ReadOnlySequence<Byte> slice, Int32 minutesOffset, Int32 secondsOffset, Int32? millisecondsOffset = null)
    {
        var hour = ParseTwoDigits(slice.Slice(0, 2));
        var minutes = ParseTwoDigits(slice.Slice(minutesOffset, 2));
        var seconds = ParseTwoDigits(slice.Slice(secondsOffset, 2));

        var time = new TimeOnly(hour, minutes, seconds);
        if (millisecondsOffset.HasValue)
        {
            time = time.Add(ParseMilliseconds(slice.Slice(millisecondsOffset.Value)));
        }

        return time;
    }

    private TimeSpan ParseMilliseconds(ReadOnlySequence<Byte> slice)
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

    private Int32 ParseDigits(ReadOnlySequence<Byte> span)
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

    private static Int32 ParseTwoDigits(ReadOnlySequence<Byte> span)
    {
        if (span.IsSingleSegment)
        {
            return ((span.First.Span[0] - '0') * 10) + (span.First.Span[1] - '0');
        }

        Span<Byte> buffer = stackalloc Byte[2];
        span.CopyTo(buffer);
        return ((buffer[0] - '0') * 10) + (buffer[1] - '0');
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
    private static Boolean IsDate(Byte b) => IsNumber(b) || IsWhiteSpace(b) || b == '/' || b == ':';
    /// <summary>
    /// : or .
    /// </summary>
    private static Boolean IsTimeSeparator(Byte b) => b == ':' || b == '.';

    private static ReadOnlySequence<Byte> Colon => new([(Byte)':']);

#pragma warning restore IDE0078 // Use pattern matching

    private void ConsumeWhiteSpace() => Advance(() => _currentByte != 0 && IsWhiteSpace(_currentByte));

    private void ConsumeSeparator()
    {
        // Consume up to the separator
        Advance(() => !IsSeparator(_currentByte));
        // Consume the separator
        Advance();
    }

    private void ConsumeToChecksum()
    {
        Start();
        Advance(() => _currentByte is not 0 and not (Byte)'*');
        // Consume the separator
        Advance();
    }
}
