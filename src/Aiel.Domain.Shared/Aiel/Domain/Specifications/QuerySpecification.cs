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
using System.Linq.Expressions;

namespace Aiel.Domain.Specifications;

/// <summary>
/// Represents a specification for querying and retrieving multiple entities of type T.
/// </summary>
/// <typeparam name="TEntity">The type of the entities to query.</typeparam>
public class QuerySpecification<TEntity>
    : EntitySpecification<TEntity>, IQueryMultipleSpecification<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuerySpecification{T}"/>
    /// class that matches everything unless Expression is set in the derived
    /// class.
    /// </summary>
    protected QuerySpecification() : base(_ => true)
    {
        Page = Page.Default;
        Sort = SortOrder.None;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuerySpecification{T}"/> class with the specified predicate.
    /// </summary>
    /// <param name="expression">The predicate to apply to the query that matches zero or more entities.</param>
    public QuerySpecification(Expression<Func<TEntity, Boolean>> expression) : base(expression)
    {
        Page = Page.Default;
        Sort = SortOrder.None;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuerySpecification{T}"/>
    /// class that matches everything unless the parent
    /// <see cref="ExpressionSpecification{T}.Expression"/> is set in the derived
    /// class. <see cref="Page"/> and <see cref="Sort"/> are copied from the
    /// <paramref name="query"/> parameter.
    /// </summary>
    /// <param name="query">The query to initialize the specification with.</param>
    public QuerySpecification(IQueryMultiple query) : base(_ => true)
    {
        ArgumentNullException.ThrowIfNull(query);

        Page = query.Page;
        Sort = query.Sort;
    }

    /// <summary>
    /// <para>
    /// Initializes a new instance of the <see cref="QuerySpecification{T}"/>
    /// class that matches everything unless the parent
    /// <see cref="ExpressionSpecification{T}.Expression"/> is set in the derived
    /// class.
    /// </para>
    /// <para>
    /// <see cref="Page"/> and <see cref="Sort"/> are set from the
    /// <paramref name="page"/> and <paramref name="sort"/> parameters when they are
    /// not null; otherwise they are set to <see cref="Page.Default"/> and
    /// <see cref="SortOrder.None"/>.
    /// </para>
    /// </summary>
    /// <param name="specification">The specification to apply to the query.</param>
    /// <param name="sort">The sort order to apply to the query.</param>
    /// <param name="page">The page request to apply to the query.</param>
    /// <exception cref="ArgumentNullException">Thrown when the specification is null.</exception>
    public QuerySpecification(IEntitySpecification<TEntity> specification, SortOrder? sort = null, Page? page = null)
        : base(specification.ToExpression())
    {
        Sort = sort ?? SortOrder.None;
        Page = page ?? Page.Default;
    }

    /// <summary>
    /// Gets or sets the pagination information for the query results. Defaults to <see cref="Page.Default"/>.
    /// </summary>
    /// <remarks>
    /// Setting <see cref="Page"/> to <see langword="null"/> will reset it to <see cref="Page.Default"/>.
    /// </remarks>
    public Page Page { get; set => field = value ?? Page.Default; } = Page.Default;

    /// <summary>
    /// Gets or sets the sorting order for the query results. Defaults to <see cref="SortOrder.None"/>.
    /// </summary>
    /// <remarks>
    /// Setting <see cref="Sort"/> to <see langword="null"/> will reset it to <see cref="SortOrder.None"/>.
    /// </remarks>
    public SortOrder Sort { get; set => field = value ?? SortOrder.None; } = SortOrder.None;

    /// <summary>
    /// Combines two specifications using the specified combiner function (e.g., AND, OR).
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <param name="combiner">The function used to combine the two specifications.</param>
    /// <returns>A new specification that represents the combination of the two specifications using the specified combiner function.</returns>
    protected static QuerySpecification<TEntity> CombineSpecification(QuerySpecification<TEntity> left, QuerySpecification<TEntity> right, Func<Expression, Expression, BinaryExpression> combiner)
    {
        var leftExpression = left.ToExpression();
        var rightExpression = right.ToExpression();
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TEntity));
        var combined = combiner.Invoke(
            new ReplaceParameterVisitor { { leftExpression.Parameters.Single(), parameter } }.Visit(leftExpression.Body),
            new ReplaceParameterVisitor { { rightExpression.Parameters.Single(), parameter } }.Visit(rightExpression.Body));
        return new QuerySpecification<TEntity>.ConstructedQuerySpecification(System.Linq.Expressions.Expression.Lambda<Func<TEntity, Boolean>>(combined, parameter));
    }

    /// <inheritdoc/>
    public static implicit operator Expression<Func<TEntity, Boolean>>(QuerySpecification<TEntity> spec) => spec.ToExpression();

    /// <inheritdoc/>
    public static QuerySpecification<TEntity> operator &(QuerySpecification<TEntity> left, QuerySpecification<TEntity> right)
        => QuerySpecification<TEntity>.CombineSpecification(left, right, System.Linq.Expressions.Expression.AndAlso);

    /// <inheritdoc/>
    public static QuerySpecification<TEntity> operator |(QuerySpecification<TEntity> left, QuerySpecification<TEntity> right)
        => QuerySpecification<TEntity>.CombineSpecification(left, right, System.Linq.Expressions.Expression.OrElse);

    /// <inheritdoc/>
    public static QuerySpecification<TEntity> operator !(QuerySpecification<TEntity> spec)
    {
        var predicate = spec.ToExpression();
        var newExpression = System.Linq.Expressions.Expression.Lambda<Func<TEntity, Boolean>>(System.Linq.Expressions.Expression.Not(predicate.Body), predicate.Parameters[0]);
        return new ConstructedQuerySpecification(newExpression);
    }

    /// <summary>
    /// Represents a constructed query specification that is created from a given expression.
    /// </summary>
    /// <param name="specificationExpression">The expression used to create the specification.</param>
    protected class ConstructedQuerySpecification(Expression<Func<TEntity, Boolean>> specificationExpression)
        : QuerySpecification<TEntity>(specificationExpression)
    {
    }
}
