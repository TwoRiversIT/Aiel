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

using System.Linq.Expressions;

namespace Aiel.Actions.Queries.Specifications;

public class QuerySpecification<TEntity>(Expression<Func<TEntity, Boolean>> predicate)
    : ExpressionSpecification<TEntity>(predicate), IQuerySpecification<TEntity>
{
    public ICollection<Expression<Func<TEntity, Object>>> Includes { get; } = [];
    public Expression<Func<TEntity, Object>>? OrderBy { get; private set; }
    public Expression<Func<TEntity, Object>>? ThenBy { get; private set; }
    public Expression<Func<TEntity, Object>>? OrderByDescending { get; private set; }
    public Expression<Func<TEntity, Object>>? GroupBy { get; private set; }

    public Int32 PageNo { get; private set; }
    public Int32 PageSize { get; private set; }
    public Boolean IsPagingEnabled => PageNo > 0 && PageSize > 0 && (OrderBy != null || OrderByDescending != null);
    public Boolean IsReadOnly { get; private set; }

    public virtual QuerySpecification<TEntity> SetReadOnly(Boolean isReadOnly = true)
    {
        IsReadOnly = isReadOnly;
        return this;
    }

    public virtual QuerySpecification<TEntity> ApplyGroupBy(Expression<Func<TEntity, Object>> groupByExpression)
    {
        GroupBy = groupByExpression;
        return this;
    }

    public virtual QuerySpecification<TEntity> AddInclude(Expression<Func<TEntity, Object>> includeExpression)
    {
        Includes.Add(includeExpression);
        return this;
    }

    public virtual QuerySpecification<TEntity> ApplyOrderBy(Expression<Func<TEntity, Object>> orderByExpression)
    {
        OrderBy = orderByExpression;
        return this;
    }

    public virtual QuerySpecification<TEntity> ApplyThenBy(Expression<Func<TEntity, Object>> orderByExpression)
    {
        ThenBy = orderByExpression;
        return this;
    }

    public virtual QuerySpecification<TEntity> ApplyOrderByDescending(Expression<Func<TEntity, Object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
        return this;
    }

    public virtual QuerySpecification<TEntity> ApplyPaging(Int32 pageNo, Int32 pageSize)
    {
        PageNo = pageNo;
        PageSize = pageSize;
        return this;
    }

    public virtual QuerySpecification<TEntity> ApplyPaging(Int32 pageNo, Int32 pageSize, Expression<Func<TEntity, Object>> orderByExpression, Expression<Func<TEntity, Object>>? thenByExpression = null)
    {
        PageNo = pageNo;
        PageSize = pageSize;
        OrderBy = orderByExpression;
        ThenBy = thenByExpression;
        return this;
    }
    protected static QuerySpecification<TEntity> CombineSpecification(QuerySpecification<TEntity> left, QuerySpecification<TEntity> right, Func<Expression, Expression, BinaryExpression> combiner)
    {
        var leftExpression = left.ToExpression();
        var rightExpression = right.ToExpression();
        var parameter = Expression.Parameter(typeof(TEntity));
        var combined = combiner.Invoke(
            new ReplaceParameterVisitor { { leftExpression.Parameters.Single(), parameter } }.Visit(leftExpression.Body),
            new ReplaceParameterVisitor { { rightExpression.Parameters.Single(), parameter } }.Visit(rightExpression.Body));
        return new ConstructedQuerySpecification(Expression.Lambda<Func<TEntity, Boolean>>(combined, parameter));
    }

    public static implicit operator Expression<Func<TEntity, Boolean>>(QuerySpecification<TEntity> spec) => spec.ToExpression();

    public static QuerySpecification<TEntity> operator &(QuerySpecification<TEntity> left, QuerySpecification<TEntity> right)
        => CombineSpecification(left, right, Expression.AndAlso);

    public static QuerySpecification<TEntity> operator |(QuerySpecification<TEntity> left, QuerySpecification<TEntity> right)
        => CombineSpecification(left, right, Expression.OrElse);

    public static QuerySpecification<TEntity> operator !(QuerySpecification<TEntity> spec)
    {
        var predicate = spec.ToExpression();
        var newExpression = Expression.Lambda<Func<TEntity, Boolean>>(Expression.Not(predicate.Body), predicate.Parameters[0]);
        return new ConstructedQuerySpecification(newExpression);
    }

    protected class ConstructedQuerySpecification(Expression<Func<TEntity, Boolean>> specificationExpression) : QuerySpecification<TEntity>(specificationExpression)
    {
    }
}
