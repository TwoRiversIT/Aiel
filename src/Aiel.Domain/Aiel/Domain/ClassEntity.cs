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

namespace Aiel.Domain;

/// <summary>
/// Represents a base class for entities with a strongly-typed identifier and versioning support.
/// </summary>
/// <typeparam name="TKey">The type of the strongly-typed identifier.</typeparam>
public abstract class ClassEntity<TKey> : IEntity<TKey>, IHasStrongId
    where TKey : notnull, IStrongId
{
    /// <summary>
    /// Gets the identifier of the entity.
    /// </summary>
    public TKey Id { get; protected init; }
    IStrongId IHasStrongId.Id => Id;

    /// <summary>
    /// Gets the version of the entity.
    /// </summary>
    public Int64 Version { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassEntity{TKey}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <exception cref="ArgumentException">Thrown when the provided identifier is the default value.</exception>
    protected ClassEntity(TKey id)
    {
        if (id.IsDefault)
        {
            throw new ArgumentException("Entity ID cannot be the default value.", nameof(id));
        }

        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassEntity{TKey}"/> class with the default identifier.
    /// </summary>
    protected ClassEntity()
    {
        Id = default!;
    }
}
