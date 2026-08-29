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
    internal const String NotImplemented = "Not Implemented";

    /// <summary>
    /// Gets the default description for the error. Derived classes can override this property to provide a custom default description.
    /// </summary>
    protected virtual String? DefaultDescription => NotImplemented;

    /// <summary>
    /// Gets the code identifying the error.
    /// </summary>
    public ErrorCode Code { get; }

    /// <summary>
    /// Gets the human-readable description of the error. NOTE: This property is primarily
    /// for logging and debugging purposes. For user-facing messages, consider adding a
    /// property to your custom generated Error that provides a friendly, localized error
    /// description that is appropriate for a non-technical end user.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the description is null or whitespace because errors are required to have a description.</exception>
    public String Description
    {
        get
        {
            if (String.IsNullOrWhiteSpace(field))
            {
                return GenerateDescription()
                    ?? throw new InvalidOperationException("Description must not be null or whitespace.");
            }

            return field;
        }
    }

    private Error()
    {
        Code = NoSourceGeneratedError.NoSourceGeneratedErrorCode.Instance;
        Description = NoSourceGeneratedError.DefaultMessage + GetType().Name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="errorCode">A code identifying the error. Must not be null.</param>
    /// <remarks>
    /// <para>
    /// This constructor must only be used when the derived implementation also implements
    /// <see cref="Error.GenerateDescription"/>.
    /// </para>
    /// </remarks>
    // ToDo: Write and analyzer that will warn if a derived class uses this constructor but does not override GenerateDescription.
    protected Error(ErrorCode errorCode)
    {
        Code = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        Description = String.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="errorCode">A code identifying the error. Must not be null.</param>
    /// <param name="description">A human-readable description of the error. Must not be null, empty, or whitespace.</param>
    /// <remarks>
    /// The <paramref name="description" /> parameter is for logging and debugging purposes. For
    /// user-facing messages, consider adding a property to your custom generated Error that
    /// provides a friendly, localized error description for the end user.
    /// </remarks>
    protected Error(ErrorCode errorCode, String description)
    {
        if (String.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException($"'{nameof(description)}' must not be null or whitespace.", nameof(description));
        }

        Code = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        Description = description;
    }

    /// <summary>
    /// Gets a human-readable description of the error. This method can be overridden by derived
    /// classes to provide a custom description using properties of the derived class.
    /// </summary>
    /// <returns>A string representing the error description.</returns>
    protected virtual String? GenerateDescription() => DefaultDescription;

    /// <summary>
    /// Determines whether this error is of a specific error type.
    /// </summary>
    /// <typeparam name="TError">The error type to check for.</typeparam>
    /// <returns>True if this error is of the specified type; otherwise, false.</returns>
    public Boolean IsErrorType<TError>() where TError : Error
        => this is TError;
}
