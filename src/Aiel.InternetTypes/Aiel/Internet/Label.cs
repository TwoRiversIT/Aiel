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

using System.Diagnostics.CodeAnalysis;

namespace Aiel.Internet;

[SuppressMessage("Design", "CA1036:Override methods on comparable types", Justification = "Domain Names are effectively strings so CompareTo() or StringComparer is preferred over <, >, <=, >=.")]
public class Label : IEquatable<Label>, IComparable<Label>
{
    private readonly String _label;

    [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    public Label(String label)
    {
        ThrowIfInvalid(label);

        _label = label;
    }

    public override Int32 GetHashCode()
        => _label?.GetHashCode() ?? 0;

    public override String ToString() => _label;

    public Int32 CompareTo(Label? other)
        => String.Compare(_label, other?._label, StringComparison.InvariantCultureIgnoreCase);

    public Boolean Equals(Label? other)
        => ReferenceEquals(this, other) || _label.Equals(other?._label, StringComparison.InvariantCultureIgnoreCase);

    public override Boolean Equals(Object? other)
        => other is not null && (ReferenceEquals(this, other) || (other is Label domainName && Equals(domainName)));

    public static implicit operator Label(String domainName) => new(domainName);

    public static Label FromString(String domainName) => new(domainName);

    public static implicit operator String(Label domainName) => domainName._label;

    public static String FromDomainName(Label domainName) => domainName._label;

    public static Boolean operator ==(Label a, Label b)
    {
        // If both are null, or both are same instance, return true.
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if ((a is null) || (b is null))
        {
            return false;
        }

        // Return true if the fields match:
        return a._label == b._label;
    }

    public static Boolean operator !=(Label a, Label b) => !(a == b);

    private static void ThrowIfInvalid(String label)
    {
        ArgumentNullException.ThrowIfNull(label);

        if (String.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Invalid: Must not be Empty or Whitespace", nameof(label));
        }

        if (label.Length > 63)
        {
            throw new ArgumentException("Invalid Length: The length of any one label is limited to between 1 and 63 octets.", nameof(label));
        }
    }
}
