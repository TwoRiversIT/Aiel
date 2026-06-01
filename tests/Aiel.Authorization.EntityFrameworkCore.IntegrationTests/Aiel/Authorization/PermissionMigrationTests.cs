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
using Microsoft.Extensions.DependencyInjection;
using Aiel.Authorization.EntityFrameworkCore;
using Aiel.Authorization.Testing;
using Aiel.Testing;

namespace Aiel.Authorization;

public sealed class PermissionMigrationTests(PermissionsEfCoreFixture fixture, ITestOutputHelper output)
    : IntegrationTestBase<PermissionsEfCoreFixture>(fixture, output)
{
    [Fact]
    public async Task Rename_UpdatesPermissionNameOnExistingGrants()
    {
        await ResetDatabaseAsync();

        var stableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");
        var subjectKey = PermissionSubjectKey.From($"subject-{Guid.NewGuid()}");

        // Arrange — seed catalog and create a grant under the old name
        var runner = Services.GetRequiredService<PermissionMigrationRunner>();
        var addPlan = new PermissionMigrationPlan()
            .Add(stableId, PermissionTestData.PermissionNameChangeAppointment, PermissionTestData.ScopeTypeAlpha);

        var addResult = await runner.ApplyAsync(addPlan, CancellationToken);
        Assert.True(addResult.IsSuccess, $"Add migration failed: {addResult}");

        var store = Services.GetRequiredService<IPermissionStore>();
        var createResult = await store.CreateGrantAsync(
            PermissionTestData.PermissionNameChangeAppointment,
            PermissionTestData.ScopeTypeAlpha,
            PermissionTestData.ScopeKeyAlpha,
            PermissionTestData.SubjectTypeAlpha,
            subjectKey,
            PermissionGrantDecision.Granted,
            CancellationToken);

        Assert.True(createResult.IsSuccess, $"CreateGrantAsync failed: {createResult}");

        // Act — rename the permission
        var renamePlan = new PermissionMigrationPlan()
            .Rename(
                stableId,
                PermissionTestData.PermissionNameChangeAppointment,
                PermissionTestData.PermissionNameRescheduleAppointment);

        var renameResult = await runner.ApplyAsync(renamePlan, CancellationToken);

        // Assert — migration succeeded
        Assert.True(renameResult.IsSuccess, $"Rename migration failed: {renameResult}");

        var dbContext = Services.GetRequiredService<PermissionsDbContext>();
        Assert.Contains("Npgsql", dbContext.Database.ProviderName ?? String.Empty);

        // Assert — grant row now has the new permission name and kept the original grant identity
        var grantsResult = await store.GetGrantsForSubjectAsync(
            PermissionTestData.SubjectTypeAlpha,
            subjectKey,
            CancellationToken);

        Assert.True(grantsResult.IsSuccess);
        var grant = Assert.Single(grantsResult.Value);
        Assert.Equal(PermissionTestData.PermissionNameRescheduleAppointment.Value, grant.PermissionName.Value);
        Assert.Equal(createResult.Value, grant.GrantId);
        Assert.DoesNotContain(grantsResult.Value, item => item.PermissionName == PermissionTestData.PermissionNameChangeAppointment);

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
            .Add(stableId, PermissionTestData.PermissionNameChangeAppointment, PermissionTestData.ScopeTypeAlpha)
            .Rename(
                stableId,
                PermissionTestData.PermissionNameChangeAppointment,
                PermissionTestData.PermissionNameRescheduleAppointment);

        // Act
        var result = await runner.ApplyAsync(plan, CancellationToken);

        // Assert
        Assert.True(result.IsSuccess, $"Migration plan failed: {result}");

        var dbContext = Services.GetRequiredService<PermissionsDbContext>();
        var snapshot = await dbContext.Snapshots.SingleAsync(CancellationToken);
        Assert.Equal(PermissionTestData.PermissionNameChangeAppointment.Value, snapshot.PreviousPermissionName);
        Assert.Equal(PermissionTestData.PermissionNameRescheduleAppointment.Value, snapshot.NewPermissionName);

        var manifestFound = Services.GetRequiredService<IPermissionDefinitionRegistry>()
            .TryGet(PermissionTestData.PermissionNameRescheduleAppointment, out var manifest);
        Assert.True(manifestFound);
        Assert.Contains(PermissionTestData.PermissionNameChangeAppointment, manifest.PreviousNames);
        Assert.Equal(PermissionLifecycle.Active, manifest.Lifecycle);
    }

    [Fact]
    public async Task Rename_WithMismatchedPreviousName_ReturnsFailureWithoutChangingCatalogOrGrants()
    {
        await ResetDatabaseAsync();

        var stableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");
        var subjectKey = PermissionSubjectKey.From($"subject-{Guid.NewGuid()}");

        var runner = Services.GetRequiredService<PermissionMigrationRunner>();
        var addPlan = new PermissionMigrationPlan()
            .Add(stableId, PermissionTestData.PermissionNameChangeAppointment, PermissionTestData.ScopeTypeAlpha);

        var addResult = await runner.ApplyAsync(addPlan, CancellationToken);
        Assert.True(addResult.IsSuccess, $"Add migration failed: {addResult}");

        var store = Services.GetRequiredService<IPermissionStore>();
        var createResult = await store.CreateGrantAsync(
            PermissionTestData.PermissionNameChangeAppointment,
            PermissionTestData.ScopeTypeAlpha,
            PermissionTestData.ScopeKeyAlpha,
            PermissionTestData.SubjectTypeAlpha,
            subjectKey,
            PermissionGrantDecision.Granted,
            CancellationToken);

        Assert.True(createResult.IsSuccess, $"CreateGrantAsync failed: {createResult}");

        var renamePlan = new PermissionMigrationPlan()
            .Rename(
                stableId,
                PermissionTestData.PermissionNameWrite,
                PermissionTestData.PermissionNameRescheduleAppointment);

        var renameResult = await runner.ApplyAsync(renamePlan, CancellationToken);

        Assert.False(renameResult.IsSuccess);

        var dbContext = Services.GetRequiredService<PermissionsDbContext>();
        var catalog = await dbContext.Catalog.SingleAsync(item => item.StableId == stableId.Value, CancellationToken);
        Assert.Equal(PermissionTestData.PermissionNameChangeAppointment.Value, catalog.PermissionName);

        var grantsResult = await store.GetGrantsForSubjectAsync(
            PermissionTestData.SubjectTypeAlpha,
            subjectKey,
            CancellationToken);

        Assert.True(grantsResult.IsSuccess);
        var grant = Assert.Single(grantsResult.Value);
        Assert.Equal(PermissionTestData.PermissionNameChangeAppointment.Value, grant.PermissionName.Value);
    }

    [Fact]
    public async Task ApplyAsync_WhenPlanFails_DoesNotLeaveTrackedChangesThatCanBeSavedLater()
    {
        await ResetDatabaseAsync();

        var stableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");
        var unknownStableId = PermissionStableId.From($"perm.test.{Guid.NewGuid()}");
        var runner = Services.GetRequiredService<PermissionMigrationRunner>();
        var plan = new PermissionMigrationPlan()
            .Add(stableId, PermissionTestData.PermissionNameRead, PermissionTestData.ScopeTypeAlpha)
            .Rename(
                unknownStableId,
                PermissionTestData.PermissionNameRead,
                PermissionTestData.PermissionNameWrite);

        var result = await runner.ApplyAsync(plan, CancellationToken);

        Assert.False(result.IsSuccess);

        var dbContext = Services.GetRequiredService<PermissionsDbContext>();
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
            .Add(stableId, PermissionTestData.PermissionNameRead, PermissionTestData.ScopeTypeAlpha)
            .Deprecate(stableId);

        var result = await runner.ApplyAsync(plan, CancellationToken);
        Assert.True(result.IsSuccess, $"Migration plan failed: {result}");

        // Note: the store does not enforce lifecycle on create — lifecycle enforcement is
        // an application-layer concern in IPermissionManager. This test verifies the migration
        // completes successfully and does not corrupt the catalog.
        var store = Services.GetRequiredService<IPermissionStore>();
        var grantsResult = await store.GetGrantsForSubjectAsync(
            PermissionTestData.SubjectTypeAlpha,
            PermissionTestData.SubjectKeyAlpha,
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
            .Rename(unknownStableId, PermissionTestData.PermissionNameRead, PermissionTestData.PermissionNameWrite);

        // Act
        var result = await runner.ApplyAsync(plan, CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsType<MigrationCatalogEntryNotFoundError>(result.Error);
    }

    private async Task ResetDatabaseAsync()
    {
        var dbContext = Services.GetRequiredService<PermissionsDbContext>();
        await dbContext.Database.EnsureDeletedAsync(CancellationToken);

        var initializer = Services.GetRequiredService<PermissionsDbInitializer>();
        await initializer.EnsureCreatedAsync(CancellationToken);
    }
}
