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

using System.Text.Json.Serialization;

namespace Aiel.Actions.Queries;

/// <summary>
/// Represents a query that retrieves multiple results with sorting and paging information.
/// </summary>
public record Query : IQuery
{
    /// <summary>
    /// Gets a query that retrieves all results without any sorting or paging.
    /// </summary>
    public static readonly Query All = new(SortOrder.None, PageInfo.All);

    /// <summary>
    /// Gets a query that retrieves results with the default paging and no sorting.
    /// </summary>
    public static readonly Query Default = new(SortOrder.None, PageInfo.Default);

    /// <summary>
    /// Gets a query that retrieves up to the specified number of results without any sorting.
    /// </summary>
    /// <param name="howMany">The number of results to retrieve.</param>
    /// <returns>A new <see cref="Query"/> instance.</returns>
    public static Query Some(Int32 howMany) => new(SortOrder.None, PageInfo.Some(howMany));

    /// <summary>
    /// Initializes a new instance of the <see cref="Query{TDto}"/> class with default sorting and paging.
    /// </summary>
    protected Query() : this(SortOrder.None, PageInfo.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Query"/> class with the specified sorting and paging.
    /// </summary>
    /// <param name="sort"></param>
    /// <param name="page"></param>
    /// <exception cref="ArgumentNullException"></exception>
    [JsonConstructor]
    protected Query(SortOrder sort, PageInfo page)
    {
        Sort = sort ?? throw new ArgumentNullException(nameof(sort));
        Page = page ?? throw new ArgumentNullException(nameof(page));
    }

    /// <summary>
    /// Gets the sorting order for the query results. Defaults to <see cref="SortOrder.None"/>.
    /// </summary>
    public SortOrder Sort { get; set; } = SortOrder.None;

    /// <summary>
    /// Gets the pagination information for the query results. Defaults to <see cref="PageInfo.Default"/>.
    /// </summary>
    public PageInfo Page { get; set; } = PageInfo.Default;
}

/// <summary>
/// Base class for queries that return multiple results with sorting and paging.
/// By default, the Sort property is set to <see cref="SortOrder.None"/> and the
/// PageInfo property is set to <see cref="PageInfo.Default"/>. Derived classes can
/// override these defaults as needed.
/// </summary>
public abstract record Query<TDto> : Query, IQuery<TDto>
    where TDto : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Query{TDto}"/> class with default sorting and paging.
    /// </summary>
    protected Query() : base(SortOrder.None, PageInfo.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Query{TDto}"/> class with the specified sorting and paging.
    /// </summary>
    /// <param name="sort">The sort order for the query results.</param>
    /// <param name="page">The pagination information for the query results.</param>
    protected Query(SortOrder sort, PageInfo page) : base(sort, page)
    {
    }
}
