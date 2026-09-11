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

using Aiel.Results;

namespace Aiel.Domain.Errors;

/// <summary>
/// Represents an error indicating that a specific entity was not found.
/// </summary>
public sealed partial class EntityNotFoundError : Error
{
    /// <summary>
    /// Gets the type of the entity that was not found.
    /// </summary>
    public String EntityType { get; init; } = default!;
    /// <summary>
    /// Gets the identifier of the entity that was not found.
    /// </summary>
    public String EntityId { get; init; } = default!;

    /// <summary>
    /// Gets the default description for the error, which includes the entity type and identifier.
    /// </summary>
    protected override String? DefaultDescription => $"Entity of type {EntityType} identified by {EntityId} was not found.";

    /// <summary>
    /// Creates a new instance of <see cref="EntityNotFoundError"/> for the specified entity type and identifier.
    /// </summary>
    /// <typeparam name="T">The type of the entity that was not found.</typeparam>
    /// <param name="entityId">The identifier of the entity that was not found.</param>
    /// <returns>A new instance of <see cref="EntityNotFoundError"/>.</returns>
    public static EntityNotFoundError Create<T>(String entityId) => Create(typeof(T).Name, entityId);

    /// <summary>
    /// Creates a new instance of <see cref="EntityNotFoundError"/> for the specified entity type and identifier.
    /// </summary>
    /// <param name="entityType">The type of the entity that was not found.</param>
    /// <param name="entityId">The identifier of the entity that was not found.</param>
    /// <returns>A new instance of <see cref="EntityNotFoundError"/>.</returns>
    public static EntityNotFoundError Create(String entityType, String entityId) => new()
    {
        EntityType = entityType,
        EntityId = entityId
    };
}

/// <summary>
/// Provides factory methods for creating instances of various errors.
/// </summary>
public static partial class AfError
{
    /// <summary>
    /// Creates a new instance of <see cref="EntityNotFoundError"/> for the specified entity type and identifier.
    /// </summary>
    /// <typeparam name="T">The type of the entity that was not found.</typeparam>
    /// <param name="entityId">The identifier of the entity that was not found.</param>
    /// <returns>A new instance of <see cref="EntityNotFoundError"/>.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static EntityNotFoundError Notfound<T>(Object entityId)
    {
        ArgumentNullException.ThrowIfNull(entityId);

        return new EntityNotFoundError
        {
            EntityType = typeof(T).Name,
            EntityId = entityId.ToString() ?? throw new ArgumentNullException(nameof(entityId))
        };
    }
}
