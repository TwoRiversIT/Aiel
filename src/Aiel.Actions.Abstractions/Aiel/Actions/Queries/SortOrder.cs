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
/// Represents the order in which to sort query results.
/// </summary>
public sealed record SortOrder
{
    /// <summary>
    /// Represents a sort order with no fields specified.
    /// </summary>
    public static SortOrder None { get; } = new() { Fields = [] };

    /// <summary>
    /// Initializes a new instance of the <see cref="SortOrder"/> record with default values.
    /// </summary>
    public SortOrder() : this([]) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SortOrder"/> record with the specified sort fields.
    /// </summary>
    /// <param name="fields"></param>
    public SortOrder(IEnumerable<SortField> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        Fields = fields.ToArray();
    }

    /// <summary>
    /// Gets the list of sort fields that define the order in which to sort query results.
    /// </summary>
    public IReadOnlyList<SortField> Fields { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the <see cref="SortOrder"/> has any sort fields specified.
    /// </summary>
    public Boolean HasValues => Fields.Count > 0;

    /// <summary>
    /// Creates a new instance of the <see cref="SortOrder"/> record from the specified sort fields.
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    public static SortOrder From(params SortField[] fields)
        => fields is null ? None : new(fields);
}
