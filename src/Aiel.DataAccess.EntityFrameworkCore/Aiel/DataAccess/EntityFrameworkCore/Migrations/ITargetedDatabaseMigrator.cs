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

namespace Aiel.DataAccess.EntityFrameworkCore.Migrations;

/// <summary>
/// Applies EF Core migrations to a single tenant's dedicated database.
/// </summary>
/// <remarks>
/// Aiel defines this contract. Consumers implement it to perform the actual EF Core
/// <c>MigrateAsync</c> call against a tenant's dedicated database connection, resolving the
/// connection from the <see cref="ITenantMigrationTarget"/> in an implementation-defined way.
/// </remarks>
public interface ITargetedDatabaseMigrator
{
    /// <summary>Applies any pending migrations to the database for <paramref name="target"/>.</summary>
    /// <param name="target">The tenant target whose database should be migrated.</param>
    /// <param name="cancellationToken">Token used to cancel the migration.</param>
    Task MigrateAsync(ITenantMigrationTarget target, CancellationToken cancellationToken = default);
}
