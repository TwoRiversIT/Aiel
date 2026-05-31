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
/// Represents a capability-snapshot continuation token without using <see langword="null"/>.
/// </summary>
public readonly record struct CapabilityContinuationToken
{
    private readonly String? _value;

    private CapabilityContinuationToken(String value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the explicit empty continuation token used for the first page.
    /// </summary>
    public static CapabilityContinuationToken Empty => default;

    /// <summary>
    /// Gets the token value.
    /// </summary>
    public String Value => _value ?? String.Empty;

    /// <summary>
    /// Gets a value indicating whether this token is empty.
    /// </summary>
    public Boolean IsEmpty => String.IsNullOrEmpty(_value);

    /// <summary>
    /// Creates a continuation token from a raw value.
    /// </summary>
    /// <param name="value">The raw token value. Empty or whitespace input becomes <see cref="Empty"/>.</param>
    /// <returns>The normalized continuation token.</returns>
    public static CapabilityContinuationToken From(String? value)
    {
        if (String.IsNullOrWhiteSpace(value))
        {
            return Empty;
        }

        return new CapabilityContinuationToken(value.Trim());
    }

    /// <summary>
    /// Returns the token text.
    /// </summary>
    /// <returns>The token text, or an empty string for the first page.</returns>
    public override String ToString() => Value;
}
