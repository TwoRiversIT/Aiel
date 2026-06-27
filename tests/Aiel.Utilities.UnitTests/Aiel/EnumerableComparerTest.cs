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

namespace Aiel.UnitTests;

public partial class EnumerableComparerTest
{
    private readonly Func<String, Object> _convert = str =>
    {
        try
        {
            return Int32.Parse(str);
        }
        catch
        {
            return str;
        }
    };

    [Fact]
    public void Test()
    {
        var unsorted = new List<String>() { "z24", "z2", "z15", "z1", "New York", "z3", "z20", "z5", "Newark", "z11", "z 21", "z22", "NewYork" };

        var sorted = unsorted.OrderBy(str => Integers().Split(str.Replace(" ", "")).Select(s => _convert(s)), new EnumerableComparer<Object>());

        sorted.Should().ContainInOrder("Newark", "New York", "NewYork", "z1", "z2", "z3", "z5", "z11", "z15", "z20", "z 21", "z22", "z24");
    }

    [GeneratedRegex("([0-9]+)")]
    private static partial Regex Integers();
}
