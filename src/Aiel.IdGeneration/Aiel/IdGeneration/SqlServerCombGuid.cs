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

// https://github.com/nhibernate/nhibernate-core/blob/master/src/NHibernate/Id/GuidCombGenerator.cs
// GNU LESSER GENERAL PUBLIC LICENSE

/// <summary>
/// Generates sequential GUIDs optimized for Microsoft SQL Server storage.
/// </summary>
/// <remarks>
/// <para>
/// SQL Server uses a non-standard GUID comparison algorithm that compares bytes in the order:
/// 10-15 (most significant), 8-9, 6-7, 4-5, 0-3. This implementation places the timestamp
/// in bytes 10-15 to ensure sequential ordering in SQL Server indexes.
/// </para>
/// <para>
/// For PostgreSQL, MySQL, Oracle, or other databases using standard RFC 4122 lexicographic
/// ordering, use <see cref="PostgreSqlCombGuid"/> instead.
/// </para>
/// </remarks>
[DebuggerStepThrough]
public class SqlServerCombGuid : IGuidGenerator
{
    /// <summary>
    /// Generates a new sequential GUID optimized for SQL Server storage.
    /// </summary>
    /// <returns>A new sequential GUID.</returns>
    /// <remarks>
    /// Sequential GUIDs are particularly useful in SQL Server as they result in better performance
    /// than random GUIDs when used as primary keys due to improved index locality.
    /// The timestamp is placed in the last 6 bytes (positions 10-15) to align with SQL Server's
    /// non-standard GUID comparison algorithm.
    /// </remarks>
    public Guid NewGuid()
    {
        var guidArray = Guid.NewGuid().ToByteArray();

        var baseDate = new DateTime(1900, 1, 1);
        var now = DateTime.UtcNow;

        var days = new TimeSpan(now.Ticks - baseDate.Ticks);
        var msecs = now.TimeOfDay;

        var daysArray = BitConverter.GetBytes(days.Days);
        var msecsArray = BitConverter.GetBytes((Int64)(msecs.TotalMilliseconds / 3.333333));

        Array.Reverse(daysArray);
        Array.Reverse(msecsArray);

        Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
        Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

        return new Guid(guidArray);
    }
}
