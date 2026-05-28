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

public static partial class ResultExtensions
{
    /// <summary>
    /// Converts a result to a typed result with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="value">The value for the successful result.</param>
    /// <returns>A successful <see cref="Result{T}"/> with the specified value, or a failed result with the original error.</returns>
    public static Result<T> ToResult<T>(this Result result, T value)
    {
        return result.IsSuccess
            ? Result<T>.Success(value)
            : Result<T>.Failure(result.Error);
    }

    /// <summary>
    /// Gets the value from a successful result or returns the specified default value if the result failed.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to extract the value from.</param>
    /// <param name="defaultValue">The value to return if the result is failed.</param>
    /// <returns>The result value if successful; otherwise, the default value.</returns>
    public static T GetValueOrDefault<T>(this Result<T> result, T defaultValue)
    {
        return result.IsSuccess ? result.Value : defaultValue;
    }

    /// <summary>
    /// Gets the value from a successful result or returns the default value for the type if the result failed.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to extract the value from.</param>
    /// <returns>The result value if successful; otherwise, the default value for type T.</returns>
    public static T? GetValueOrDefault<T>(this Result<T> result)
    {
        return result.IsSuccess ? result.Value : default;
    }
}
