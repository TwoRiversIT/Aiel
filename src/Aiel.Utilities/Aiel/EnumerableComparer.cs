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

namespace Aiel;

/// <summary>
/// Compares two sequences.
/// </summary>
/// <typeparam name="T">Type of item in the sequences.</typeparam>
/// <remarks>
/// Compares elements from the two input sequences in turn. If we
/// run out of list before finding unequal elements, then the shorter
/// list is deemed to be the lesser list.
/// </remarks>
public class EnumerableComparer<T> : IComparer<IEnumerable<T>>
{
    /// <summary>
    /// Create a sequence comparer using the default comparer for T.
    /// </summary>
    public EnumerableComparer()
    {
        _comp = Comparer<T>.Default;
    }

    /// <summary>
    /// Create a sequence comparer, using the specified item comparer
    /// for T.
    /// </summary>
    /// <param name="comparer">Comparer for comparing each pair of
    /// items from the sequences.</param>
    public EnumerableComparer(IComparer<T> comparer)
    {
        _comp = comparer;
    }

    /// <summary>
    /// Object used for comparing each element.
    /// </summary>
    private readonly IComparer<T> _comp;

    /// <summary>
    /// Compare two sequences of T.
    /// </summary>
    /// <param name="x">First sequence.</param>
    /// <param name="y">Second sequence.</param>
    public Int32 Compare(IEnumerable<T>? x, IEnumerable<T>? y)
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

        using (var leftIt = x.GetEnumerator())
        using (var rightIt = y.GetEnumerator())
        {
            while (true)
            {
                var left = leftIt.MoveNext();
                var right = rightIt.MoveNext();

                if (!(left || right))
                {
                    return 0;
                }

                if (!left)
                {
                    return -1;
                }

                if (!right)
                {
                    return 1;
                }

                var itemResult = _comp.Compare(leftIt.Current, rightIt.Current);
                if (itemResult != 0)
                {
                    return itemResult;
                }
            }
        }
    }
}
