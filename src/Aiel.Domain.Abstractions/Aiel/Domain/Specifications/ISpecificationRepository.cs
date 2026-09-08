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

using Aiel.Actions.Queries;

namespace Aiel.Domain.Specifications;

/// <summary>
/// Defines the read-side persistence contract for specification-based queries.
/// Honestly though, why are you using this? You should be using Entity
/// Framework Core which is already an abstraction over a database and
/// supports specifications in the form of
/// <c>Expression&lt;Func&lt;TEntity, Boolean&gt;&gt;</c>.
/// </summary>
/// <typeparam name="TEntity">The read model entity type.</typeparam>
public interface ISpecificationRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Finds entities that satisfy the given specification, with optional sorting and paging.
    /// </summary>
    /// <param name="specification">The specification to filter the entities.</param>
    /// <param name="sort">The optional sort order.</param>
    /// <param name="page">The optional paging information.</param>
    /// <returns>An asynchronous stream of entities that satisfy the specification.</returns>
    IAsyncEnumerable<TEntity> FindAsync(IEntitySpecification<TEntity> specification, SortOrder? sort = null, Page? page = null);

    /// <summary>
    /// Gets a single entity that satisfies the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity that satisfies the specification, or <c>null</c> if none is found.</returns>
    Task<TEntity?> GetAsync(IEntitySpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any entities satisfy the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if any entities satisfy the specification; otherwise, <c>false</c>.</returns>
    Task<Boolean> AnyAsync(IEntitySpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of entities that satisfy the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of entities that satisfy the specification.</returns>
    Task<Int32> CountAsync(IEntitySpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries multiple entities that satisfy the given specification.
    /// </summary>
    /// <param name="specification">The specification to filter the entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result containing the entities that satisfy the specification.</returns>
    Task<MultipleResult<TEntity>> QueryAsync(IQueryMultipleSpecification<TEntity> specification, CancellationToken cancellationToken = default);
}
