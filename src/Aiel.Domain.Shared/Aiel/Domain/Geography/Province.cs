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

using System.Diagnostics;

namespace Aiel.Domain.Geography;

/// <summary>
/// Represents a Canadian province with its name and code.
/// </summary>
[DebuggerDisplay("{Code}")]
public readonly struct Province : IRegion, IEquatable<Province>, IComparable<Province>
{
    /// <summary>
    /// Gets an empty <see cref="Province"/> instance with empty name and code.
    /// </summary>
    public static readonly Province Empty = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Province"/> struct with empty name and code.
    /// </summary>
    public Province()
    {
    }

    internal Province(String code, String name) : this()
    {
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Gets the code of the province (e.g., "ON" for Ontario).
    /// </summary>
    public String Code { get; } = String.Empty;

    /// <summary>
    /// Gets the name of the province (e.g., "Ontario").
    /// </summary>
    public String Name { get; } = String.Empty;

    /// <summary>
    /// Returns a string representation of the province, which is its code.
    /// </summary>
    /// <returns>The code of the province.</returns>
    public override String ToString() => Code;

    // IEquatable<Province> implementation
    /// <summary>
    /// Determines whether the specified <see cref="Province"/> is equal to the current <see cref="Province"/>.
    /// </summary>
    /// <param name="other">The province to compare with the current province.</param>
    /// <returns><c>true</c> if the specified province is equal to the current province; otherwise, <c>false</c>.</returns>
    public Boolean Equals(Province other)
        => String.Equals(Code, other.Code, StringComparison.OrdinalIgnoreCase)
        && String.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="Province"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current province.</param>
    /// <returns><c>true</c> if the specified object is equal to the current province; otherwise, <c>false</c>.</returns>
    public override Boolean Equals(Object? obj) => obj is Province other && Equals(other);

    /// <inheritdoc/>
    public override Int32 GetHashCode() => HashCode.Combine(Code.ToUpperInvariant(), Name.ToUpperInvariant());

    // IComparable<Province> implementation
    /// <summary>
    /// Compares the current <see cref="Province"/> with another <see cref="Province"/> and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other province.
    /// </summary>
    /// <param name="other">The province to compare with the current province.</param>
    /// <returns>
    /// A value that indicates the relative order of the provinces being compared.
    /// The return value has these meanings:
    /// <list type="bullet">
    /// <item>
    /// <description>Less than zero: This instance precedes <paramref name="other"/> in the sort order.</description>
    /// </item>
    /// <item>
    /// <description>Zero: This instance occurs in the same position in the sort order as <paramref name="other"/>.</description>
    /// </item>
    /// <item>
    /// <description>Greater than zero: This instance follows <paramref name="other"/> in the sort order.</description>
    /// </item>
    /// </list>
    /// </returns>
    public Int32 CompareTo(Province other)
    {
        // Compare by Code first, then by Name if Codes are equal
        Int32 codeComparison = String.Compare(Code, other.Code, StringComparison.OrdinalIgnoreCase);
        return codeComparison != 0 ? codeComparison : String.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }

    // Equality operators
    /// <inheritdoc/>
    public static Boolean operator ==(Province left, Province right) => left.Equals(right);

    /// <inheritdoc/>
    public static Boolean operator !=(Province left, Province right) => !(left == right);

    // Comparison operators
    /// <inheritdoc/>
    public static Boolean operator <(Province left, Province right) => left.CompareTo(right) < 0;

    /// <inheritdoc/>
    public static Boolean operator >(Province left, Province right) => left.CompareTo(right) > 0;

    /// <inheritdoc/>
    public static Boolean operator <=(Province left, Province right) => left.CompareTo(right) <= 0;

    /// <inheritdoc/>
    public static Boolean operator >=(Province left, Province right) => left.CompareTo(right) >= 0;

    /// <inheritdoc/>
    public static implicit operator String(Province province) => province.ToString();
}
