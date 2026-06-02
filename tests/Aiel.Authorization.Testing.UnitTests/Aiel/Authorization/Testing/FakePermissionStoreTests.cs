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

namespace Aiel.Authorization.Testing;

public sealed class FakePermissionStoreTests
{
    [Fact]
    public void CreateGrantCalls_InitiallyEmpty()
    {
        var store = new FakeAuthorizationGrantStore();
        store.CreateGrantCalls.Should().BeEmpty();
    }

    [Fact]
    public void GetGrantsCalls_InitiallyEmpty()
    {
        var store = new FakeAuthorizationGrantStore();
        store.GetGrantsCalls.Should().BeEmpty();
    }

    [Fact]
    public void RevokeGrantCalls_InitiallyEmpty()
    {
        var store = new FakeAuthorizationGrantStore();
        store.RevokeGrantCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateGrantAsync_RecordsCall()
    {
        var store = new FakeAuthorizationGrantStore();

        await store.CreateGrantAsync(
            AuthorizationTestData.PermissionNameRead,
            AuthorizationTestData.ScopeTypeAlpha,
            AuthorizationTestData.ScopeKeyAlpha,
            AuthorizationTestData.SubjectTypeAlpha,
            AuthorizationTestData.SubjectKeyAlpha,
            AuthorizationGrantDecision.Granted,
            TestContext.Current.CancellationToken);

        store.CreateGrantCalls.Should().ContainSingle();
        var call = store.CreateGrantCalls[0];
        call.PermissionName.Should().Be(AuthorizationTestData.PermissionNameRead);
        call.ScopeType.Should().Be(AuthorizationTestData.ScopeTypeAlpha);
        call.ScopeKey.Should().Be(AuthorizationTestData.ScopeKeyAlpha);
        call.SubjectType.Should().Be(AuthorizationTestData.SubjectTypeAlpha);
        call.SubjectKey.Should().Be(AuthorizationTestData.SubjectKeyAlpha);
        call.Decision.Should().Be(AuthorizationGrantDecision.Granted);
    }

    [Fact]
    public async Task CreateGrantAsync_ReturnsConfiguredResult()
    {
        var expected = AuthorizationTestData.GrantIdBeta;
        var store = new FakeAuthorizationGrantStore { CreateGrantResult = Result.Success(expected) };

        var result = await store.CreateGrantAsync(
            AuthorizationTestData.PermissionNameRead,
            AuthorizationTestData.ScopeTypeAlpha,
            AuthorizationTestData.ScopeKeyAlpha,
            AuthorizationTestData.SubjectTypeAlpha,
            AuthorizationTestData.SubjectKeyAlpha,
            AuthorizationGrantDecision.Granted,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void CreateGrantResult_DefaultIsSuccessWithNonDefaultId()
    {
        var store = new FakeAuthorizationGrantStore();
        store.CreateGrantResult.IsSuccess.Should().BeTrue();
        store.CreateGrantResult.Value.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task RevokeGrantAsync_RecordsGrantId()
    {
        var store = new FakeAuthorizationGrantStore();

        await store.RevokeGrantAsync(AuthorizationTestData.GrantIdAlpha, TestContext.Current.CancellationToken);

        store.RevokeGrantCalls.Should().ContainSingle()
            .Which.Should().Be(AuthorizationTestData.GrantIdAlpha);
    }

    [Fact]
    public async Task RevokeGrantAsync_ReturnsConfiguredResult()
    {
        var store = new FakeAuthorizationGrantStore
        {
            RevokeGrantResult = Result.Failure(AuthorizationErrors.PermissionDenied(AuthorizationTestData.PermissionNameRead))
        };

        var result = await store.RevokeGrantAsync(AuthorizationTestData.GrantIdAlpha, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void RevokeGrantResult_DefaultIsSuccess()
    {
        var store = new FakeAuthorizationGrantStore();
        store.RevokeGrantResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetGrantsForSubjectAsync_RecordsSubjectQuery()
    {
        var store = new FakeAuthorizationGrantStore();

        await store.GetGrantsForSubjectAsync(
            AuthorizationTestData.SubjectTypeAlpha,
            AuthorizationTestData.SubjectKeyAlpha,
            TestContext.Current.CancellationToken);

        store.GetGrantsCalls.Should().ContainSingle();
        var call = store.GetGrantsCalls[0];
        call.SubjectType.Should().Be(AuthorizationTestData.SubjectTypeAlpha);
        call.SubjectKey.Should().Be(AuthorizationTestData.SubjectKeyAlpha);
    }

    [Fact]
    public async Task GetGrantsForSubjectAsync_ReturnsConfiguredResult()
    {
        var grant = new AuthorizationGrantSummary
        {
            GrantId = AuthorizationTestData.GrantIdAlpha,
            PermissionName = AuthorizationTestData.PermissionNameRead,
            ScopeType = AuthorizationTestData.ScopeTypeAlpha,
            ScopeKey = AuthorizationTestData.ScopeKeyAlpha,
            SubjectType = AuthorizationTestData.SubjectTypeAlpha,
            SubjectKey = AuthorizationTestData.SubjectKeyAlpha,
            Decision = AuthorizationGrantDecision.Granted
        };

        IReadOnlyList<AuthorizationGrantSummary> grants = [grant];
        var store = new FakeAuthorizationGrantStore
        {
            GetGrantsResult = Result.Success(grants)
        };

        var result = await store.GetGrantsForSubjectAsync(
            AuthorizationTestData.SubjectTypeAlpha,
            AuthorizationTestData.SubjectKeyAlpha,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public void GetGrantsResult_DefaultIsEmptyList()
    {
        var store = new FakeAuthorizationGrantStore();
        store.GetGrantsResult.IsSuccess.Should().BeTrue();
        store.GetGrantsResult.Value.Should().NotBeNull().And.BeEmpty();
    }
}
