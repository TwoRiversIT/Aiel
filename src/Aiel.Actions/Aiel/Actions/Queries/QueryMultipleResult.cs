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
    protected QueryMultipleResult(Error error) : base(false, error) { }

    protected QueryMultipleResult(IQueryMultiple query, Int32 count, Int32 totalRecords)
        : this(query.Page.Number, query.Page.Size, count, totalRecords)
    {
    }

    protected QueryMultipleResult(Int32 pageNo, Int32 pageSize, Int32 count, Int32 totalRecords)
        : base(true, null!)
    {
        TotalRecords = totalRecords;
        PageNumber = pageNo;
        PageSize = pageSize;
        Count = count;
    }

    private Int32 _pageSize = PageInfo.DefaultPageSize;
    private Int32 _pageNumber = PageInfo.DefaultPageNumber;

    public Int32 Count { get; }

    public Int32 TotalRecords { get; }

    public Int32 TotalPages => TotalRecords % PageSize == 0
        ? TotalRecords / PageSize
        : (TotalRecords / PageSize) + 1;

    /// <summary>
    /// Gets or sets the current page number. If the value is less than 1, it defaults to <see cref="PageInfo.DefaultPageNumber"/>.
    /// </summary>
    public Int32 PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? PageInfo.DefaultPageNumber : value;
    }

    public Int32 PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? PageInfo.DefaultPageSize : value;
    }

    public static QueryMultipleResult<TDto> Create<TDto>(IReadOnlyList<TDto> records, IQueryMultiple query, Int32 totalRecords = 0)
        where TDto : notnull
    {
        return Create(records, query.Page.Number, query.Page.Size, totalRecords);
    }

    public static QueryMultipleResult<TDto> Create<TDto>(IReadOnlyList<TDto> records, Int32 pageNumber = 1, Int32 pageSize = 10, Int32 totalRecords = 0)
        where TDto : notnull
    {
        return new QueryMultipleResult<TDto>(records, pageNumber, pageSize, totalRecords);
    }
}

public sealed class QueryMultipleResult<TDto> : QueryMultipleResult
    where TDto : notnull
{
    [JsonConstructor]
    public QueryMultipleResult(IReadOnlyList<TDto> records, Int32 pageNumber, Int32 pageSize, Int32 totalRecords)
        : base(pageNumber, pageSize, records.Count, totalRecords)
    {
        Records = records ?? [];
    }

    public QueryMultipleResult(IReadOnlyList<TDto> records, IQueryMultiple query, Int32 totalRecords)
        : this(records, query.Page.Number, query.Page.Size, totalRecords)
    {
        Records = records ?? [];
    }

    private QueryMultipleResult(Error error) : base(error) { }

    public IReadOnlyList<TDto> Records { get; init; } = [];

    public static implicit operator QueryMultipleResult<TDto>(Error error) => new(error);
}
