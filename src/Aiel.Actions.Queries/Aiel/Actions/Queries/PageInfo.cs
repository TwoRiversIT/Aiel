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

public sealed record PageInfo
{
    public const Int32 DefaultPageNumber = 1;
    public const Int32 DefaultPageSize = 20;

    public static readonly PageInfo Default = new(DefaultPageNumber, DefaultPageSize);
    public static readonly PageInfo All = new(DefaultPageNumber, Int32.MaxValue);

    public PageInfo(Int32 pageNumber, Int32 pageSize = DefaultPageSize)
    {
        Number = pageNumber < 1
            ? throw new ArgumentOutOfRangeException(nameof(pageNumber), "Paging is 1 based. The pageNumber parameter must be greater than or equal to 1.")
            : pageNumber;

        Size = pageSize < 1
            ? throw new ArgumentOutOfRangeException(nameof(pageSize), "The pageSize parameter must be greater than 0.")
            : pageSize;
    }

    public Int32 Number { get; } = DefaultPageNumber;

    public Int32 Size { get; } = DefaultPageSize;

    public Int32 Offset => (Number - 1) * Size;
}
