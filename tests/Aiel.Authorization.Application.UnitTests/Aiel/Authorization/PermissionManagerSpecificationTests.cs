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

using Aiel.Actions;
using Aiel.Results;
using System.Diagnostics.CodeAnalysis;

namespace Aiel.Authorization;

/// <summary>
/// Specification-level tests for <see cref="DefaultPermissionManager"/> behavior.
/// </summary>
public sealed class PermissionManagerSpecificationTests
{
    private static readonly PermissionName DocumentsRead = PermissionName.From("documents.read");
    private static readonly AuthorizationScopeTypeName TenantScope = AuthorizationScopeTypeName.From("Tenant");
    private static readonly AuthorizationScopeKey TenantScopeKey = AuthorizationScopeKey.From("t-1");
    private static readonly AuthorizationSubjectTypeName UserSubject = AuthorizationSubjectTypeName.From("User");
    private static readonly AuthorizationSubjectKey UserSubjectKey = AuthorizationSubjectKey.From("u-1");

    [Fact]
    public async Task GrantPermissionAsync_WhenPermissionNotRegistered_ReturnsMissingAuthorizationStoryError()
    {
        var manager = CreateManager(registryHasPermission: false);

        var result = await manager.GrantAsync(CreateRequest(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<MissingAuthorizationStoryError>();
    }

    [Fact]
    public async Task GrantPermissionAsync_WhenValid_DelegatesToStoreAndReturnsGrantId()
    {
        var expectedId = AuthorizationGrantId.From(Guid.Parse("d9f15185-8ea2-4c86-b4c5-57e59b7a8ae2"));
        var store = new FakePermissionStore
        {
            CreateGrantResult = Result.Success(expectedId)
        };
        var manager = CreateManager(store: store);

        var result = await manager.GrantAsync(CreateRequest(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedId);
        store.CreateGrantCalls.Should().ContainSingle();
        store.CreateGrantCalls[0].PermissionName.Should().Be(DocumentsRead);
        store.CreateGrantCalls[0].ScopeType.Should().Be(TenantScope);
        store.CreateGrantCalls[0].ScopeKey.Should().Be(TenantScopeKey);
        store.CreateGrantCalls[0].SubjectType.Should().Be(UserSubject);
        store.CreateGrantCalls[0].SubjectKey.Should().Be(UserSubjectKey);
        store.CreateGrantCalls[0].Decision.Should().Be(AuthorizationGrantDecision.Granted);
    }

    [Fact]
    public async Task RevokePermissionAsync_DelegatesToStoreWithGrantId()
    {
        var store = new FakePermissionStore
        {
            RevokeGrantResult = Result.Success()
        };
        var manager = CreateManager(store: store);
        var grantId = AuthorizationGrantId.From(Guid.Parse("2b30dadf-a609-4b4a-8c7d-7a2410508f8d"));

        var result = await manager.RevokeAsync(
            new RevokeAuthorizationRequest { GrantId = grantId },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        store.RevokeGrantCalls.Should().ContainSingle().Which.Should().Be(grantId);
    }

    [Fact]
    public async Task GetGrantsForSubjectAsync_DoesNotExposePersistenceRecords()
    {
        var store = new FakePermissionStore
        {
            GetGrantsResult = Result.Success<IReadOnlyList<AuthorizationGrantSummary>>(
                [
                    new AuthorizationGrantSummary
                    {
                        GrantId = AuthorizationGrantId.From(Guid.Parse("ca64b97a-eb26-4d06-9a75-1f0a6d0efeea")),
                        PermissionName = DocumentsRead,
                        ScopeType = TenantScope,
                        ScopeKey = TenantScopeKey,
                        SubjectType = UserSubject,
                        SubjectKey = UserSubjectKey,
                        Decision = AuthorizationGrantDecision.Granted
                    }
                ])
        };
        var manager = CreateManager(store: store);

        var result = await manager.GetGrantsForSubjectAsync(
            UserSubject,
            UserSubjectKey,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().AllBeOfType<AuthorizationGrantSummary>();
        store.GetGrantsCalls.Should().ContainSingle();
    }

    private static DefaultPermissionManager CreateManager(
        Boolean registryHasPermission = true,
        FakePermissionStore? store = null)
        => new(
            new FakePermissionDefinitionRegistry(registryHasPermission),
            store ?? new FakePermissionStore
            {
                CreateGrantResult = Result.Success(AuthorizationGrantId.From(Guid.Parse("d972d5cd-b196-4a0b-bffa-31bb05789d76"))),
                RevokeGrantResult = Result.Success(),
                GetGrantsResult = Result.Success<IReadOnlyList<AuthorizationGrantSummary>>([])
            });

    private static GrantPermissionRequest CreateRequest()
        => new()
        {
            PermissionName = DocumentsRead,
            ScopeType = TenantScope,
            ScopeKey = TenantScopeKey,
            SubjectType = UserSubject,
            SubjectKey = UserSubjectKey,
            Decision = AuthorizationGrantDecision.Granted
        };

    private sealed class FakePermissionDefinitionRegistry(Boolean hasPermission) : IAuthorizationDefinitionRegistry
    {
        private static readonly AuthorizationDefinitionManifest Manifest = new()
        {
            StableId = PermissionStableId.From("perm_01k0task5manager000000000001"),
            PermissionName = DocumentsRead,
            ScopeType = TenantScope,
            SubjectType = UserSubject,
            DisplayName = "Read documents",
            Description = "Allows a user to read documents.",
            ActionType = typeof(TestAction)
        };

        public IReadOnlyList<AuthorizationDefinitionManifest> GetAll()
            => hasPermission ? new[] { Manifest } : [];

        public Boolean TryGet(
            PermissionName permissionName,
            [NotNullWhen(true)] out AuthorizationDefinitionManifest manifest)
        {
            if (hasPermission && permissionName == Manifest.PermissionName)
            {
                manifest = Manifest;
                return true;
            }

            manifest = null!;
            return false;
        }

        public Boolean TryGetForAction<TAction>([NotNullWhen(true)] out AuthorizationDefinitionManifest manifest)
            where TAction : IAction
        {
            if (hasPermission && typeof(TAction) == Manifest.ActionType)
            {
                manifest = Manifest;
                return true;
            }

            manifest = null!;
            return false;
        }
    }

    private sealed class FakePermissionStore : IAuthorizationGrantStore
    {
        public List<CreateGrantCall> CreateGrantCalls { get; } = [];

        public List<PermissionSubjectLookup> GetGrantsCalls { get; } = [];

        public List<AuthorizationGrantId> RevokeGrantCalls { get; } = [];

        public Result<AuthorizationGrantId> CreateGrantResult { get; init; } =
            Result.Success(AuthorizationGrantId.From(Guid.Parse("7f1c7467-bbe9-4adc-ab9d-b7bf869d1b87")));

        public Result<IReadOnlyList<AuthorizationGrantSummary>> GetGrantsResult { get; init; } =
            Result.Success<IReadOnlyList<AuthorizationGrantSummary>>([]);

        public Result RevokeGrantResult { get; init; } = Result.Success();

        public Task<Result<AuthorizationGrantId>> CreateGrantAsync(
            PermissionName permissionName,
            AuthorizationScopeTypeName scopeType,
            AuthorizationScopeKey scopeKey,
            AuthorizationSubjectTypeName subjectType,
            AuthorizationSubjectKey subjectKey,
            AuthorizationGrantDecision decision,
            CancellationToken cancellationToken = default)
        {
            CreateGrantCalls.Add(new CreateGrantCall(
                permissionName,
                scopeType,
                scopeKey,
                subjectType,
                subjectKey,
                decision));

            return Task.FromResult(CreateGrantResult);
        }

        public Task<Result<IReadOnlyList<AuthorizationGrantSummary>>> GetGrantsForSubjectAsync(
            AuthorizationSubjectTypeName subjectType,
            AuthorizationSubjectKey subjectKey,
            CancellationToken cancellationToken = default)
        {
            GetGrantsCalls.Add(new PermissionSubjectLookup(subjectType, subjectKey));
            return Task.FromResult(GetGrantsResult);
        }

        public Task<Result> RevokeGrantAsync(
            AuthorizationGrantId grantId,
            CancellationToken cancellationToken = default)
        {
            RevokeGrantCalls.Add(grantId);
            return Task.FromResult(RevokeGrantResult);
        }
    }

    private sealed record CreateGrantCall(
        PermissionName PermissionName,
        AuthorizationScopeTypeName ScopeType,
        AuthorizationScopeKey ScopeKey,
        AuthorizationSubjectTypeName SubjectType,
        AuthorizationSubjectKey SubjectKey,
        AuthorizationGrantDecision Decision);

    private sealed record PermissionSubjectLookup(
        AuthorizationSubjectTypeName SubjectType,
        AuthorizationSubjectKey SubjectKey);

    private sealed class TestAction : IAction;
}
