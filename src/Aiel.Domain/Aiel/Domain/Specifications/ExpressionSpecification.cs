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
/// Represents a specification that is defined using an expression.
/// </summary>
/// <typeparam name="T">The type of the entity to which the specification applies.</typeparam>
public class ExpressionSpecification<T> : AbstractSpecification<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionSpecification{T}"/> class with the specified expression.
    /// </summary>
    /// <param name="expression">The expression that defines the specification.</param>
    public ExpressionSpecification(Expression<Func<T, Boolean>> expression)
    {
        PredicateExpression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionSpecification{T}"/> class.
    /// </summary>
    protected ExpressionSpecification() { }

    /// <summary>
    /// Gets the expression that defines the specification.
    /// </summary>
    protected Expression<Func<T, Boolean>>? PredicateExpression { get; init; }

    /// <inheritdoc/>
    public override Expression<Func<T, Boolean>> ToExpression() => PredicateExpression ?? throw new InvalidOperationException("The expression has not been set.");

    /// <summary>
    /// Combines two specifications using the specified combiner function.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <param name="combiner">The function used to combine the results of the specifications.</param>
    /// <returns>A new specification that represents the combination of the two specifications using the specified combiner function.</returns>
    protected static ExpressionSpecification<T> CombineSpecification(ExpressionSpecification<T> left, ExpressionSpecification<T> right, Func<Boolean, Boolean, Boolean> combiner)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(combiner);

        return new ConstructedExpressionSpecification(entity => combiner(left.IsSatisfiedBy(entity), right.IsSatisfiedBy(entity)));
    }

    /// <inheritdoc/>
    public override String ToString() => this.GetType().Name;

    /// <inheritdoc/>
    public static ExpressionSpecification<T> operator &(ExpressionSpecification<T> left, ExpressionSpecification<T> right)
        => CombineSpecification(left, right, (leftResult, rightResult) => leftResult && rightResult);

    /// <inheritdoc/>
    public static ExpressionSpecification<T> operator |(ExpressionSpecification<T> left, ExpressionSpecification<T> right)
        => CombineSpecification(left, right, (leftResult, rightResult) => leftResult || rightResult);

    /// <inheritdoc/>
    public static ExpressionSpecification<T> operator !(ExpressionSpecification<T> spec)
        => new ConstructedExpressionSpecification(entity => !spec.IsSatisfiedBy(entity));

    /// <summary>
    /// Represents a constructed expression specification that is created from a given predicate.
    /// </summary>
    /// <param name="predicate">The predicate used to create the specification.</param>
    protected class ConstructedExpressionSpecification(Expression<Func<T, Boolean>> predicate) : ExpressionSpecification<T>(predicate);
}
