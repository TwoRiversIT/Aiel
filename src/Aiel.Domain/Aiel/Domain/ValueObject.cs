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

namespace Aiel.Domain;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<Object?> GetEqualityComponents();

    public Boolean Equals(ValueObject? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        using var left = GetEqualityComponents().GetEnumerator();
        using var right = other.GetEqualityComponents().GetEnumerator();

        while (true)
        {
            var leftHasNext = left.MoveNext();
            var rightHasNext = right.MoveNext();

            if (!leftHasNext && !rightHasNext)
            {
                return true;
            }

            if (leftHasNext != rightHasNext)
            {
                return false;
            }

            if (!Equals(left.Current, right.Current))
            {
                return false;
            }
        }
    }

    public override Boolean Equals(Object? obj) => Equals(obj as ValueObject);

    public override Int32 GetHashCode()
    {
        var hash = new HashCode();

        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    public static Boolean operator ==(ValueObject? left, ValueObject? right)
        => left is null
            ? right is null
            : left.Equals(right);

    public static Boolean operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}
