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

using Aiel.Results;
using Microsoft.EntityFrameworkCore;

namespace Aiel.Authorization.EntityFrameworkCore;

/// <summary>
/// Applies a <see cref="PermissionMigrationPlan"/> to the permissions database.
/// </summary>
/// <remarks>
/// Each operation is applied in the order it was added to the plan. The entire plan is committed in
/// a single <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> call at the end, so the
/// database is either fully updated or left untouched on failure.
/// </remarks>
public sealed class PermissionMigrationRunner(
    AuthorizationDbContext dbContext,
    TimeProvider timeProvider)
{
    /// <summary>
    /// Applies all operations in <paramref name="plan"/> and commits them to the database.
    /// </summary>
    /// <param name="plan">The migration plan to apply.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A successful <see cref="Result"/> when all operations succeed, or the first failure encountered.</returns>
    public async Task<Result> ApplyAsync(PermissionMigrationPlan plan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);

        foreach (var operation in plan.TypedOperations)
        {
            var result = operation switch
            {
                AddPermissionOperation add => await ApplyAddAsync(add, cancellationToken),
                RenamePermissionOperation rename => await ApplyRenameAsync(rename, cancellationToken),
                DeprecatePermissionOperation deprecate => await ApplyDeprecateAsync(deprecate, cancellationToken),
                _ => Result.Failure(new UnknownMigrationOperationError(
                    AuthorizationEfCoreErrorMessages.UnknownMigrationOperation(operation.GetType().Name))),
            };

            if (!result.IsSuccess)
            {
                dbContext.ChangeTracker.Clear();
                return result;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    [SuppressMessage("Roslynator", "RCS1163:Unused parameter", Justification = "Async method signature required")]
    private Task<Result> ApplyAddAsync(AddPermissionOperation operation, CancellationToken cancellationToken = default)
    {
        var record = new PermissionCatalogRecord
        {
            StableId = operation.StableId.Value,
            PermissionName = operation.PermissionName.Value,
            ScopeType = operation.ScopeType.Value,
            Lifecycle = (Int32)PermissionLifecycle.Active,
        };

        dbContext.Catalog.Add(record);

        return Task.FromResult(Result.Success());
    }

    private async Task<Result> ApplyRenameAsync(RenamePermissionOperation operation, CancellationToken cancellationToken = default)
    {
        var catalog = dbContext.Catalog.Local.FirstOrDefault(r => r.StableId == operation.StableId.Value)
            ?? await dbContext.Catalog
                .FirstOrDefaultAsync(r => r.StableId == operation.StableId.Value, cancellationToken);

        if (catalog is null)
        {
            return Result.Failure(new MigrationCatalogEntryNotFoundError(
                AuthorizationEfCoreErrorMessages.MigrationCatalogEntryNotFound(operation.StableId.Value)));
        }

        if (catalog.PermissionName != operation.PreviousName.Value)
        {
            return Result.Failure(new MigrationCatalogNameMismatchError(
                AuthorizationEfCoreErrorMessages.MigrationCatalogNameMismatch(
                    operation.StableId.Value,
                    operation.PreviousName.Value,
                    catalog.PermissionName)));
        }

        catalog.PermissionName = operation.NewName.Value;

        var grantsToRename = await dbContext.Grants
            .Where(g => g.StableId == operation.StableId.Value && g.PermissionName == operation.PreviousName.Value)
            .ToListAsync(cancellationToken);

        foreach (var grant in grantsToRename)
        {
            grant.PermissionName = operation.NewName.Value;
        }

        var snapshot = new PermissionManifestSnapshotRecord
        {
            Id = Guid.CreateVersion7(),
            StableId = operation.StableId.Value,
            PreviousPermissionName = operation.PreviousName.Value,
            NewPermissionName = operation.NewName.Value,
            MigratedAt = timeProvider.GetUtcNow(),
        };

        dbContext.Snapshots.Add(snapshot);

        return Result.Success();
    }

    private async Task<Result> ApplyDeprecateAsync(DeprecatePermissionOperation operation, CancellationToken cancellationToken = default)
    {
        var catalog = dbContext.Catalog.Local.FirstOrDefault(r => r.StableId == operation.StableId.Value)
            ?? await dbContext.Catalog
                .FirstOrDefaultAsync(r => r.StableId == operation.StableId.Value, cancellationToken);

        if (catalog is null)
        {
            return Result.Failure(new MigrationCatalogEntryNotFoundError(
                AuthorizationEfCoreErrorMessages.MigrationCatalogEntryNotFound(operation.StableId.Value)));
        }

        catalog.Lifecycle = (Int32)PermissionLifecycle.Deprecated;

        return Result.Success();
    }
}
