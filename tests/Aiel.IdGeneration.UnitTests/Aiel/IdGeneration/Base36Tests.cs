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

using Aiel.IdGeneration.Internal;

namespace Aiel.IdGeneration;

public class Base36Tests
{
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(10, "A")]
    [InlineData(35, "Z")]
    [InlineData(36, "10")]
    [InlineData(100, "2S")]
    [InlineData(1000, "RS")]
    [InlineData(12345, "9IX")]
    [InlineData(Int64.MaxValue, "1Y2P0IJ32E8E7")]
    public void Encode_ConvertsCorrectly(Int64 input, String expected)
    {
        var result = Base36.Encode(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-1, "-1")]
    [InlineData(-10, "-A")]
    [InlineData(-35, "-Z")]
    [InlineData(-36, "-10")]
    [InlineData(-100, "-2S")]
    [InlineData(-1000, "-RS")]
    [InlineData(Int64.MinValue, "-1Y2P0IJ32E8E8")]
    public void Encode_HandlesNegativeNumbers(Int64 input, String expected)
    {
        var result = Base36.Encode(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("A", 10)]
    [InlineData("Z", 35)]
    [InlineData("10", 36)]
    [InlineData("2S", 100)]
    [InlineData("RS", 1000)]
    [InlineData("9IX", 12345)]
    [InlineData("1Y2P0IJ32E8E7", Int64.MaxValue)]
    public void Decode_ConvertsCorrectly(String input, Int64 expected)
    {
        var result = Base36.Decode(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("-1", -1)]
    [InlineData("-A", -10)]
    [InlineData("-Z", -35)]
    [InlineData("-10", -36)]
    [InlineData("-2S", -100)]
    [InlineData("-RS", -1000)]
    [InlineData("-1Y2P0IJ32E8E8", Int64.MinValue)]
    public void Decode_HandlesNegativeNumbers(String input, Int64 expected)
    {
        var result = Base36.Decode(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("a", 10)]
    [InlineData("z", 35)]
    [InlineData("abc", 13368)]
    public void Decode_IsCaseInsensitive(String input, Int64 expected)
    {
        var result = Base36.Decode(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("  10  ", 36)]
    [InlineData("  ABC  ", 13368)]
    public void Decode_TrimsWhitespace(String input, Int64 expected)
    {
        var result = Base36.Decode(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(12345)]
    [InlineData(-12345)]
    [InlineData(Int64.MaxValue)]
    [InlineData(Int64.MinValue)]
    public void RoundTrip_PreservesValue(Int64 original)
    {
        var encoded = Base36.Encode(original);
        var decoded = Base36.Decode(encoded);

        Assert.Equal(original, decoded);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Decode_ThrowsOnInvalidInput(String? input)
    {
        Assert.ThrowsAny<ArgumentException>(() => Base36.Decode(input!));
    }

    [Theory]
    [InlineData("ABC@")]
    [InlineData("12.34")]
    [InlineData("AB CD")]
    [InlineData("!@#$")]
    public void Decode_ThrowsOnInvalidCharacters(String input)
    {
        Assert.ThrowsAny<ArgumentException>(() => Base36.Decode(input));
    }

    [Theory]
    [InlineData("1Y2P0IJ32E8E8")]
    [InlineData("ZZZZZZZZZZZZZ")]
    public void Decode_ThrowsOnOverflow_TooLarge(String input)
    {
        Assert.ThrowsAny<ArgumentException>(() => Base36.Decode(input));
    }

    [Theory]
    [InlineData("-1Y2P0IJ32E8E9")]
    [InlineData("-ZZZZZZZZZZZZZ")]
    public void Decode_ThrowsOnOverflow_TooSmall(String input)
    {
        Assert.ThrowsAny<ArgumentException>(() => Base36.Decode(input));
    }

    [Theory]
    [InlineData("0", "0", 0)]
    [InlineData("1", "2", 1)]
    [InlineData("2", "1", -1)]
    [InlineData("A", "B", 1)]
    [InlineData("10", "Z", -1)]
    [InlineData("-10", "-1", 1)]
    [InlineData("-1", "-10", -1)]
    public void Compare_ReturnsCorrectComparison(String valueA, String valueB, Int32 expectedSign)
    {
        var result = Base36.Compare(valueA, valueB);

        if (expectedSign < 0)
        {
            Assert.True(result < 0, $"Expected {valueA} comparison to {valueB} to be negative, but got {result}");
        }
        else if (expectedSign > 0)
        {
            Assert.True(result > 0, $"Expected {valueA} comparison to {valueB} to be positive, but got {result}");
        }
        else
        {
            Assert.Equal(0, result);
        }
    }

    [Theory]
    [InlineData("1Y2P0IJ32E8E8", true)]
    [InlineData("ZZZZZZZZZZZZZ", true)]
    [InlineData("-1Y2P0IJ32E8E9", true)]
    [InlineData("1Y2P0IJ32E8E7", false)]
    [InlineData("-1Y2P0IJ32E8E8", false)]
    [InlineData("0", false)]
    public void WouldOverflow_DetectsOverflow(String value, Boolean expectedToOverflow)
    {
        var result = Base36.WouldOverflow(value);

        Assert.Equal(expectedToOverflow, result);
    }
}
