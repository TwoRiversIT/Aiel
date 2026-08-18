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
/// Represents an error with an error code and error description.
/// </summary>
[JsonConverter(typeof(ErrorJsonConverter))]
public abstract class Error
{
    /// <summary>
    /// Gets the code identifying the error.
    /// </summary>
    public ErrorCode ErrorCode { get; }

    /// <summary>
    /// Gets the human-readable description of the error. NOTE: This property is primarily
    /// for logging and debugging purposes. For user-facing messages, consider adding a
    /// property to your custom generated Error that provides a friendly, localized error
    /// description for the end user.
    /// </summary>
    public String ErrorDescription { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="errorCode">A code identifying the error. Must not be null.</param>
    /// <param name="errorDescription">A human-readable description of the error. Must not be null, empty, or whitespace.</param>
    /// <remarks>
    /// The <paramref name="errorDescription" /> parameter is for logging and debugging purposes. For
    /// user-facing messages, consider adding a property to your custom generated Error that
    /// provides a friendly, localized error description for the end user.
    /// </remarks>
    protected Error(ErrorCode errorCode, String errorDescription)
    {
        if (String.IsNullOrWhiteSpace(errorDescription))
        {
            throw new ArgumentException($"'{nameof(errorDescription)}' must not be null or whitespace.", nameof(errorDescription));
        }

        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        ErrorDescription = errorDescription;
    }

    /// <summary>
    /// Determines whether this error is of a specific error type.
    /// </summary>
    /// <typeparam name="TError">The error type to check for.</typeparam>
    /// <returns>True if this error is of the specified type; otherwise, false.</returns>
    public Boolean IsErrorType<TError>() where TError : Error
        => this is TError;
}
