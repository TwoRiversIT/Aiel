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
/// EF Core implementation of <see cref="IAuthorizationGrantStore"/>.
/// </summary>
/// <remarks>
/// Requires the permission catalog to be seeded via <see cref="PermissionMigrationRunner"/> before
/// grants can be created. Grant creation looks up the catalog entry by permission name to resolve
/// the stable ID.
/// </remarks>
public sealed class EfCorePermissionStore(AuthorizationDbContext dbContext, TimeProvider timeProvider) : IAuthorizationGrantStore
{
    /// <inheritdoc />
    public async Task<Result<AuthorizationGrantId>> CreateGrantAsync(
        PermissionName permissionName,
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        AuthorizationGrantDecision decision,
        CancellationToken cancellationToken = default)
    {
        var catalogEntry = await dbContext.Catalog
            .FirstOrDefaultAsync(r => r.PermissionName == permissionName.Value, cancellationToken);

        if (catalogEntry is null)
        {
            return Result<AuthorizationGrantId>.Failure(
                new PermissionCatalogEntryNotFoundError(
                    AuthorizationEfCoreErrorMessages.CatalogEntryNotFoundForPermissionName(permissionName.Value)));
        }

        var grantId = Guid.CreateVersion7();

        var record = new AuthorizationGrantRecord
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

        return Result<AuthorizationGrantId>.Success(AuthorizationGrantId.From(grantId));
    }

    /// <inheritdoc />
    public async Task<Result> RevokeGrantAsync(
        AuthorizationGrantId grantId,
        CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Grants
            .FirstOrDefaultAsync(r => r.Id == grantId.Value, cancellationToken);

        if (record is null)
        {
            return Result.Failure(
                new AuthorizationGrantNotFoundError(
                    AuthorizationEfCoreErrorMessages.GrantNotFound(grantId.Value)));
        }

        dbContext.Grants.Remove(record);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<AuthorizationGrantSummary>>> GetGrantsForSubjectAsync(
        AuthorizationSubjectTypeName subjectType,
        AuthorizationSubjectKey subjectKey,
        CancellationToken cancellationToken = default)
    {
        var records = await dbContext.Grants
            .Where(r => r.SubjectType == subjectType.Value && r.SubjectKey == subjectKey.Value)
            .ToListAsync(cancellationToken);

        var summaries = records
            .ConvertAll(ToSummary)
;

        return Result<IReadOnlyList<AuthorizationGrantSummary>>.Success(summaries);
    }

    private static AuthorizationGrantSummary ToSummary(AuthorizationGrantRecord record)
        => new()
        {
            GrantId = AuthorizationGrantId.From(record.Id),
            PermissionName = PermissionName.From(record.PermissionName),
            ScopeType = AuthorizationScopeTypeName.From(record.ScopeType),
            ScopeKey = AuthorizationScopeKey.From(record.ScopeKey),
            SubjectType = AuthorizationSubjectTypeName.From(record.SubjectType),
            SubjectKey = AuthorizationSubjectKey.From(record.SubjectKey),
            Decision = (AuthorizationGrantDecision)record.Decision,
        };
}
