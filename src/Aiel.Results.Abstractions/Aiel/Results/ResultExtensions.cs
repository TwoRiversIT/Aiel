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

namespace Aiel.Results;

/// <summary>
/// Provides extension methods for working with Result and Maybe types.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Attempts to retrieve the value from a <see cref="Result{T}"/> where T is <see cref="Maybe{T}"/>. If the Result is successful and contains a value, it returns true and outputs the value; otherwise, it returns false and outputs the default value of T.
    /// </summary>
    /// <typeparam name="T">The type of the value that may be present.</typeparam>
    /// <param name="result">The  instance to retrieve the value from.</param>
    /// <param name="value">When this method returns, contains the value if the Result is successful and contains a value; otherwise, the default value of T.</param>
    /// <returns><see langword="true"/> if the Result is successful and contains a value; otherwise, <see langword="false"/>.</returns>
    public static Boolean MaybeGetValue<T>(this Result<Maybe<T>> result, out T? value)
        where T : notnull
    {
        value = result.TryGetValue(out var maybe) && maybe.TryGetValue(out var innerValue)
              ? innerValue
              : default;

        return maybe.HasValue;
    }

    /// <summary>
    /// Attempts to retrieve the value from a <see cref="Result"/> instance if it is of type <see cref="Result{TValue}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="result">The result instance to retrieve the value from.</param>
    /// <param name="value">When this method returns, contains the value from the result if it is of type <see cref="Result{TValue}"/>; otherwise, the default value for the type.</param>
    /// <returns><see langword="true"/> if the value was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public static Boolean TryGetValue<T>(this Result result, out T value)
        where T : notnull
    {
        if (result is Result<T> tResult)
        {
            return tResult.TryGetValue(out value);

        }

        value = default!;

        return false;
    }
}
