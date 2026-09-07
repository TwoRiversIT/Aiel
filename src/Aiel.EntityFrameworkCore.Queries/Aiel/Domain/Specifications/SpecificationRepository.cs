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
using Aiel.Domain.Queries;
using Aiel.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aiel.Domain.Specifications;

/// <inheritdoc cref="ISpecificationRepository{TEntity}"/>
public class SpecificationRepository<TEntity, TDbContext>(TDbContext context) : ISpecificationRepository<TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    private readonly TDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public IAsyncEnumerable<TEntity> FindAsync(IEntitySpecification<TEntity> specification, SortOrder? sort = null, Page? page = null)
        => _context.QueryMultiple(sort ?? SortOrder.None, page ?? Page.Default, specification).AsAsyncEnumerable();

    /// <inheritdoc />
    public async Task<TEntity?> GetAsync(IEntitySpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => await _context.GetQueryable<TEntity>().SingleOrDefaultAsync(specification.ToExpression(), cancellationToken);

    /// <inheritdoc />
    public async Task<Boolean> AnyAsync(IEntitySpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => await _context.GetQueryable<TEntity>().AnyAsync(specification.ToExpression(), cancellationToken);

    /// <inheritdoc />
    public async Task<MultipleResult<TEntity>> QueryAsync(IQueryMultipleSpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => await _context.GetQueryable<TEntity>().ToQueryMultipleResultAsync(specification, cancellationToken);

    /// <inheritdoc />
    public async Task<Int32> CountAsync(IEntitySpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => await _context.GetQueryable<TEntity>().CountAsync(specification.ToExpression(), cancellationToken);
}
