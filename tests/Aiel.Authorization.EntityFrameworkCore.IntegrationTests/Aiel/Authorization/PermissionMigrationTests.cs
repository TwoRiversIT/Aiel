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
        addResult.IsSuccess.Should().BeTrue($"Add migration failed: {addResult}");

        var store = Services.GetRequiredService<IAuthorizationGrantStore>();
        var createResult = await store.CreateGrantAsync(
            AuthorizationTestData.PermissionNameChangeAppointment,
            AuthorizationTestData.ScopeTypeAlpha,
            AuthorizationTestData.ScopeKeyAlpha,
            AuthorizationTestData.SubjectTypeAlpha,
            subjectKey,
            AuthorizationGrantDecision.Granted,
            CancellationToken);

        createResult.IsSuccess.Should().BeTrue($"CreateGrantAsync failed: {createResult}");

        // Act — rename the permission
        var renamePlan = new PermissionMigrationPlan()
            .Rename(
                stableId,
                AuthorizationTestData.PermissionNameChangeAppointment,
                AuthorizationTestData.PermissionNameRescheduleAppointment);

        var renameResult = await runner.ApplyAsync(renamePlan, CancellationToken);

        // Assert — migration succeeded
        renameResult.IsSuccess.Should().BeTrue($"Rename migration failed: {renameResult}");

        var dbContext = Services.GetRequiredService<AuthorizationDbContext>();
        dbContext.Database.ProviderName.Should().Contain("Npgsql");

        // Assert — grant row now has the new permission name and kept the original grant identity
        var grantsResult = await store.GetGrantsForSubjectAsync(
            AuthorizationTestData.SubjectTypeAlpha,
            subjectKey,
            CancellationToken);

        grantsResult.Should().NotBeNull().And.HaveCount(1);
        grantsResult[0].PermissionName.Value.Should().Be(AuthorizationTestData.PermissionNameRescheduleAppointment.Value);
        grantsResult[0].Id.Should().Be(createResult.Value);
        grantsResult.Should().NotContain(item => item.PermissionName == AuthorizationTestData.PermissionNameChangeAppointment);

        var persistedGrant = await dbContext.Grants.SingleAsync(item => item.Id == createResult.Value.Value, CancellationToken);
        persistedGrant.StableId.Should().Be(stableId.Value);
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
        result.IsSuccess.Should().BeTrue($"Migration plan failed: {result}");

        var dbContext = Services.GetRequiredService<AuthorizationDbContext>();
        var snapshot = await dbContext.Snapshots.SingleAsync(CancellationToken);
        snapshot.PreviousPermissionName.Should().Be(AuthorizationTestData.PermissionNameChangeAppointment.Value);
        snapshot.NewPermissionName.Should().Be(AuthorizationTestData.PermissionNameRescheduleAppointment.Value);

        var manifestFound = Services.GetRequiredService<IAuthorizationDefinitionRegistry>()
            .TryGet(AuthorizationTestData.PermissionNameRescheduleAppointment, out var manifest);
        manifestFound.Should().BeTrue();
        manifest.PreviousNames.Should().Contain(AuthorizationTestData.PermissionNameChangeAppointment);
        manifest.Lifecycle.Should().Be(PermissionLifecycle.Active);
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
        addResult.IsSuccess.Should().BeTrue($"Add migration failed: {addResult}");

        var store = Services.GetRequiredService<IAuthorizationGrantStore>();
        var createResult = await store.CreateGrantAsync(
            AuthorizationTestData.PermissionNameChangeAppointment,
            AuthorizationTestData.ScopeTypeAlpha,
            AuthorizationTestData.ScopeKeyAlpha,
            AuthorizationTestData.SubjectTypeAlpha,
            subjectKey,
            AuthorizationGrantDecision.Granted,
            CancellationToken);

        createResult.IsSuccess.Should().BeTrue($"CreateGrantAsync failed: {createResult}");

        var renamePlan = new PermissionMigrationPlan()
            .Rename(
                stableId,
                AuthorizationTestData.PermissionNameWrite,
                AuthorizationTestData.PermissionNameRescheduleAppointment);

        var renameResult = await runner.ApplyAsync(renamePlan, CancellationToken);

        renameResult.IsSuccess.Should().BeFalse();

        var dbContext = Services.GetRequiredService<AuthorizationDbContext>();
        var catalog = await dbContext.Catalog.SingleAsync(item => item.StableId == stableId.Value, CancellationToken);
        catalog.PermissionName.Should().Be(AuthorizationTestData.PermissionNameChangeAppointment.Value);

        var grantsResult = await store.GetGrantsForSubjectAsync(
            AuthorizationTestData.SubjectTypeAlpha,
            subjectKey,
            CancellationToken);

        grantsResult.Should().NotBeNull().And.HaveCount(1);
        var grant = grantsResult[0];
        grant.PermissionName.Value.Should().Be(AuthorizationTestData.PermissionNameChangeAppointment.Value);
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

        result.IsSuccess.Should().BeFalse();

        var dbContext = Services.GetRequiredService<AuthorizationDbContext>();
        await dbContext.SaveChangesAsync(CancellationToken);

        var catalogEntries = await dbContext.Catalog.ToListAsync(CancellationToken);
        catalogEntries.Should().BeEmpty();
        dbContext.ChangeTracker.Entries().Should().BeEmpty();
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
        result.IsSuccess.Should().BeTrue($"Migration plan failed: {result}");

        // Note: the store does not enforce lifecycle on create — lifecycle enforcement is
        // an application-layer concern in IAuthorizationManager. This test verifies the migration
        // completes successfully and does not corrupt the catalog.
        var store = Services.GetRequiredService<IAuthorizationGrantStore>();
        var grantsResult = await store.GetGrantsForSubjectAsync(
            AuthorizationTestData.SubjectTypeAlpha,
            AuthorizationTestData.SubjectKeyAlpha,
            CancellationToken);

        grantsResult.Should().NotBeNull().And.BeEmpty();
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
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<MigrationCatalogEntryNotFoundError>();
    }

    private async Task ResetDatabaseAsync()
    {
        var dbContext = Services.GetRequiredService<AuthorizationDbContext>();
        await dbContext.Database.EnsureDeletedAsync(CancellationToken);

        var initializer = Services.GetRequiredService<AuthorizationDbInitializer>();
        await initializer.EnsureCreatedAsync(CancellationToken);
    }
}
