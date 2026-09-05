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
/// Represents an abstract specification that can be used to define business rules and criteria.
/// </summary>
/// <typeparam name="T">The type of the entity to which the specification applies.</typeparam>
public abstract class AbstractSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractSpecification{T}"/> class.
    /// </summary>
    protected AbstractSpecification() { }

    /// <inheritdoc/>
    public virtual Boolean IsSatisfiedBy(T entity)
        => ToExpression().Compile().Invoke(entity);

    /// <inheritdoc/>
    public abstract Expression<Func<T, Boolean>> ToExpression();

    /// <inheritdoc/>
    public static implicit operator Expression<Func<T, Boolean>>(AbstractSpecification<T> specification)
        => specification.ToExpression();

    /// <summary>
    /// Combines two specifications using a specified combiner function.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <param name="combiner">The function used to combine the results of the two specifications.</param>
    /// <returns>A new specification that represents the combination of the two specifications.</returns>
    protected static AbstractSpecification<T> CombineSpecification(AbstractSpecification<T> left, AbstractSpecification<T> right, Func<Boolean, Boolean, Boolean> combiner)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(combiner);

        return new ConstructedSpecification(entity => combiner(left.IsSatisfiedBy(entity), right.IsSatisfiedBy(entity)));
    }

    /// <inheritdoc/>
    public override String ToString() => GetType().Name;

    /// <inheritdoc/>
    public static AbstractSpecification<T> operator &(AbstractSpecification<T> left, AbstractSpecification<T> right)
        => CombineSpecification(left, right, (leftResult, rightResult) => leftResult && rightResult);

    /// <inheritdoc/>
    public static AbstractSpecification<T> operator |(AbstractSpecification<T> left, AbstractSpecification<T> right)
        => CombineSpecification(left, right, (leftResult, rightResult) => leftResult || rightResult);

    /// <inheritdoc/>
    public static AbstractSpecification<T> operator !(AbstractSpecification<T> spec)
        => new ConstructedSpecification(entity => !spec.IsSatisfiedBy(entity));

    /// <summary>
    /// Represents a constructed specification that is created from a given expression.
    /// </summary>
    /// <param name="expression">The expression used to create the specification.</param>
    protected class ConstructedSpecification(Expression<Func<T, Boolean>> expression) : AbstractSpecification<T>()
    {
        private readonly Expression<Func<T, Boolean>> _expression = expression;

        /// <inheritdoc/>
        public override Expression<Func<T, Boolean>> ToExpression() => _expression;
    }
}
