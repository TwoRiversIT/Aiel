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

using System.Diagnostics;

namespace Aiel.IdGeneration;

/// <summary>
/// Generates sequential GUIDs optimized for PostgreSQL, MySQL, Oracle, and other databases
/// using standard RFC 4122 lexicographic GUID ordering.
/// </summary>
/// <remarks>
/// <para>
/// PostgreSQL and most other database systems use standard lexicographic (left-to-right) byte
/// comparison for GUIDs. This implementation places the timestamp in the first bytes to ensure
/// sequential ordering and optimal index locality.
/// </para>
/// <para>
/// For Microsoft SQL Server, which uses a non-standard GUID comparison algorithm,
/// use <see cref="SqlServerCombGuid"/> instead.
/// </para>
/// </remarks>
[DebuggerStepThrough]
public class PostgreSqlCombGuid : IGuidGenerator
{
    /// <summary>
    /// Generates a new sequential GUID optimized for PostgreSQL and databases using
    /// standard lexicographic GUID ordering.
    /// </summary>
    /// <returns>A new sequential GUID.</returns>
    /// <remarks>
    /// Sequential GUIDs are particularly useful in databases as they result in better performance
    /// than random GUIDs when used as primary keys due to improved index locality.
    /// The timestamp is placed in the first 6 bytes to align with standard RFC 4122
    /// lexicographic comparison.
    /// </remarks>
    public Guid NewGuid()
    {
        var guidArray = Guid.NewGuid().ToByteArray();

        var baseDate = new DateTime(1900, 1, 1);
        var now = DateTime.UtcNow;

        var days = new TimeSpan(now.Ticks - baseDate.Ticks);
        var msecs = now.TimeOfDay;

        var daysArray = BitConverter.GetBytes(days.Days);
        var msecsArray = BitConverter.GetBytes((Int32)msecs.TotalMilliseconds);

        Array.Reverse(daysArray);
        Array.Reverse(msecsArray);

        Array.Copy(daysArray, 0, guidArray, 0, 4);
        Array.Copy(msecsArray, 2, guidArray, 4, 2);

        return new Guid(guidArray);
    }
}
