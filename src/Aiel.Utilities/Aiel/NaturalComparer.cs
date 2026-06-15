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

namespace Aiel;

/// <summary>
/// Compares objects using natural ordering, where numeric sequences are compared numerically rather than lexicographically.
/// </summary>
/// <typeparam name="T">The type of objects to compare.</typeparam>
/// <remarks>
/// This comparer is useful for sorting alphanumeric strings in a human-friendly way.
/// For example, "file2.txt" comes before "file10.txt" in natural order.
/// </remarks>
public partial class NaturalComparer<T> : IComparer<T>
{
    private readonly EnumerableComparer<Object> _comparer = new();

    /// <summary>
    /// Gets a compiled regular expression for splitting numeric sequences.
    /// </summary>
    /// <returns>A compiled regex for identifying numeric sequences.</returns>
    [GeneratedRegex("([0-9]+)", RegexOptions.Compiled)]
    private partial Regex SplitRegex();

    /// <summary>
    /// Gets a compiled regular expression for replacing multiple whitespace characters.
    /// </summary>
    /// <returns>A compiled regex matching one or more whitespace characters.</returns>
    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private partial Regex ReplaceRegex();

    /// <summary>
    /// Converts a sequence of strings to an array of objects, parsing numeric strings as integers.
    /// </summary>
    /// <param name="str">The sequence of strings to convert.</param>
    /// <returns>An array of objects containing integers or strings.</returns>
    private static Object[] ConvertToObjects(IEnumerable<String> str)
    {
        var list = new List<Object>();
        foreach (var s in str)
        {
            if (String.IsNullOrEmpty(s))
            {
                continue;
            }

            if (Int32.TryParse(s, out var result))
            {
                list.Add(result);
            }
            else
            {
                list.Add(s);
            }
        }

        return list.ToArray();
    }

    /// <summary>
    /// Compares two objects using natural ordering.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>A value indicating the relative order of the objects.</returns>
    public Int32 Compare(T? x, T? y)
    {
        if (x == null && y == null)
        {
            return 0;
        }

        if (x == null)
        {
            return -1;
        }

        if (y == null)
        {
            return 1;
        }

        var xr = ReplaceRegex().Replace(x.ToString()!, "");
        var xs = SplitRegex().Split(xr);
        var xc = NaturalComparer<T>.ConvertToObjects(xs);
        var yr = ReplaceRegex().Replace(y.ToString()!, "");
        var ys = SplitRegex().Split(yr);
        var yc = NaturalComparer<T>.ConvertToObjects(ys);
        return _comparer.Compare(xc, yc);
    }
}
