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

[DebuggerDisplay("{Code}")]
public readonly struct Province : IRegion, IEquatable<Province>, IComparable<Province>
{
    public static readonly Province Empty = new();

    public Province()
    {
        // Private constructor for Empty instance
    }

    internal Province(String code, String name) : this()
    {
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
    }

    public String Code { get; } = String.Empty;
    public String Name { get; } = String.Empty;

    public override String ToString() => Code;

    // IEquatable<Province> implementation
    public Boolean Equals(Province other)
        => String.Equals(Code, other.Code, StringComparison.OrdinalIgnoreCase)
        && String.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

    public override Boolean Equals(Object? obj) => obj is Province other && Equals(other);

    public override Int32 GetHashCode() => HashCode.Combine(Code.ToUpperInvariant(), Name.ToUpperInvariant());

    // IComparable<Province> implementation
    public Int32 CompareTo(Province other)
    {
        // Compare by Code first, then by Name if Codes are equal
        Int32 codeComparison = String.Compare(Code, other.Code, StringComparison.OrdinalIgnoreCase);
        return codeComparison != 0 ? codeComparison : String.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }

    // Equality operators
    public static Boolean operator ==(Province left, Province right) => left.Equals(right);

    public static Boolean operator !=(Province left, Province right) => !(left == right);

    // Comparison operators
    public static Boolean operator <(Province left, Province right) => left.CompareTo(right) < 0;

    public static Boolean operator >(Province left, Province right) => left.CompareTo(right) > 0;

    public static Boolean operator <=(Province left, Province right) => left.CompareTo(right) <= 0;

    public static Boolean operator >=(Province left, Province right) => left.CompareTo(right) >= 0;

    public static implicit operator String(Province province) => province.ToString();
}
