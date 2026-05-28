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

using Aiel.StrongIds;
using System.Runtime.CompilerServices;

namespace Aiel.Domain;

public abstract class Entity<TKey> : IEquatable<Entity<TKey>>
    where TKey : notnull, IStrongId
{
    public TKey Id { get; protected init; }

    public Int64 Version { get; protected set; }

    protected Entity(TKey id)
    {
        if (IsDefaultKey(id))
        {
            throw new ArgumentException("Entity ID cannot be the default value.", nameof(id));
        }

        Id = id;
    }

    protected Entity()
    {
        Id = default!;
    }

    public Boolean Equals(Entity<TKey>? other)
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

        if (IsDefaultKey(Id) || IsDefaultKey(other.Id))
        {
            return false;
        }

        return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
    }

    public override Boolean Equals(Object? obj) => Equals(obj as Entity<TKey>);

    public override Int32 GetHashCode()
        => IsDefaultKey(Id)
            ? RuntimeHelpers.GetHashCode(this)
            : HashCode.Combine(GetType(), Id);

    public static Boolean operator ==(Entity<TKey>? left, Entity<TKey>? right)
        => left is null
            ? right is null
            : left.Equals(right);

    public static Boolean operator !=(Entity<TKey>? left, Entity<TKey>? right) => !(left == right);

    private static Boolean IsDefaultKey(TKey key) => EqualityComparer<TKey>.Default.Equals(key, default!);
}
