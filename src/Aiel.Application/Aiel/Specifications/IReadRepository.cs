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

using Aiel.Queries;
using System.Linq.Expressions;

namespace Aiel.Specifications;

/// <summary>
/// Defines the read-side persistence contract for specification-based queries.
/// </summary>
/// <typeparam name="TEntity">The read model entity type.</typeparam>
public interface IReadRepository<TEntity> : IDisposable
    where TEntity : class
{
    IAsyncEnumerable<TEntity> FindAsync(
        IQuerySpecification<TEntity> specification,
        SortRequest? sort = null,
        PageRequest? page = null);

    Task<TEntity?> GetAsync(
        IQuerySpecification<TEntity> specification,
        SortRequest? sort = null,
        CancellationToken cancellationToken = default);

    Task<Boolean> AnyAsync(IQuerySpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<Boolean> AnyAsync(Expression<Func<TEntity, Boolean>> predicate, CancellationToken cancellationToken = default);

    Task<Int32> CountAsync(IQuerySpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<Int32> CountAsync(Expression<Func<TEntity, Boolean>> predicate, CancellationToken cancellationToken = default);
}