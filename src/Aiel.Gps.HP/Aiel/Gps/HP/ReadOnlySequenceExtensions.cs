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

using System.Buffers.Text;
using System.Runtime.CompilerServices;

namespace Aiel.Gps.HP;

public static class ReadOnlySequenceExtensions
{
    public static Boolean TryParse(this ReadOnlySpan<Byte> slice, out Int32 value)
    {
        Span<Byte> temp = stackalloc Byte[(Int32)slice.Length];
        slice.CopyTo(temp);
        if (Utf8Parser.TryParse(temp, out value, out _))
        {
            return true;
        }

        value = default;
        return false;
    }

    public static Boolean TryParse(this ReadOnlySpan<Byte> slice, out Double value)
    {
        Span<Byte> temp = stackalloc Byte[(Int32)slice.Length];
        slice.CopyTo(temp);
        if (Utf8Parser.TryParse(temp, out value, out _))
        {
            return true;
        }

        value = default;
        return false;
    }

    public static Boolean TryParseHex(this ReadOnlySpan<Byte> slice, out Int32 value)
    {
        value = default;
        for (var i = 0; i < slice.Length; i++)
        {
            var b = slice[i..];

            value += HexToInt(b[0]) << ((slice.Length - 1 - i) * 4);
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int32 HexToInt(Byte ch)
    {
        if (ch is < 48 or (> 57 and < 65) or > 70)
        {
            throw new ArgumentOutOfRangeException(nameof(ch));
        }

        return (ch < 58) ? ch - 48 : ch - 55;
    }
}

