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

public abstract class AbstractSpecification<T> : ISpecification<T>
{
    public virtual Boolean IsSatisfiedBy(T entity)
        => ToExpression().Compile().Invoke(entity);

    public abstract Expression<Func<T, Boolean>> ToExpression();

    public static implicit operator Expression<Func<T, Boolean>>(AbstractSpecification<T> specification)
        => specification.ToExpression();

    protected static AbstractSpecification<T> CombineSpecification(AbstractSpecification<T> left, AbstractSpecification<T> right, Func<Boolean, Boolean, Boolean> combiner)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(combiner);

        return new ConstructedSpecification(entity => combiner(left.IsSatisfiedBy(entity), right.IsSatisfiedBy(entity)));
    }

    public override String ToString() => GetType().Name;

    public static AbstractSpecification<T> operator &(AbstractSpecification<T> left, AbstractSpecification<T> right)
        => CombineSpecification(left, right, (leftResult, rightResult) => leftResult && rightResult);

    public static AbstractSpecification<T> operator |(AbstractSpecification<T> left, AbstractSpecification<T> right)
        => CombineSpecification(left, right, (leftResult, rightResult) => leftResult || rightResult);

    public static AbstractSpecification<T> operator !(AbstractSpecification<T> spec)
        => new ConstructedSpecification(entity => !spec.IsSatisfiedBy(entity));

    protected class ConstructedSpecification(Expression<Func<T, Boolean>> expression) : AbstractSpecification<T>()
    {
        private readonly Expression<Func<T, Boolean>> _expression = expression;

        public override Expression<Func<T, Boolean>> ToExpression() => _expression;
    }
}
