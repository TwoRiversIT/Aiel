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

/// <summary>
/// Represents a base class for entities with a strongly-typed identifier and versioning support.
/// </summary>
/// <typeparam name="TKey">The type of the strongly-typed identifier.</typeparam>
public abstract class Entity<TKey> : IEquatable<Entity<TKey>>
    where TKey : notnull, IStrongId
{
    /// <summary>
    /// Gets the identifier of the entity.
    /// </summary>
    public TKey Id { get; protected init; }

    /// <summary>
    /// Gets the version of the entity.
    /// </summary>
    public Int64 Version { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TKey}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <exception cref="ArgumentException">Thrown when the provided identifier is the default value.</exception>
    protected Entity(TKey id)
    {
        if (id.IsDefault)
        {
            throw new ArgumentException("Entity ID cannot be the default value.", nameof(id));
        }

        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TKey}"/> class with the default identifier.
    /// </summary>
    protected Entity()
    {
        Id = default!;
    }

    /// <summary>
    /// Determines whether the specified entity is equal to the current entity based on their identifiers and types.
    /// </summary>
    /// <param name="other">The entity to compare with the current entity.</param>
    /// <returns><c>true</c> if the specified entity is equal to the current entity; otherwise, <c>false</c>.</returns>
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

        if (Id.IsDefault || other.Id.IsDefault)
        {
            return false;
        }

        return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity based on their identifiers and types.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override Boolean Equals(Object? obj) => Equals(obj as Entity<TKey>);

    /// <inheritdoc/>
    public override Int32 GetHashCode()
        => Id.IsDefault
            ? RuntimeHelpers.GetHashCode(this)
            : HashCode.Combine(GetType(), Id);

    /// <inheritdoc/>
    public static Boolean operator ==(Entity<TKey>? left, Entity<TKey>? right)
        => left is null
            ? right is null
            : left.Equals(right);

    /// <inheritdoc/>
    public static Boolean operator !=(Entity<TKey>? left, Entity<TKey>? right) => !(left == right);
}
