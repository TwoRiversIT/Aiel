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

using System.Text.Json.Serialization;

namespace Aiel.Results;

/// <summary>
/// Represents an error with an error code and error message.
/// </summary>
[JsonConverter(typeof(ErrorJsonConverter))]
public abstract class Error
{
    /// <summary>
    /// Gets the code identifying the error.
    /// </summary>
    public ErrorCode ErrorCode { get; }

    /// <summary>
    /// Gets the human-readable description of the error.
    /// </summary>
    public String Message { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="errorCode">A code identifying the error. Must not be null.</param>
    /// <param name="message">A human-readable description of the error. Must not be null, empty, or whitespace.</param>
    protected Error(ErrorCode errorCode, String message)
    {
        if (String.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException($"'{nameof(message)}' cannot be null or whitespace.", nameof(message));
        }

        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        Message = message;
    }

    /// <summary>
    /// Determines whether this error is of a specific error type.
    /// </summary>
    /// <typeparam name="TError">The error type to check for.</typeparam>
    /// <returns>True if this error is of the specified type; otherwise, false.</returns>
    public Boolean IsErrorType<TError>() where TError : Error
        => this is TError;
}
