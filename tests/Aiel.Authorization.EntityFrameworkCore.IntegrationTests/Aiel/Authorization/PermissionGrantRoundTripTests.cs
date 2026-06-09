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

using Aiel.Authorization.EntityFrameworkCore;
using Aiel.Authorization.Testing;
using Aiel.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Authorization;

public sealed class PermissionGrantRoundTripTests(AuthorizationEfCoreFixture fixture, ITestOutputHelper output)
    : IntegrationTestBase<AuthorizationEfCoreFixture>(fixture, output)
{
    [Fact]
    public async Task CreateGrantAsync_WithValidPermissionName_ReturnsNewGrantId()
    {
        await ResetDatabaseAsync();

        var stableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");
        var runner = Services.GetRequiredService<PermissionMigrationRunner>();
        var plan = new PermissionMigrationPlan()
            .Add(stableId, AuthorizationTestData.PermissionNameRead, AuthorizationTestData.ScopeTypeAlpha);

        var migrationResult = await runner.ApplyAsync(plan, CancellationToken);
        Assert.True(migrationResult.IsSuccess, $"Migration failed: {migrationResult}");

        var store = Services.GetRequiredService<IAuthorizationGrantStore>();

        // Act
        var result = await store.CreateGrantAsync(
            AuthorizationTestData.PermissionNameRead,
            AuthorizationTestData.ScopeTypeAlpha,
            AuthorizationTestData.ScopeKeyAlpha,
            AuthorizationTestData.SubjectTypeAlpha,
            AuthorizationTestData.SubjectKeyAlpha,
            AuthorizationGrantDecision.Granted,
            CancellationToken);

        // Assert
        Assert.True(result.IsSuccess, $"Expected success but got: {result}");
        Assert.NotEqual(default, result.Value.Value);
    }

    [Fact]
    public async Task GetGrantsForSubjectAsync_AfterCreatingGrant_ReturnsThatGrant()
    {
        await ResetDatabaseAsync();

        var stableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");
        var subjectKey = AuthorizationSubjectKey.From($"subject-{Guid.NewGuid()}");

        var runner = Services.GetRequiredService<PermissionMigrationRunner>();
        var plan = new PermissionMigrationPlan()
            .Add(stableId, AuthorizationTestData.PermissionNameRead, AuthorizationTestData.ScopeTypeAlpha);

        var migrationResult = await runner.ApplyAsync(plan, CancellationToken);
        Assert.True(migrationResult.IsSuccess, $"Migration failed: {migrationResult}");

        var store = Services.GetRequiredService<IAuthorizationGrantStore>();
        var createResult = await store.CreateGrantAsync(
            AuthorizationTestData.PermissionNameRead,
            AuthorizationTestData.ScopeTypeAlpha,
            AuthorizationTestData.ScopeKeyAlpha,
            AuthorizationTestData.SubjectTypeAlpha,
            subjectKey,
            AuthorizationGrantDecision.Granted,
            CancellationToken);

        Assert.True(createResult.IsSuccess);

        // Act
        var result = await store.GetGrantsForSubjectAsync(
            AuthorizationTestData.SubjectTypeAlpha,
            subjectKey,
            CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        var grants = result.Value;
        Assert.Single(grants);
        Assert.Equal(AuthorizationTestData.PermissionNameRead.Value, grants[0].PermissionName.Value);
        Assert.Equal(AuthorizationGrantDecision.Granted, grants[0].Decision);
    }

    [Fact]
    public async Task RevokeGrantAsync_AfterCreatingGrant_RemovesGrant()
    {
        await ResetDatabaseAsync();

        var stableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");
        var subjectKey = AuthorizationSubjectKey.From($"subject-{Guid.NewGuid()}");

        var runner = Services.GetRequiredService<PermissionMigrationRunner>();
        var plan = new PermissionMigrationPlan()
            .Add(stableId, AuthorizationTestData.PermissionNameRead, AuthorizationTestData.ScopeTypeAlpha);

        var migrationResult = await runner.ApplyAsync(plan, CancellationToken);
        Assert.True(migrationResult.IsSuccess, $"Migration failed: {migrationResult}");

        var store = Services.GetRequiredService<IAuthorizationGrantStore>();
        var createResult = await store.CreateGrantAsync(
            AuthorizationTestData.PermissionNameRead,
            AuthorizationTestData.ScopeTypeAlpha,
            AuthorizationTestData.ScopeKeyAlpha,
            AuthorizationTestData.SubjectTypeAlpha,
            subjectKey,
            AuthorizationGrantDecision.Granted,
            CancellationToken);

        Assert.True(createResult.IsSuccess);

        // Act
        var revokeResult = await store.RevokeGrantAsync(createResult.Value, CancellationToken);

        // Assert
        Assert.True(revokeResult.IsSuccess, $"Expected success but got: {revokeResult}");

        var grantsResult = await store.GetGrantsForSubjectAsync(
            AuthorizationTestData.SubjectTypeAlpha,
            subjectKey,
            CancellationToken);

        Assert.True(grantsResult.IsSuccess);
        Assert.Empty(grantsResult.Value);
    }

    [Fact]
    public async Task CreateGrantAsync_WithUnknownPermissionName_ReturnsFailure()
    {
        await ResetDatabaseAsync();

        // Arrange — catalog is empty (no migration run)
        var store = Services.GetRequiredService<IAuthorizationGrantStore>();
        var unknownName = PermissionName.From("testing.unknown");

        // Act
        var result = await store.CreateGrantAsync(
            unknownName,
            AuthorizationTestData.ScopeTypeAlpha,
            AuthorizationTestData.ScopeKeyAlpha,
            AuthorizationTestData.SubjectTypeAlpha,
            AuthorizationTestData.SubjectKeyAlpha,
            AuthorizationGrantDecision.Granted,
            CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsType<PermissionCatalogEntryNotFoundError>(result.Error);
    }

    private async Task ResetDatabaseAsync()
    {
        var dbContext = Services.GetRequiredService<AuthorizationDbContext>();
        await dbContext.Database.EnsureDeletedAsync(CancellationToken);

        var initializer = Services.GetRequiredService<AuthorizationDbInitializer>();
        await initializer.EnsureCreatedAsync(CancellationToken);
    }
}
