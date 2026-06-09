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

namespace Aiel.Specifications;

public class Specification<T> : ISpecification<T>
{
    private readonly Func<T, Boolean>? _predicate;

    protected Specification() { }

    public Specification(Func<T, Boolean> predicate)
        => _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

    public virtual Boolean IsSatisfiedBy(T entity)
    {
        return _predicate?.Invoke(entity) ?? false;
    }

    protected static Specification<T> CombineSpecification(Specification<T> left, Specification<T> right, Func<Boolean, Boolean, Boolean> combiner)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(combiner);

        return new ConstructedSpecification(entity => combiner(left.IsSatisfiedBy(entity), right.IsSatisfiedBy(entity)));
    }

    public override String ToString() => GetType().Name;

    public static Specification<T> operator &(Specification<T> left, Specification<T> right)
        => CombineSpecification(left, right, (leftResult, rightResult) => leftResult && rightResult);

    public static Specification<T> operator |(Specification<T> left, Specification<T> right)
        => CombineSpecification(left, right, (leftResult, rightResult) => leftResult || rightResult);

    public static Specification<T> operator !(Specification<T> spec)
        => new ConstructedSpecification(entity => !spec.IsSatisfiedBy(entity));

    protected class ConstructedSpecification(Func<T, Boolean> predicate) : Specification<T>(predicate);
}
