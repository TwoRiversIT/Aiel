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

namespace Aiel.Domain.Specifications;

/// <summary>
/// Represents a specification for querying multiple entities of type T.
/// </summary>
/// <typeparam name="T">The type of the entities to query.</typeparam>
public record QueryMultipleSpecification<T> : QueryMultiple<T>, IQueryMultipleSpecification<T>
    where T : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryMultipleSpecification{T}"/> class.
    /// </summary>
    protected QueryMultipleSpecification() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryMultipleSpecification{T}"/> class with the specified specification, sort order, and page request.
    /// </summary>
    /// <param name="specification">The specification to apply to the query.</param>
    /// <param name="sortRequest">The sort order to apply to the query.</param>
    /// <param name="pageRequest">The page request to apply to the query.</param>
    /// <exception cref="ArgumentNullException">Thrown when the specification is null.</exception>
    public QueryMultipleSpecification(ISpecification<T> specification, SortOrder? sortRequest = null, Page? pageRequest = null)
        : base(sortRequest ?? SortOrder.None, pageRequest ?? Page.Default)
    {
        Specification = specification ?? throw new ArgumentNullException(nameof(specification));
    }

    /// <summary>
    /// Gets the specification to apply to the query.
    /// </summary>
    public required ISpecification<T> Specification { get; init; }
}
