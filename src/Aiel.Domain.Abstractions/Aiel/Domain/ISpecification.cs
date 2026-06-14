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

namespace Aiel.Domain;

public interface ISpecification<T>
    where T : notnull
{
    Boolean IsSatisfiedBy(T entity);
}

public abstract class Specification<T>(Func<T, Boolean> predicate) : ISpecification<T>
    where T : notnull
{
    private readonly Func<T, Boolean> _predicate = predicate;

    public virtual Boolean IsSatisfiedBy(T entity) => _predicate(entity);
}

internal class ConcreteSpecification<T>(Func<T, Boolean> predicate) : Specification<T>(predicate)
    where T : notnull
{
}

public static class SpecificationExtensions
{
    internal static Specification<TEntity> CombineSpecification<TEntity>(Specification<TEntity> left, Specification<TEntity> right, Func<Boolean, Boolean, Boolean> combiner)
        where TEntity : notnull
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(combiner);

        return new ConcreteSpecification<TEntity>(entity => combiner(left.IsSatisfiedBy(entity), right.IsSatisfiedBy(entity)));
    }

    public static Specification<T> And<T>(this Specification<T> left, Specification<T> right)
        where T : notnull
        => CombineSpecification(left, right, (leftResult, rightResult) => leftResult && rightResult);

    public static Specification<T> Or<T>(this Specification<T> left, Specification<T> right)
        where T : notnull
        => CombineSpecification(left, right, (leftResult, rightResult) => leftResult || rightResult);

    public static Specification<T> Not<T>(this Specification<T> _, Specification<T> spec)
        where T : notnull
        => new ConcreteSpecification<T>(entity => !spec.IsSatisfiedBy(entity));
}
