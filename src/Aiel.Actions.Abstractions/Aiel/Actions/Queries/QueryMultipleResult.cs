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

using Aiel.Results;
using System.Text.Json.Serialization;

namespace Aiel.Actions.Queries;

/// <summary>
/// Base class for query results that return multiple items with paging information.
/// </summary>
public abstract class QueryMultipleResult : Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryMultipleResult"/> class with an error.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    protected QueryMultipleResult(Error error) : base(false, error) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryMultipleResult"/> class with the specified query, count, and total records.
    /// </summary>
    /// <param name="query">The query that produced the results.</param>
    /// <param name="count">The number of records in the current page.</param>
    /// <param name="totalRecords">The total number of records available.</param>
    protected QueryMultipleResult(IQueryMultiple query, Int32 count, Int32 totalRecords)
        : this(query.Page.Number, query.Page.Size, count, totalRecords)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryMultipleResult"/> class with the specified page number, page size, count, and total records.
    /// </summary>
    /// <param name="pageNo">The current page number.</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="count">The number of records in the current page.</param>
    /// <param name="totalRecords">The total number of records available.</param>
    protected QueryMultipleResult(Int32 pageNo, Int32 pageSize, Int32 count, Int32 totalRecords)
        : base(true, null!)
    {
        TotalRecords = totalRecords;
        PageNumber = pageNo;
        PageSize = pageSize;
        Count = count;
    }

    private Int32 _pageSize = Page.DefaultPageSize;
    private Int32 _pageNumber = Page.DefaultPageNumber;

    /// <summary>
    /// Gets the number of records in the current page.
    /// </summary>
    public Int32 Count { get; }

    /// <summary>
    /// Gets the total number of records available.
    /// </summary>
    public Int32 TotalRecords { get; }

    /// <summary>
    /// Gets the total number of pages available based on the total records and page size.
    /// </summary>
    public Int32 TotalPages => TotalRecords % PageSize == 0
        ? TotalRecords / PageSize
        : (TotalRecords / PageSize) + 1;

    /// <summary>
    /// Gets or sets the current page number. If the value is less than 1, it defaults to <see cref="Page.DefaultPageNumber"/>.
    /// </summary>
    public Int32 PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? Page.DefaultPageNumber : value;
    }

    /// <summary>
    /// Gets or sets the number of records per page. If the value is less than or equal to 0, it defaults to <see cref="Page.DefaultPageSize"/>.
    /// </summary>
    public Int32 PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? Page.DefaultPageSize : value;
    }

    /// <summary>
    /// Creates a new <see cref="QueryMultipleResult{TDto}"/> instance with the specified records, query, and total records.
    /// </summary>
    /// <param name="records">The records in the current page.</param>
    /// <param name="query">The query that produced the results.</param>
    /// <param name="totalRecords">The total number of records available.</param>
    /// <typeparam name="TDto">The type of the records.</typeparam>
    /// <returns>A new <see cref="QueryMultipleResult{TDto}"/> instance.</returns>
    public static QueryMultipleResult<TDto> Create<TDto>(IReadOnlyList<TDto> records, IQueryMultiple query, Int32 totalRecords = 0)
        where TDto : notnull
    {
        return Create(records, query.Page.Number, query.Page.Size, totalRecords);
    }

    /// <summary>
    /// Creates a new <see cref="QueryMultipleResult{TDto}"/> instance with the specified records, page number, page size, and total records.
    /// </summary>
    /// <typeparam name="TDto">The type of the records.</typeparam>
    /// <param name="records">The records in the current page.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="totalRecords">The total number of records available.</param>
    /// <returns>A new <see cref="QueryMultipleResult{TDto}"/> instance.</returns>
    public static QueryMultipleResult<TDto> Create<TDto>(IReadOnlyList<TDto> records, Int32 pageNumber = 1, Int32 pageSize = 10, Int32 totalRecords = 0)
        where TDto : notnull
    {
        return new QueryMultipleResult<TDto>(records, pageNumber, pageSize, totalRecords);
    }
}

/// <summary>
/// Represents the result of a query that returns multiple items of type <typeparamref name="TDto"/> with paging information.
/// </summary>
/// <typeparam name="TDto">The type of the records.</typeparam>
public sealed class QueryMultipleResult<TDto> : QueryMultipleResult
    where TDto : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryMultipleResult{TDto}"/> class with the specified records, page number, page size, and total records.
    /// </summary>
    /// <param name="records">The records in the current page.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="totalRecords">The total number of records available.</param>
    [JsonConstructor]
    public QueryMultipleResult(IReadOnlyList<TDto> records, Int32 pageNumber, Int32 pageSize, Int32 totalRecords)
        : base(pageNumber, pageSize, records.Count, totalRecords)
    {
        Records = records ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryMultipleResult{TDto}"/> class with the specified records, query, and total records.
    /// </summary>
    /// <param name="records"></param>
    /// <param name="query"></param>
    /// <param name="totalRecords"></param>
    public QueryMultipleResult(IReadOnlyList<TDto> records, IQueryMultiple query, Int32 totalRecords)
        : this(records, query.Page.Number, query.Page.Size, totalRecords)
    {
        Records = records ?? [];
    }

    private QueryMultipleResult(Error error) : base(error) { }

    /// <summary>
    /// Gets the records in the current page. If there are no records, this will be an empty list.
    /// </summary>
    public IReadOnlyList<TDto> Records { get; init; } = [];

    /// <summary>
    /// Defines an implicit conversion from an <see cref="Error"/> to a <see cref="QueryMultipleResult{TDto}"/>. This allows for easy creation of error results from error instances.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator QueryMultipleResult<TDto>(Error error) => new(error);
}
