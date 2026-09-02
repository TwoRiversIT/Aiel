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

public readonly struct SortField
{
    public const String InvalidName = "!INVALID!";

    public static readonly SortField Empty = new(InvalidName, SortDirection.None);

    public SortField()
    {
        Name = InvalidName;
        Direction = SortDirection.None;
    }

    public SortField([DisallowNull] String name, SortDirection direction = SortDirection.Ascending)
    {
        Name = name;
        Direction = direction;
    }

    public String Name
    {
        get;
        init => field = String.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Sort field name must not be null, empty, or whitespace.", nameof(Name))
            : value;
    }

    public SortDirection Direction
    {
        get;
        init => field = Enum.IsDefined(value)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(Direction), "Invalid sort direction.");
    } = SortDirection.Ascending;
}
