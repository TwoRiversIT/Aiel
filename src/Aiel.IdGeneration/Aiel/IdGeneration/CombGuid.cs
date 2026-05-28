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
/// Factory for generating sequential GUIDs (Comb GUIDs) optimized for different database systems.
/// </summary>
/// <remarks>
/// <para>
/// Sequential GUIDs combine a timestamp with a random GUID to create identifiers that are
/// both unique and sequential. This results in better database performance when used as
/// primary keys due to improved index locality.
/// </para>
/// <para>
/// Different database systems sort GUIDs differently:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// <b>SQL Server</b> uses a non-standard comparison algorithm (bytes 10-15 are most significant).
/// Use <see cref="DatabaseType.SqlServer"/> or call <see cref="SqlServerCombGuid.NewGuid"/> directly.
/// </description>
/// </item>
/// <item>
/// <description>
/// <b>PostgreSQL, MySQL, Oracle</b> use standard RFC 4122 lexicographic ordering (left-to-right).
/// Use <see cref="DatabaseType.PostgreSql"/> or call <see cref="PostgreSqlCombGuid.NewGuid"/> directly.
/// </description>
/// </item>
/// </list>
/// </remarks>
[DebuggerStepThrough]
public static class CombGuid
{
    /// <summary>
    /// Generates a new sequential GUID optimized for the specified database type.
    /// </summary>
    /// <param name="databaseType">The type of database system for which to optimize the GUID.</param>
    /// <returns>A new sequential GUID.</returns>
    /// <remarks>
    /// This factory method delegates to the appropriate database-specific implementation.
    /// For better performance in tight loops, consider calling the specific implementation directly.
    /// </remarks>
    public static Guid NewGuid(DatabaseType databaseType)
    {
        return databaseType switch
        {
            DatabaseType.None => Guid.NewGuid(),
            DatabaseType.SqlServer => new SqlServerCombGuid().NewGuid(),
            DatabaseType.PostgreSql => new PostgreSqlCombGuid().NewGuid(),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, "Unsupported database type.")
        };
    }
}
