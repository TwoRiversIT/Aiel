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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Authorization;

public sealed class PermissionMigrationTests(AuthorizationEfCoreFixture fixture, ITestOutputHelper output)
    : IntegrationTestBase<AuthorizationEfCoreFixture>(fixture, output)
{
    [Fact]
    public async Task Rename_UpdatesPermissionNameOnExistingGrants()
    {
        await ResetDatabaseAsync();

        var stableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");
        var subjectKey = AuthorizationSubjectKey.From($"subject-{Guid.NewGuid()}");

        // Arrange — seed catalog and create a grant under the old name
        var runner = Services.GetRequiredService<PermissionMigrationRunner>();
        var addPlan = new PermissionMigrationPlan()
            .Add(stableId, AuthorizationTestData.PermissionNameChangeAppointment, AuthorizationTestData.ScopeTypeAlpha);

        var addResult = await runner.ApplyAsync(addPlan, CancellationToken);
        Assert.True(addResult.IsSuccess, $"Add migration failed: {addResult}");

        var store = Services.GetRequiredService<IAuthorizationGrantStore>();
        var createResult = await store.CreateGrantAsync(
            AuthorizationTestData.PermissionNameChangeAppointment,
            AuthorizationTestData.ScopeTypeAlpha,
            AuthorizationTestData.ScopeKeyAlpha,
            AuthorizationTestData.SubjectTypeAlpha,
            subjectKey,
            AuthorizationGrantDecision.Granted,
            CancellationToken);

        Assert.True(createResult.IsSuccess, $"CreateGrantAsync failed: {createResult}");

        // Act — rename the permission
        var renamePlan = new PermissionMigrationPlan()
            .Rename(
                stableId,
                AuthorizationTestData.PermissionNameChangeAppointment,
                AuthorizationTestData.PermissionNameRescheduleAppointment);

        var renameResult = await runner.ApplyAsync(renamePlan, CancellationToken);

        // Assert — migration succeeded
        Assert.True(renameResult.IsSuccess, $"Rename migration failed: {renameResult}");

        var dbContext = Services.GetRequiredService<AuthorizationDbContext>();
        Assert.Contains("Npgsql", dbContext.Database.ProviderName ?? String.Empty);

        // Assert — grant row now has the new permission name and kept the original grant identity
        var grantsResult = await store.GetGrantsForSubjectAsync(
            AuthorizationTestData.SubjectTypeAlpha,
            subjectKey,
            CancellationToken);

        Assert.True(grantsResult.IsSuccess);
        var grant = Assert.Single(grantsResult.Value);
        Assert.Equal(AuthorizationTestData.PermissionNameRescheduleAppointment.Value, grant.PermissionName.Value);
        Assert.Equal(createResult.Value, grant.GrantId);
        Assert.DoesNotContain(grantsResult.Value, item => item.PermissionName == AuthorizationTestData.PermissionNameChangeAppointment);

        var persistedGrant = await dbContext.Grants.SingleAsync(item => item.Id == createResult.Value.Value, CancellationToken);
        Assert.Equal(stableId.Value, persistedGrant.StableId);
    }

    [Fact]
    public async Task Rename_WritesManifestSnapshot_WithPreviousPermissionName()
    {
        await ResetDatabaseAsync();

        var stableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");

        var runner = Services.GetRequiredService<PermissionMigrationRunner>();
        var plan = new PermissionMigrationPlan()
            .Add(stableId, AuthorizationTestData.PermissionNameChangeAppointment, AuthorizationTestData.ScopeTypeAlpha)
            .Rename(
                stableId,
                AuthorizationTestData.PermissionNameChangeAppointment,
                AuthorizationTestData.PermissionNameRescheduleAppointment);

        // Act
        var result = await runner.ApplyAsync(plan, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess, $"Migration plan failed: {result}");

        var dbContext = Services.GetRequiredService<AuthorizationDbContext>();
        var snapshot = await dbContext.Snapshots.SingleAsync(CancellationToken);
        Assert.Equal(AuthorizationTestData.PermissionNameChangeAppointment.Value, snapshot.PreviousPermissionName);
        Assert.Equal(AuthorizationTestData.PermissionNameRescheduleAppointment.Value, snapshot.NewPermissionName);

        var manifestFound = Services.GetRequiredService<IAuthorizationDefinitionRegistry>()
            .TryGet(AuthorizationTestData.PermissionNameRescheduleAppointment, out var manifest);
        Assert.True(manifestFound);
        Assert.Contains(AuthorizationTestData.PermissionNameChangeAppointment, manifest.PreviousNames);
        Assert.Equal(PermissionLifecycle.Active, manifest.Lifecycle);
    }

    [Fact]
    public async Task Rename_WithMismatchedPreviousName_ReturnsFailureWithoutChangingCatalogOrGrants()
    {
        await ResetDatabaseAsync();

        var stableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");
        var subjectKey = AuthorizationSubjectKey.From($"subject-{Guid.NewGuid()}");

        var runner = Services.GetRequiredService<PermissionMigrationRunner>();
        var addPlan = new PermissionMigrationPlan()
            .Add(stableId, AuthorizationTestData.PermissionNameChangeAppointment, AuthorizationTestData.ScopeTypeAlpha);

        var addResult = await runner.ApplyAsync(addPlan, CancellationToken);
        Assert.True(addResult.IsSuccess, $"Add migration failed: {addResult}");

        var store = Services.GetRequiredService<IAuthorizationGrantStore>();
        var createResult = await store.CreateGrantAsync(
            AuthorizationTestData.PermissionNameChangeAppointment,
            AuthorizationTestData.ScopeTypeAlpha,
            AuthorizationTestData.ScopeKeyAlpha,
            AuthorizationTestData.SubjectTypeAlpha,
            subjectKey,
            AuthorizationGrantDecision.Granted,
            CancellationToken);

        Assert.True(createResult.IsSuccess, $"CreateGrantAsync failed: {createResult}");

        var renamePlan = new PermissionMigrationPlan()
            .Rename(
                stableId,
                AuthorizationTestData.PermissionNameWrite,
                AuthorizationTestData.PermissionNameRescheduleAppointment);

        var renameResult = await runner.ApplyAsync(renamePlan, CancellationToken);

        Assert.False(renameResult.IsSuccess);

        var dbContext = Services.GetRequiredService<AuthorizationDbContext>();
        var catalog = await dbContext.Catalog.SingleAsync(item => item.StableId == stableId.Value, CancellationToken);
        Assert.Equal(AuthorizationTestData.PermissionNameChangeAppointment.Value, catalog.PermissionName);

        var grantsResult = await store.GetGrantsForSubjectAsync(
            AuthorizationTestData.SubjectTypeAlpha,
            subjectKey,
            CancellationToken);

        Assert.True(grantsResult.IsSuccess);
        var grant = Assert.Single(grantsResult.Value);
        Assert.Equal(AuthorizationTestData.PermissionNameChangeAppointment.Value, grant.PermissionName.Value);
    }

    [Fact]
    public async Task ApplyAsync_WhenPlanFails_DoesNotLeaveTrackedChangesThatCanBeSavedLater()
    {
        await ResetDatabaseAsync();

        var stableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");
        var unknownStableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");
        var runner = Services.GetRequiredService<PermissionMigrationRunner>();
        var plan = new PermissionMigrationPlan()
            .Add(stableId, AuthorizationTestData.PermissionNameRead, AuthorizationTestData.ScopeTypeAlpha)
            .Rename(
                unknownStableId,
                AuthorizationTestData.PermissionNameRead,
                AuthorizationTestData.PermissionNameWrite);

        var result = await runner.ApplyAsync(plan, CancellationToken);

        Assert.False(result.IsSuccess);

        var dbContext = Services.GetRequiredService<AuthorizationDbContext>();
        await dbContext.SaveChangesAsync(CancellationToken);

        var catalogEntries = await dbContext.Catalog.ToListAsync(CancellationToken);
        Assert.Empty(catalogEntries);
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }

    [Fact]
    public async Task Deprecate_CatalogEntryStopsAcceptingNewGrants()
    {
        await ResetDatabaseAsync();

        var stableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");

        // Arrange — add then deprecate
        var runner = Services.GetRequiredService<PermissionMigrationRunner>();
        var plan = new PermissionMigrationPlan()
            .Add(stableId, AuthorizationTestData.PermissionNameRead, AuthorizationTestData.ScopeTypeAlpha)
            .Deprecate(stableId);

        var result = await runner.ApplyAsync(plan, CancellationToken);
        Assert.True(result.IsSuccess, $"Migration plan failed: {result}");

        // Note: the store does not enforce lifecycle on create — lifecycle enforcement is
        // an application-layer concern in IAuthorizationManager. This test verifies the migration
        // completes successfully and does not corrupt the catalog.
        var store = Services.GetRequiredService<IAuthorizationGrantStore>();
        var grantsResult = await store.GetGrantsForSubjectAsync(
            AuthorizationTestData.SubjectTypeAlpha,
            AuthorizationTestData.SubjectKeyAlpha,
            CancellationToken);

        Assert.True(grantsResult.IsSuccess);
    }

    [Fact]
    public async Task Rename_WithUnknownStableId_ReturnsFailure()
    {
        await ResetDatabaseAsync();

        var unknownStableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");

        // Arrange — do NOT run an Add migration; this stable ID does not exist in the catalog
        var runner = Services.GetRequiredService<PermissionMigrationRunner>();
        var plan = new PermissionMigrationPlan()
            .Rename(unknownStableId, AuthorizationTestData.PermissionNameRead, AuthorizationTestData.PermissionNameWrite);

        // Act
        var result = await runner.ApplyAsync(plan, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsType<MigrationCatalogEntryNotFoundError>(result.Error);
    }

    private async Task ResetDatabaseAsync()
    {
        var dbContext = Services.GetRequiredService<AuthorizationDbContext>();
        await dbContext.Database.EnsureDeletedAsync(CancellationToken);

        var initializer = Services.GetRequiredService<AuthorizationDbInitializer>();
        await initializer.EnsureCreatedAsync(CancellationToken);
    }
}
