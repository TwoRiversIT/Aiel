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

namespace Aiel.Specifications;

public class QuerySpecification<T> : Specification<T>, IQuerySpecification<T>
{
    private Func<T, Boolean>? _isSatisfiedBy;

    protected QuerySpecification() { }

    public QuerySpecification(Expression<Func<T, Boolean>> predicate)
        => Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

    public virtual Expression<Func<T, Boolean>> Predicate { get; protected set; } = (_) => false;

    protected static QuerySpecification<T> CombineSpecification(QuerySpecification<T> left, QuerySpecification<T> right, Func<Expression, Expression, BinaryExpression> combiner)
    {
        var leftExpression = left.Predicate;
        var rightExpression = right.Predicate;
        var parameter = Expression.Parameter(typeof(T));
        var combined = combiner.Invoke(
            new ReplaceParameterVisitor { { leftExpression.Parameters.Single(), parameter } }.Visit(leftExpression.Body),
            new ReplaceParameterVisitor { { rightExpression.Parameters.Single(), parameter } }.Visit(rightExpression.Body));
        return new ConstructedQuerySpecification(Expression.Lambda<Func<T, Boolean>>(combined, parameter));
    }

    public override Boolean IsSatisfiedBy(T entity)
    {
        _isSatisfiedBy ??= Predicate.Compile();
        return _isSatisfiedBy(entity);
    }

    public static implicit operator Expression<Func<T, Boolean>>(QuerySpecification<T> spec) => spec.Predicate;

    public static QuerySpecification<T> operator &(QuerySpecification<T> left, QuerySpecification<T> right)
        => CombineSpecification(left, right, Expression.AndAlso);

    public static QuerySpecification<T> operator |(QuerySpecification<T> left, QuerySpecification<T> right)
        => CombineSpecification(left, right, Expression.OrElse);

    public static QuerySpecification<T> operator !(QuerySpecification<T> spec)
    {
        var predicate = spec.Predicate;
        var newExpression = Expression.Lambda<Func<T, Boolean>>(Expression.Not(predicate.Body), predicate.Parameters[0]);
        return new ConstructedQuerySpecification(newExpression);
    }

    protected class ConstructedQuerySpecification(Expression<Func<T, Boolean>> specificationExpression) : QuerySpecification<T>
    {
        public override Expression<Func<T, Boolean>> Predicate { get; protected set; } = specificationExpression;
    }
}
