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

namespace Aiel.Authorization;

/// <summary>
/// Represents an opaque subject key without exposing raw strings in public models.
/// </summary>
public readonly record struct AuthorizationSubjectKey
{
    private readonly String? _value;

    private AuthorizationSubjectKey(String value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the canonical subject key value.
    /// </summary>
    public String Value => _value ?? String.Empty;

    /// <summary>
    /// Creates a subject key or throws when the supplied value is not valid.
    /// </summary>
    /// <param name="value">The candidate subject key.</param>
    /// <returns>The validated subject key.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not a valid subject key.</exception>
    public static AuthorizationSubjectKey From(String value)
    {
        if (!TryCreate(value, out var subjectKey))
        {
            throw new ArgumentException("Subject keys cannot be empty or whitespace.", nameof(value));
        }

        return subjectKey;
    }

    /// <summary>
    /// Attempts to create a subject key without throwing for expected validation failures.
    /// </summary>
    /// <param name="value">The candidate subject key.</param>
    /// <param name="subjectKey">The created subject key when validation succeeds.</param>
    /// <returns><see langword="true"/> when <paramref name="value"/> is valid; otherwise, <see langword="false"/>.</returns>
    public static Boolean TryCreate(String? value, out AuthorizationSubjectKey subjectKey)
    {
        if (PermissionTextValidator.TryCreateKey(value, out var normalizedValue))
        {
            subjectKey = new AuthorizationSubjectKey(normalizedValue);
            return true;
        }

        subjectKey = default;
        return false;
    }

    /// <summary>
    /// Returns the canonical subject key.
    /// </summary>
    /// <returns>The canonical subject key.</returns>
    public override String ToString() => Value;
}
