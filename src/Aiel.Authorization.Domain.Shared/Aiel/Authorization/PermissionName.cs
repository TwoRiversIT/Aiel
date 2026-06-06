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
/// Represents the persisted human-readable permission key.
/// </summary>
public readonly record struct PermissionName
{
    private readonly String? _value;

    private PermissionName(String value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the canonical permission name value.
    /// </summary>
    public String Value => _value ?? String.Empty;

    /// <summary>
    /// Creates a permission name or throws when the supplied value is not valid.
    /// </summary>
    /// <param name="value">The candidate permission name.</param>
    /// <returns>The validated permission name.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not a valid permission name.</exception>
    public static PermissionName From(String value)
    {
        if (!TryCreate(value, out var permissionName))
        {
            throw new ArgumentException("Permission names must be dot-delimited identifiers and cannot be empty.", nameof(value));
        }

        return permissionName;
    }

    /// <summary>
    /// Attempts to create a permission name without throwing for expected validation failures.
    /// </summary>
    /// <param name="value">The candidate permission name.</param>
    /// <param name="permissionName">The created permission name when validation succeeds.</param>
    /// <returns><see langword="true"/> when <paramref name="value"/> is valid; otherwise, <see langword="false"/>.</returns>
    public static Boolean TryCreate(String? value, out PermissionName permissionName)
    {
        if (PermissionTextValidator.TryCreatePermissionName(value, out var normalizedValue))
        {
            permissionName = new PermissionName(normalizedValue);
            return true;
        }

        permissionName = default;
        return false;
    }

    /// <summary>
    /// Returns the canonical permission name.
    /// </summary>
    /// <returns>The canonical permission name.</returns>
    public override String ToString() => Value;
}
