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

namespace Aiel.Domain.Specifications;

/// <summary>
/// Represents a specification that can be used to define business rules and criteria for entities of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to which the specification applies.</typeparam>
/// <param name="predicate">The expression that defines the criteria for the specification.</param>
public class EntitySpecification<TEntity>(Expression<Func<TEntity, Boolean>> predicate)
    : ExpressionSpecification<TEntity>(predicate), ISpecification<TEntity>
{
    /// <summary>
    /// Combines two specifications using the specified combiner function (e.g., AND, OR).
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <param name="combiner">The function used to combine the two specifications.</param>
    /// <returns>A new specification that represents the combination of the two specifications using the specified combiner function.</returns>
    protected static EntitySpecification<TEntity> CombineSpecification(EntitySpecification<TEntity> left, EntitySpecification<TEntity> right, Func<Expression, Expression, BinaryExpression> combiner)
    {
        var leftExpression = left.ToExpression();
        var rightExpression = right.ToExpression();
        var parameter = Expression.Parameter(typeof(TEntity));
        var combined = combiner.Invoke(
            new ReplaceParameterVisitor { { leftExpression.Parameters.Single(), parameter } }.Visit(leftExpression.Body),
            new ReplaceParameterVisitor { { rightExpression.Parameters.Single(), parameter } }.Visit(rightExpression.Body));
        return new ConstructedQuerySpecification(Expression.Lambda<Func<TEntity, Boolean>>(combined, parameter));
    }

    /// <inheritdoc/>
    public static implicit operator Expression<Func<TEntity, Boolean>>(EntitySpecification<TEntity> spec) => spec.ToExpression();

    /// <inheritdoc/>
    public static EntitySpecification<TEntity> operator &(EntitySpecification<TEntity> left, EntitySpecification<TEntity> right)
        => CombineSpecification(left, right, Expression.AndAlso);

    /// <inheritdoc/>
    public static EntitySpecification<TEntity> operator |(EntitySpecification<TEntity> left, EntitySpecification<TEntity> right)
        => CombineSpecification(left, right, Expression.OrElse);

    /// <inheritdoc/>
    public static EntitySpecification<TEntity> operator !(EntitySpecification<TEntity> spec)
    {
        var predicate = spec.ToExpression();
        var newExpression = Expression.Lambda<Func<TEntity, Boolean>>(Expression.Not(predicate.Body), predicate.Parameters[0]);
        return new ConstructedQuerySpecification(newExpression);
    }

    /// <summary>
    /// Represents a constructed query specification that is created from a given expression.
    /// </summary>
    /// <param name="specificationExpression">The expression used to create the specification.</param>
    protected class ConstructedQuerySpecification(Expression<Func<TEntity, Boolean>> specificationExpression)
        : EntitySpecification<TEntity>(specificationExpression)
    {
    }
}
