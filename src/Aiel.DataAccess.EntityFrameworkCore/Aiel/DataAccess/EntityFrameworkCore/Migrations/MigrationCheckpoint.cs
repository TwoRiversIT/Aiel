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
/// Represents the set of tenant migration targets already successfully migrated, allowing an
/// out-of-band runner to resume from a known position after partial failure or restart.
/// </summary>
/// <remarks>
/// Callers build a <see cref="MigrationCheckpoint"/> from their persistent store (for example,
/// a catalog database table) and pass it to <see cref="ITenantMigrationRunner.ResumeAsync"/>.
/// The runner skips any target whose <see cref="TenantMigrationKey"/> appears in
/// <see cref="CompletedKeys"/>.
/// </remarks>
/// <remarks>
/// Initializes a new <see cref="MigrationCheckpoint"/> from the keys of already-migrated targets.
/// </remarks>
/// <param name="completedKeys">The keys of targets that have already been successfully migrated.</param>
public sealed class MigrationCheckpoint(IEnumerable<TenantMigrationKey> completedKeys)
{
    private readonly HashSet<TenantMigrationKey> _completedKeys = completedKeys.ToHashSet();

    /// <summary>
    /// Gets a checkpoint that marks no targets as completed, causing the runner to process
    /// every target from the beginning.
    /// </summary>
    public static MigrationCheckpoint Empty { get; } = new([]);

    /// <summary>Gets the set of target keys that have already been successfully migrated.</summary>
    public IReadOnlySet<TenantMigrationKey> CompletedKeys => _completedKeys;

    /// <summary>
    /// Returns <see langword="true"/> if the target identified by <paramref name="key"/> has
    /// already been successfully migrated and should be skipped by the runner.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    public Boolean IsCompleted(TenantMigrationKey key) => _completedKeys.Contains(key);
}
