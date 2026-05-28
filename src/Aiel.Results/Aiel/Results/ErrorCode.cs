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
/// Represents an error code value used to identify specific error conditions.
/// </summary>
/// <remarks>
/// <para>
/// ErrorCode uses value equality semantics based on type and name. Each error code type
/// should define a single static Instance property that serves as the canonical reference.
/// </para>
/// <para>
/// Users can extend ErrorCode to create custom error types by creating a sealed class
/// that inherits from ErrorCode and defines a public static Instance property.
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the ErrorCode class.
/// </remarks>
//[JsonConverter(typeof(ErrorCodeJsonConverter))]
public abstract class ErrorCode() : IEquatable<ErrorCode>
{
    /// <summary>
    /// A human-readable name for the error code.
    /// </summary>
    protected abstract String Name { get; }

    /// <summary>
    /// Returns the name of the error code.
    /// </summary>
    /// <returns>The name of the error code.</returns>
    public override String ToString() => Name;

    /// <summary>
    /// Implicitly converts an <see cref="ErrorCode"/> to a <see cref="String"/> by returning its name.
    /// </summary>
    /// <param name="errorCode">The error code to convert.</param>
    /// <returns>The name of the error code.</returns>
    public static implicit operator String(ErrorCode errorCode) => errorCode.Name;

    /// <summary>
    /// Determines whether this error code is equal to another based on type and name.
    /// </summary>
    /// <param name="other">The error code to compare to.</param>
    /// <returns>True if both have the same type and name; otherwise, false.</returns>
    public virtual Boolean Equals(ErrorCode? other)
        => other is not null && GetType() == other.GetType() && Name == other.Name;

    /// <summary>
    /// Determines whether this error code is equal to another object based on type and name.
    /// </summary>
    /// <param name="obj">The object to compare to.</param>
    /// <returns>True if both have the same type and name; otherwise, false.</returns>
    public override Boolean Equals(Object? obj) => Equals(obj as ErrorCode);

    /// <summary>
    /// Returns the hash code for this error code based on its type and name.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override Int32 GetHashCode() => Name.GetHashCode();
}
