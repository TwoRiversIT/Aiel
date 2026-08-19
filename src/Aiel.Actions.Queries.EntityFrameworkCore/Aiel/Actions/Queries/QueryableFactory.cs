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

using Aiel.Actions.Queries.Specifications;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Aiel.Actions.Queries;

public static class QueryableFactory
{
    public static IQueryable<TEntity> QueryMultiple<TEntity>(this DbContext dbContext, IQueryMultiple request, ISpecification<TEntity> specification)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(specification);

        return QueryMultiple(dbContext, request.SortOrder, request.Sort, specification);
    }

    public static IQueryable<TEntity> QueryMultiple<TEntity>(this DbContext dbContext, IQueryMultiple request, Expression<Func<TEntity, Boolean>> predicate)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(predicate);

        return dbContext.QueryMultiple(request.SortOrder, request.Sort, predicate);
    }

    public static IQueryable<TEntity> QueryMultiple<TEntity>(this DbContext dbContext, SortOrder? sort = null, Page? page = null, ISpecification<TEntity>? specification = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        return dbContext.QueryMultiple(sort, page, specification?.ToExpression());
    }

    public static IQueryable<TEntity> QueryMultiple<TEntity>(this DbContext dbContext, SortOrder? sort = null, Page? page = null, Expression<Func<TEntity, Boolean>>? predicate = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var query = dbContext.GetQueryable<TEntity>();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (sort?.HasValues == true)
        {
            query = query.ApplySorting(sort);
        }

        if (page is not null)
        {
            query = query.ApplyPaging(page);
        }

        return query;
    }

    public static IQueryable<TEntity> GetQueryable<TEntity>(this DbContext dbContext)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        return dbContext.Set<TEntity>().AsQueryable();
    }
}
