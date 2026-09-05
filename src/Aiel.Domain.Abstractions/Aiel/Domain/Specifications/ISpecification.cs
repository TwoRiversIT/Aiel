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
/// Defines a specification that can be used to determine if an object
/// satisfies certain criteria.
/// </summary>
/// <typeparam name="T">The type of the object to evaluate.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Determines whether the specified object satisfies the criteria defined by the specification.
    /// </summary>
    /// <param name="obj">The object to evaluate.</param>
    /// <returns><c>true</c> if the object satisfies the criteria; otherwise, <c>false</c>.</returns>
    Boolean IsSatisfiedBy(T obj) => ToExpression().Compile().Invoke(obj);

    /// <summary>
    /// Converts the specification to a LINQ expression.
    /// </summary>
    /// <returns>An expression that represents the specification.</returns>
    public abstract Expression<Func<T, Boolean>> ToExpression();
}
