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

namespace Aiel.IdGeneration;

/// <summary>
/// Specifies the type of database system for sequential GUID generation optimization.
/// </summary>
public enum DatabaseType
{
    /// <summary>
    /// Falls back to the default GUID generation method (random GUIDs) without any specific
    /// optimization for database storage.
    /// </summary>
    None,

    /// <summary>
    /// Microsoft SQL Server, which uses a non-standard GUID comparison algorithm.
    /// Sequential GUIDs for SQL Server place the timestamp in the last 6 bytes.
    /// </summary>
    SqlServer,

    /// <summary>
    /// PostgreSQL, MySQL, Oracle, and other databases using standard RFC 4122 lexicographic GUID ordering.
    /// Sequential GUIDs for these databases place the timestamp in the first bytes.
    /// </summary>
    PostgreSql
}
