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

namespace Aiel.Actions.Queries;

public abstract class ListQuery<TDto> : IListQuery<TDto>
{
    protected ListQuery() {}

    protected ListQuery(SortRequest? sortRequest = null, PageRequest? pageRequest = null)
    {
        SortRequest = sortRequest ?? SortRequest.Empty;
        PageRequest = pageRequest ?? PageRequest.Default;
    }

    public SortRequest SortRequest { get; set; } = SortRequest.Empty;
    public PageRequest PageRequest { get; set; } = PageRequest.Default;
}

public abstract class ListQueryResult
{
    protected ListQueryResult() { }
    protected ListQueryResult(Int32 totalRecords, Int32 pageNo, Int32 pageSize)
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
        set => _pageNo = value < 0 ? 0 : value;
    }

    public Int32 PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? Int32.MaxValue : value;
    }
}

public class ListQueryResult<TDto> : ListQueryResult
    where TDto : class
{
    public ListQueryResult() { }
    public ListQueryResult(Int32 totalRecords, Int32 pageNo, Int32 pageSize) : base(totalRecords, pageNo, pageSize)
    {
    }

    public ICollection<TDto> Records { get; init; } = [];
}
