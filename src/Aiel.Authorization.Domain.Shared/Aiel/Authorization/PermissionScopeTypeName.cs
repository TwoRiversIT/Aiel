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
/// Represents the human-readable name of a permission scope type.
/// </summary>
public readonly record struct PermissionScopeTypeName
{
    private readonly String? _value;

    private PermissionScopeTypeName(String value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the canonical scope type name value.
    /// </summary>
    public String Value => _value ?? String.Empty;

    /// <summary>
    /// Creates a scope type name or throws when the supplied value is not valid.
    /// </summary>
    /// <param name="value">The candidate scope type name.</param>
    /// <returns>The validated scope type name.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not a valid scope type name.</exception>
    public static PermissionScopeTypeName From(String value)
    {
        if (!TryCreate(value, out var scopeTypeName))
        {
            throw new ArgumentException("Scope type names must be single identifiers and cannot be empty.", nameof(value));
        }

        return scopeTypeName;
    }

    /// <summary>
    /// Attempts to create a scope type name without throwing for expected validation failures.
    /// </summary>
    /// <param name="value">The candidate scope type name.</param>
    /// <param name="scopeTypeName">The created scope type name when validation succeeds.</param>
    /// <returns><see langword="true"/> when <paramref name="value"/> is valid; otherwise, <see langword="false"/>.</returns>
    public static Boolean TryCreate(String? value, out PermissionScopeTypeName scopeTypeName)
    {
        if (PermissionTextValidator.TryCreateTypeName(value, out var normalizedValue))
        {
            scopeTypeName = new PermissionScopeTypeName(normalizedValue);
            return true;
        }

        scopeTypeName = default;
        return false;
    }

    /// <summary>
    /// Returns the canonical scope type name.
    /// </summary>
    /// <returns>The canonical scope type name.</returns>
    public override String ToString() => Value;
}
