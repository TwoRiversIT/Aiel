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
/// The result of a single out-of-band tenant migration run.
/// </summary>
/// <remarks>
/// Inspect <see cref="HasFailures"/> to determine whether any targets require attention.
/// Targets that were already present in the <see cref="MigrationCheckpoint"/> passed to
/// <see cref="ITenantMigrationRunner.ResumeAsync"/> are neither succeeded nor failed — they
/// are simply skipped and do not appear in either collection.
/// </remarks>
/// <param name="Succeeded">The keys of targets successfully migrated in this run.</param>
/// <param name="Failed">Sanitised details of targets whose migration failed in this run.</param>
public sealed record TenantMigrationResult(
    IReadOnlyList<TenantMigrationKey> Succeeded,
    IReadOnlyList<MigrationFailedTarget> Failed)
{
    /// <summary>
    /// Gets <see langword="true"/> when at least one target failed to migrate in this run.
    /// </summary>
    public Boolean HasFailures => Failed.Count > 0;
}
