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

public interface IQueryMultiple
{
    SortOrder Sort { get; }
    PageInfo Page { get; }
}

public interface IQueryMultiple<TDto> : IQueryMultiple, IQuery<IReadOnlyList<TDto>>
    where TDto : notnull;

/// <summary>
/// Base class for queries that return multiple results with sorting and paging.
/// By default, the Sort property is set to <see cref="SortOrder.None"/> and the
/// Page property is set to <see cref="PageInfo.Default"/>. Derived classes can
/// override these defaults as needed.
/// </summary>
public abstract class QueryMultiple : IQueryMultiple
{
    protected QueryMultiple() { }
    protected QueryMultiple(SortOrder? sortRequest = null, PageInfo? pageRequest = null)
    {
        Sort = sortRequest ?? SortOrder.None;
        Page = pageRequest ?? PageInfo.Default;
    }

    public SortOrder Sort { get; set; } = SortOrder.None;
    public PageInfo Page { get; set; } = PageInfo.Default;
}

/// <summary>
/// Base class for queries that return multiple results with sorting and paging.
/// By default, the Sort property is set to <see cref="SortOrder.None"/> and the
/// Page property is set to <see cref="PageInfo.Default"/>. Derived classes can
/// override these defaults as needed.
/// </summary>
public abstract class QueryMultiple<TDto> : QueryMultiple, IQueryMultiple<TDto>
 where TDto : notnull
{
    protected QueryMultiple() { }

    protected QueryMultiple(SortOrder? sortRequest = null, PageInfo? pageRequest = null)
        : base(sortRequest, pageRequest)
    {
    }
}

/// <summary>
/// Base class for query results that return multiple items with paging information.
/// </summary>
public abstract class QueryMultipleResult : Result
{
    protected QueryMultipleResult(Error error) : base(false, error) { }

    protected QueryMultipleResult(IQueryMultiple query, Int32 totalRecords)
        : this(query.Page.Number, query.Page.Size, totalRecords)
    {
    }

    protected QueryMultipleResult(Int32 pageNo, Int32 pageSize, Int32 totalRecords)
        : base(true, null!)
    {
        TotalRecords = totalRecords;
        PageNo = pageNo;
        PageSize = pageSize;
    }

    private Int32 _pageSize = 10;
    private Int32 _pageNo;

    public Int32 TotalRecords { get; set; }

    public Int32 TotalPages => TotalRecords % PageSize == 0
        ? TotalRecords / PageSize
        : (TotalRecords / PageSize) + 1;

    public Int32 PageNo
    {
        get => _pageNo;
        set => _pageNo = value < 1 ? PageInfo.DefaultPageNumber : value;
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

    public static QueryMultipleResult<TDto> Create<TDto>(IReadOnlyList<TDto> records, Int32 pageNo = 1, Int32 pageSize = 10, Int32 totalRecords = 0)
        where TDto : notnull
    {
        return new QueryMultipleResult<TDto>(records, pageNo, pageSize, totalRecords);
    }
}

public sealed class QueryMultipleResult<TDto> : QueryMultipleResult
    where TDto : notnull
{
    [JsonConstructor]
    public QueryMultipleResult(IReadOnlyList<TDto> records, Int32 pageNo, Int32 pageSize, Int32 totalRecords) : base(pageNo, pageSize, totalRecords)
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
