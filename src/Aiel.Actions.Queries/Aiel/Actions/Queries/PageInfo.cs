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
public sealed record PageInfo
{
    public const Int32 DefaultPageNumber = 1;
    public const Int32 DefaultPageSize = 20;

    public static readonly PageInfo Default = new(DefaultPageNumber, DefaultPageSize);
    public static readonly PageInfo All = new(DefaultPageNumber, Int32.MaxValue);

    public PageInfo(Int32 pageNumber, Int32 pageSize = DefaultPageSize, Int32 totalRecords = 0)
    {
        Number = pageNumber;
        Size = pageSize;
        Total = totalRecords;
    }

    public Boolean IncludeTotalCount {  get; set; }

    /// <summary>
    /// Gets or sets the current page number. The page number is 1 based, and must be greater than or equal to 1.
    /// </summary>
    public Int32 Number
    {
        get;
        set
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Number), "Paging is 1 based. The pageNumber parameter must be greater than or equal to 1.");
            }

            field = value;
        }
    } = DefaultPageNumber;

    /// <summary>
    /// Gets or sets the number of records per page. The page size must be greater than or equal to 1.
    /// </summary>
    public Int32 Size
    {
        get;
        set
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Size), "The pageSize parameter must be greater than or equal to 1.");
            }

            field = value;
        }
    } = DefaultPageSize;

    /// <summary>
    /// Gets or sets the total number of records available for the query. This property is typically set by the query handler and is used to calculate the total number of pages.
    /// </summary>
    public Int32 Total { get; set; }

    /// <summary>
    /// Gets the offset for the query based on the current page number and page size. The offset is calculated as (Number - 1) * Size.
    /// </summary>
    public Int32 Offset => (Number - 1) * Size;

    /// <summary>
    /// Gets the total number of pages based on the total number of records and the page size. Returns -1 if the total number of records is zero.
    /// </summary>
    public Int32 Pages => Total > 0 ? (Int32)Math.Ceiling((Double)Total / Size) : -1;
}
