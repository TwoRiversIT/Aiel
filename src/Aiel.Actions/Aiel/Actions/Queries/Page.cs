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
/// Carries paging information both directions for a query. The page number is 1 based, and the page size must be greater than or equal to 1.
/// </summary>
public record Page
{
    public const Int32 DefaultPageNumber = 1;
    public const Int32 DefaultPageSize = 20;

    public static readonly Page Default = new()
    {
        Number = DefaultPageNumber,
        Size = DefaultPageSize,
        Total = 0
    };

    public static readonly Page All = new()
    {
        Number = DefaultPageNumber,
        Size = Int32.MaxValue,
        Total = 0
    };

    public static Page Create(Int32 pageNumber, Int32 pageSize, Int32 totalRecords = 0)
    {
        return new Page
        {
            Number = pageNumber,
            Size = pageSize,
            Total = totalRecords
        };
    }

    public static Page SkipTake(Int32 skip, Int32 take, Int32 totalRecords = 0)
    {
        return new Page
        {
            Offset = skip,
            Size = take,
            Total = totalRecords
        };
    }

    public Boolean IncludeTotalCount { get; init; }

    /// <summary>
    /// Gets or sets the current page number. The page number is 1 based, and must be greater than or equal to 1.
    /// </summary>
    public Int32 Number
    {
        get => field < 1 ? 1 : field;
        init => field = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Gets or sets the number of records per page. The page size must be greater than or equal to 1.
    /// </summary>
    public Int32 Size
    {
        get => field < 1 ? 1 : field;
        init => field = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Gets or sets the total number of records available for the query. This property is typically set by the query handler and is used to calculate the total number of pages.
    /// </summary>
    public Int32 Total
    {
        get => field < 0 ? 0 : field;
        init => field = value < 0 ? 0 : value;
    }

    /// <summary>
    /// Gets or sets the offset for the query. If a value is not explicitly set, the offset is calculated as: (<see cref="Number"/> - 1) * <see cref="Size"/>.
    /// </summary>
    public Int32 Offset
    {
        get => (field <= 0) ? (Number - 1) * Size : field;
        init => field = value < 0 ? 0 : value;
    }

    /// <summary>
    /// Gets the total number of pages based on the total number of records and the page size.
    /// </summary>
    public Int32 Pages => Total > 0 ? (Int32)Math.Ceiling((Double)Total / Size) : 0;
}
