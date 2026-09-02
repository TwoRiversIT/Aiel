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
