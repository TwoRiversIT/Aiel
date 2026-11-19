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
/// Base class for query results that return multiple values with paging information.
/// </summary>
public abstract class QueryResult : Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryResult"/> class with an error.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    protected QueryResult(Error error) : base(false, error) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryResult"/> class with the specified query, count, and total values.
    /// </summary>
    /// <param name="query">The query that produced the results.</param>
    /// <param name="count">The number of values in the current page.</param>
    /// <param name="matching">The number of results that matched the query. Optional.</param>
    /// <param name="total">The total number of values in the dataset.</param>
    protected QueryResult(Query query, Int32 count, Int32 matching, Int32 total = 0)
        : this(query.Page.Number, query.Page.Size, count, matching, total)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryResult"/> class with the specified page number, page size, count, and total values.
    /// </summary>
    /// <param name="page">The current page number.</param>
    /// <param name="size">The number of values per page.</param>
    /// <param name="count">The number of values in the current page.</param>
    /// <param name="matching">The number of results that matched the query. Optional.</param>
    /// <param name="total">The total number of values in the dataset.</param>
    protected QueryResult(Int32 page, Int32 size, Int32 count, Int32 matching, Int32 total = 0)
        : base(true, null)
    {
        Page = page;
        Size = size;
        Count = count;
        Matching = matching;
        Total = total;
    }

    private Int32 _size = Queries.PageInfo.DefaultPageSize;
    private Int32 _page = Queries.PageInfo.DefaultPageNumber;

    /// <summary>
    /// Gets the number of values in the current page.
    /// </summary>
    public Int32 Count { get; }

    /// <summary>
    /// Gets the number of results that matched the query.
    /// </summary>
    public Int32 Matching { get; }

    /// <summary>
    /// Gets the total number of values in the dataset. Optional.
    /// </summary>
    public Int32 Total { get; }

    /// <summary>
    /// Gets the total number of pages available based on the total values and page size.
    /// </summary>
    public Int32 Pages => Matching % Size == 0
        ? Matching / Size
        : (Matching / Size) + 1;

    /// <summary>
    /// Gets or sets the current page number. If the value is less than 1, it defaults to <see cref="PageInfo.DefaultPageNumber"/>.
    /// </summary>
    public Int32 Page
    {
        get => _page;
        set => _page = value < 1 ? Queries.PageInfo.DefaultPageNumber : value;
    }

    /// <summary>
    /// Gets or sets the number of values per page. If the value is less than or equal to 0, it defaults to <see cref="PageInfo.DefaultPageSize"/>.
    /// </summary>
    public Int32 Size
    {
        get => _size;
        set => _size = value <= 0 ? Queries.PageInfo.DefaultPageSize : value;
    }

    /// <summary>
    /// Creates a new <see cref="QueryResult{TDto}"/> instance with the specified values, query, and total values.
    /// </summary>
    /// <param name="records">The values in the current page.</param>
    /// <param name="query">The query that produced the results.</param>
    /// <param name="matching">The number of values that matched the query.</param>
    /// <param name="total">The total number of values in the dataset. Optional.</param>
    /// <typeparam name="TDto">The type of the values.</typeparam>
    /// <returns>A new <see cref="QueryResult{TDto}"/> instance.</returns>
    public static QueryResult<TDto> Success<TDto>(IReadOnlyList<TDto> records, IQuery query, Int32 matching, Int32 total = 0)
        where TDto : notnull
    {
        return Success(records, query.Page.Number, query.Page.Size, matching, total);
    }

    /// <summary>
    /// Creates a successful <see cref="QueryResult{TDto}"/> instance with the specified values, page number, page size, and total values.
    /// </summary>
    /// <param name="records">The values in the current page.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="size">The number of values per page.</param>
    /// <param name="matching">The number of values that matched the query.</param>
    /// <param name="total">The total number of values in the dataset. Optional.</param>
    /// <returns>A successful <see cref="QueryResult{TDto}"/> instance.</returns>
    public static QueryResult<TDto> Success<TDto>(IReadOnlyList<TDto> records, Int32 page = 1, Int32 size = 10, Int32 matching = 0, Int32 total = 0)
        where TDto : notnull
        => new(records, page, size, matching, total);

    /// <summary>
    /// Creates a failed <see cref="QueryResult{TDto}"/> instance with the specified error.
    /// </summary>
    /// <param name="error">The error to associate with the failed result.</param>
    /// <returns>A failed <see cref="QueryResult{TDto}"/> instance.</returns>
    public static new QueryResult<TDto> Failure<TDto>(Error error)
        where TDto : notnull
        => error;
}

