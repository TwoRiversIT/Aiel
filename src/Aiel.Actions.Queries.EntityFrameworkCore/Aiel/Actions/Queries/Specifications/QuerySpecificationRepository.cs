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

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Aiel.Actions.Queries.Specifications;

public class QuerySpecificationRepository<TEntity, TDbContext>(TDbContext context) : ISpecificationRepository<TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    private readonly TDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private Boolean _disposed;

    public IAsyncEnumerable<TEntity> FindAsync(IQueryMultipleSpecification<TEntity> query)
        => FindAsync(query.Specification, query.SortOrder, query.Sort);

    public IAsyncEnumerable<TEntity> FindAsync(
        ISpecification<TEntity> specification,
        SortOrder? sort = null,
        Page? page = null)
        => _context.QueryMultiple(sort, page, specification).AsAsyncEnumerable();

    public async Task<TEntity?> GetAsync(
        ISpecification<TEntity> specification,
        SortOrder? sort = null,
        CancellationToken cancellationToken = default)
        => await _context.QueryMultiple(sort, specification: specification).SingleOrDefaultAsync(cancellationToken);

    public async Task<Boolean> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => await _context.QueryMultiple(specification: specification).AnyAsync(cancellationToken);

    public async Task<Boolean> AnyAsync(Expression<Func<TEntity, Boolean>> predicate, CancellationToken cancellationToken = default)
        => await _context.Set<TEntity>().AnyAsync(predicate, cancellationToken);

    public async Task<Int32> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => await _context.QueryMultiple(specification: specification).CountAsync(cancellationToken);

    public async Task<Int32> CountAsync(Expression<Func<TEntity, Boolean>> predicate, CancellationToken cancellationToken = default)
        => await _context.Set<TEntity>().CountAsync(predicate, cancellationToken);

    protected virtual void Dispose(Boolean disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
