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

using Microsoft.EntityFrameworkCore;
using Aiel.Results;

namespace Aiel.Permissions.EntityFrameworkCore;

/// <summary>
/// EF Core implementation of <see cref="IPermissionStore"/>.
/// </summary>
/// <remarks>
/// Requires the permission catalog to be seeded via <see cref="PermissionMigrationRunner"/> before
/// grants can be created. Grant creation looks up the catalog entry by permission name to resolve
/// the stable ID.
/// </remarks>
public sealed class EfCorePermissionStore(PermissionsDbContext dbContext, TimeProvider timeProvider) : IPermissionStore
{
    /// <inheritdoc />
    public async Task<Result<PermissionGrantId>> CreateGrantAsync(
        PermissionName permissionName,
        PermissionScopeTypeName scopeType,
        PermissionScopeKey scopeKey,
        PermissionSubjectTypeName subjectType,
        PermissionSubjectKey subjectKey,
        PermissionGrantDecision decision,
        CancellationToken cancellationToken = default)
    {
        var catalogEntry = await dbContext.Catalog
            .FirstOrDefaultAsync(r => r.PermissionName == permissionName.Value, cancellationToken);

        if (catalogEntry is null)
        {
            return Result<PermissionGrantId>.Failure(
                new PermissionCatalogEntryNotFoundError(
                    PermissionsEfCoreErrorMessages.CatalogEntryNotFoundForPermissionName(permissionName.Value)));
        }

        var grantId = Guid.CreateVersion7();

        var record = new PermissionGrantRecord
        {
            Id = grantId,
            StableId = catalogEntry.StableId,
            PermissionName = permissionName.Value,
            ScopeType = scopeType.Value,
            ScopeKey = scopeKey.Value,
            SubjectType = subjectType.Value,
            SubjectKey = subjectKey.Value,
            Decision = (Int32)decision,
            GrantedAt = timeProvider.GetUtcNow(),
        };

        dbContext.Grants.Add(record);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<PermissionGrantId>.Success(PermissionGrantId.From(grantId));
    }

    /// <inheritdoc />
    public async Task<Result> RevokeGrantAsync(
        PermissionGrantId grantId,
        CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Grants
            .FirstOrDefaultAsync(r => r.Id == grantId.Value, cancellationToken);

        if (record is null)
        {
            return Result.Failure(
                new PermissionGrantNotFoundError(
                    PermissionsEfCoreErrorMessages.GrantNotFound(grantId.Value)));
        }

        dbContext.Grants.Remove(record);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<PermissionGrantSummary>>> GetGrantsForSubjectAsync(
        PermissionSubjectTypeName subjectType,
        PermissionSubjectKey subjectKey,
        CancellationToken cancellationToken = default)
    {
        var records = await dbContext.Grants
            .Where(r => r.SubjectType == subjectType.Value && r.SubjectKey == subjectKey.Value)
            .ToListAsync(cancellationToken);

        var summaries = records
            .Select(ToSummary)
            .ToList();

        return Result<IReadOnlyList<PermissionGrantSummary>>.Success(summaries);
    }

    private static PermissionGrantSummary ToSummary(PermissionGrantRecord record)
        => new()
        {
            GrantId = PermissionGrantId.From(record.Id),
            PermissionName = PermissionName.From(record.PermissionName),
            ScopeType = PermissionScopeTypeName.From(record.ScopeType),
            ScopeKey = PermissionScopeKey.From(record.ScopeKey),
            SubjectType = PermissionSubjectTypeName.From(record.SubjectType),
            SubjectKey = PermissionSubjectKey.From(record.SubjectKey),
            Decision = (PermissionGrantDecision)record.Decision,
        };
}
