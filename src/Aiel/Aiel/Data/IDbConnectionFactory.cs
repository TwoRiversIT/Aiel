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

using System.Data;

namespace Aiel.Data;

/// <summary>
/// Provides methods to create database connection instances.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates and returns a new database connection instance.
    /// </summary>
    /// <remarks>The returned connection may require explicit opening, depending on the implementation. It is
    /// the caller's responsibility to close and dispose of the connection when it is no longer needed
    /// unless it's lifetime is being managed through dependency injection.</remarks>
    /// <returns>An <see cref="IDbConnection"/> representing an open or ready-to-use database connection. The caller is
    /// responsible for managing the connection's lifetime.</returns>
    IDbConnection CreateConnection();

    /// <summary>
    /// Asynchronously creates and opens a new database connection.
    /// </summary>
    /// <remarks>The returned connection is opened and ready for use. Callers are responsible for closing and
    /// disposing the connection when it is no longer needed unless it's lifetime is being managed through
    /// dependency injection.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains an open <see cref="IDbConnection"/>
    /// instance that must be disposed by the caller.</returns>
    Task<IDbConnection> CreateConnectionAsync();
}