/// <summary>
/// Represents the result of a query that returns multiple items of type <typeparamref name="TDto"/> with paging information.
/// </summary>
/// <typeparam name="TDto">The type of the values.</typeparam>
public sealed class QueryResult<TDto> : QueryResult
    where TDto : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryResult{TDto}"/> class with the specified values, page number, page size, and total values.
    /// </summary>
    /// <param name="values">The values in the current page.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="size">The number of values per page.</param>
    /// <param name="matching">The number of results that matched the query.</param>
    /// <param name="total">The total number of values in the dataset. Optional.</param>
    [JsonConstructor]
    public QueryResult(IReadOnlyList<TDto> values, Int32 page, Int32 size, Int32 matching, Int32 total = 0)
        : base(page, size, values.Count, matching, total)
    {
        Values = values ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryResult{TDto}"/> class with the specified values, query, and total values.
    /// </summary>
    /// <param name="values">The values in the current page.</param>
    /// <param name="query">The query that produced the results.</param>
    /// <param name="matching">The number of results that matched the query. </param>
    /// <param name="total">The total number of values in the dataset. Optional.</param>
    public QueryResult(IReadOnlyList<TDto> values, IQuery query, Int32 matching, Int32 total = 0)
        : this(values, query.Page.Number, query.Page.Size, matching, total)
    {
        Values = values ?? [];
    }

    private QueryResult(Error error) : base(error) { }

    /// <summary>
    /// Gets the values in the current page. If there are no values, this will be an empty list.
    /// </summary>
    public IReadOnlyList<TDto> Values { get; init; } = [];

    /// <summary>
    /// Attempts to retrieve the values from the result. If the result is successful, it outputs the values; otherwise, it outputs an empty list.
    /// </summary>
    /// <param name="records">When this method returns, contains the values if the result is successful; otherwise, an empty list.</param>
    /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
    public Boolean TryGetValue([NotNullWhen(true)] out IReadOnlyList<TDto> records)
    {
        records = IsSuccess ? Values : [];
        return IsSuccess;
    }

    /// <summary>
    /// Defines an implicit conversion from an <see cref="Error"/> to a <see cref="QueryResult{TDto}"/>. This allows for easy creation of error results from error instances.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator QueryResult<TDto>(Error error) => new(error);
}

/// <summary>
/// Provides extension methods for working with <see cref="QueryResult{TDto}"/> instances.
/// </summary>
public static class MultipleResultExtensions
{
    /// <summary>
    /// Attempts to retrieve the values from a <see cref="QueryResult{TDto}"/> instance. If the result is successful, it outputs the values and returns true; otherwise, it outputs an empty list and returns false.
    /// </summary>
    /// <typeparam name="TDto">The type of the values.</typeparam>
    /// <param name="result">The <see cref="QueryResult{TDto}"/> instance to retrieve values from.</param>
    /// <param name="values">When this method returns, contains the values if the result is successful; otherwise, an empty list.</param>
    /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
    public static Boolean TryGetValues<TDto>(this QueryResult result, out IReadOnlyList<TDto> values)
        where TDto : notnull
    {
        if (result is QueryResult<TDto> multiple && multiple.Values.Count > 0)
        {
            values = multiple.Values;
            return true;
        }

        values = [];
        return false;
    }
}
