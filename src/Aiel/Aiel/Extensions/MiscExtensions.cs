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

namespace Aiel.Extensions;

/// <summary>
/// Miscellaneous extension methods.
/// </summary>
public static class MiscExtensions
{
    /// <summary>
    /// Visits the exception and all its inner exceptions, executing an action on each.
    /// </summary>
    /// <param name="ex">The exception to visit.</param>
    /// <param name="action">An action to execute on each exception in the hierarchy.</param>
    /// <remarks>
    /// This method traverses the <see cref="Exception.InnerException"/> chain and executes
    /// the specified action on the original exception and all inner exceptions.
    /// </remarks>
    public static void Visit(this Exception ex, Action<Exception> action)
    {
        action(ex);
        var iex = ex.InnerException;
        while (iex != null)
        {
            action(iex);
            iex = iex.InnerException;
        }
    }

    /// <summary>
    /// Clamps a value between a minimum and maximum value.
    /// </summary>
    /// <typeparam name="T">The type of value to clamp. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="val">The value to clamp.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The original value if it is within the range, otherwise the minimum or maximum value.</returns>
    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0)
        {
            return min;
        }
        else if (val.CompareTo(max) > 0)
        {
            return max;
        }
        else
        {
            return val;
        }
    }
}
