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

using Aiel.IdGeneration;
using System.Diagnostics;

namespace Aiel.Testing;

/// <summary>
/// Provides a testable implementation of GUID generation that allows tests to override the GUID value.
/// </summary>
[DebuggerNonUserCode]
public static class TestableGuid
{
    private static Func<Guid> GetGuid = GetGuidInternal;

    /// <summary>
    /// Sets the GUID value that will be returned by all subsequent calls to <see cref="NewGuid"/>.
    /// </summary>
    /// <param name="guid">The GUID value to return.</param>
    [DebuggerNonUserCode]
    public static void GuidIs(Guid guid) => GetGuid = () => guid;

    /// <summary>
    /// Sets the GUID value that will be returned by all subsequent calls to <see cref="NewGuid"/>.
    /// </summary>
    /// <param name="guid">A string representation of the GUID value to return.</param>
    [DebuggerNonUserCode]
    public static void GuidIs(String guid)
    {
        var g = Guid.Parse(guid);
        GetGuid = () => g;
    }

    /// <summary>
    /// Generates a new GUID, using the overridden value if one has been set, otherwise generates a sequential GUID.
    /// </summary>
    /// <returns>A GUID value.</returns>
    [DebuggerNonUserCode]
    public static Guid NewGuid() => GetGuid?.Invoke() ?? GetGuidInternal();

    /// <summary>
    /// Resets the GUID generator to use the default sequential GUID generation.
    /// </summary>
    [DebuggerNonUserCode]
    public static void Reset() => GetGuid = GetGuidInternal;

    /// <summary>
    /// Generates a new sequential GUID using the <see cref="SqlServerCombGuid"/> implementation.
    /// </summary>
    /// <returns>A sequential GUID.</returns>
    /// <remarks>
    /// Uses SQL Server-optimized sequential GUIDs by default. For PostgreSQL-optimized GUIDs,
    /// use <c>TestableGuid.GetGuid = PostgreSqlCombGuid.NewGuid</c>.
    /// </remarks>
    private static Guid GetGuidInternal() => CombGuid.NewGuid(DatabaseType.None);
}
