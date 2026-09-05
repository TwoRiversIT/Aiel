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
/// Represents a field by which query results can be sorted, including the field name and sort direction.
/// </summary>
public readonly struct SortField
{
    /// <summary>
    /// Represents an invalid sort field name, used to indicate that a sort field is not valid or has not been set.
    /// </summary>
    public const String InvalidName = "!INVALID!";

    /// <summary>
    /// Represents an empty sort field, used to indicate that no sorting is applied.
    /// </summary>
    public static readonly SortField Empty = new(InvalidName, SortDirection.None);

    /// <summary>
    /// Initializes a new instance of the <see cref="SortField"/> struct with default values.
    /// </summary>
    public SortField()
    {
        Name = InvalidName;
        Direction = SortDirection.None;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SortField"/> struct with the specified name and direction.
    /// </summary>
    /// <param name="name">The name of the sort field.</param>
    /// <param name="direction">The direction in which to sort.</param>
    public SortField([DisallowNull] String name, SortDirection direction = SortDirection.Ascending)
    {
        Name = name;
        Direction = direction;
    }

    /// <summary>
    /// Gets the name of the sort field.
    /// </summary>
    public String Name
    {
        get;
        init => field = String.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Sort field name must not be null, empty, or whitespace.", nameof(Name))
            : value;
    }

    /// <summary>
    /// Gets the direction in which to sort.
    /// </summary>
    public SortDirection Direction
    {
        get;
        init => field = Enum.IsDefined(value)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(Direction), "Invalid sort direction.");
    } = SortDirection.Ascending;
}
