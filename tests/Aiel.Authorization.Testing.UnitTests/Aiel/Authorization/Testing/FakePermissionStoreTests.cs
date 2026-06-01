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
        var store = new FakePermissionStore();
        store.CreateGrantCalls.Should().BeEmpty();
    }

    [Fact]
    public void GetGrantsCalls_InitiallyEmpty()
    {
        var store = new FakePermissionStore();
        store.GetGrantsCalls.Should().BeEmpty();
    }

    [Fact]
    public void RevokeGrantCalls_InitiallyEmpty()
    {
        var store = new FakePermissionStore();
        store.RevokeGrantCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateGrantAsync_RecordsCall()
    {
        var store = new FakePermissionStore();

        await store.CreateGrantAsync(
            PermissionTestData.PermissionNameRead,
            PermissionTestData.ScopeTypeAlpha,
            PermissionTestData.ScopeKeyAlpha,
            PermissionTestData.SubjectTypeAlpha,
            PermissionTestData.SubjectKeyAlpha,
            PermissionGrantDecision.Granted,
            TestContext.Current.CancellationToken);

        store.CreateGrantCalls.Should().ContainSingle();
        var call = store.CreateGrantCalls[0];
        call.PermissionName.Should().Be(PermissionTestData.PermissionNameRead);
        call.ScopeType.Should().Be(PermissionTestData.ScopeTypeAlpha);
        call.ScopeKey.Should().Be(PermissionTestData.ScopeKeyAlpha);
        call.SubjectType.Should().Be(PermissionTestData.SubjectTypeAlpha);
        call.SubjectKey.Should().Be(PermissionTestData.SubjectKeyAlpha);
        call.Decision.Should().Be(PermissionGrantDecision.Granted);
    }

    [Fact]
    public async Task CreateGrantAsync_ReturnsConfiguredResult()
    {
        var expected = PermissionTestData.GrantIdBeta;
        var store = new FakePermissionStore { CreateGrantResult = Result.Success(expected) };

        var result = await store.CreateGrantAsync(
            PermissionTestData.PermissionNameRead,
            PermissionTestData.ScopeTypeAlpha,
            PermissionTestData.ScopeKeyAlpha,
            PermissionTestData.SubjectTypeAlpha,
            PermissionTestData.SubjectKeyAlpha,
            PermissionGrantDecision.Granted,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void CreateGrantResult_DefaultIsSuccessWithNonDefaultId()
    {
        var store = new FakePermissionStore();
        store.CreateGrantResult.IsSuccess.Should().BeTrue();
        store.CreateGrantResult.Value.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task RevokeGrantAsync_RecordsGrantId()
    {
        var store = new FakePermissionStore();

        await store.RevokeGrantAsync(PermissionTestData.GrantIdAlpha, TestContext.Current.CancellationToken);

        store.RevokeGrantCalls.Should().ContainSingle()
            .Which.Should().Be(PermissionTestData.GrantIdAlpha);
    }

    [Fact]
    public async Task RevokeGrantAsync_ReturnsConfiguredResult()
    {
        var store = new FakePermissionStore
        {
            RevokeGrantResult = Result.Failure(PermissionErrors.PermissionDenied(PermissionTestData.PermissionNameRead))
        };

        var result = await store.RevokeGrantAsync(PermissionTestData.GrantIdAlpha, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void RevokeGrantResult_DefaultIsSuccess()
    {
        var store = new FakePermissionStore();
        store.RevokeGrantResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetGrantsForSubjectAsync_RecordsSubjectQuery()
    {
        var store = new FakePermissionStore();

        await store.GetGrantsForSubjectAsync(
            PermissionTestData.SubjectTypeAlpha,
            PermissionTestData.SubjectKeyAlpha,
            TestContext.Current.CancellationToken);

        store.GetGrantsCalls.Should().ContainSingle();
        var call = store.GetGrantsCalls[0];
        call.SubjectType.Should().Be(PermissionTestData.SubjectTypeAlpha);
        call.SubjectKey.Should().Be(PermissionTestData.SubjectKeyAlpha);
    }

    [Fact]
    public async Task GetGrantsForSubjectAsync_ReturnsConfiguredResult()
    {
        var grant = new PermissionGrantSummary
        {
            GrantId = PermissionTestData.GrantIdAlpha,
            PermissionName = PermissionTestData.PermissionNameRead,
            ScopeType = PermissionTestData.ScopeTypeAlpha,
            ScopeKey = PermissionTestData.ScopeKeyAlpha,
            SubjectType = PermissionTestData.SubjectTypeAlpha,
            SubjectKey = PermissionTestData.SubjectKeyAlpha,
            Decision = PermissionGrantDecision.Granted
        };

        IReadOnlyList<PermissionGrantSummary> grants = [grant];
        var store = new FakePermissionStore
        {
            GetGrantsResult = Result.Success(grants)
        };

        var result = await store.GetGrantsForSubjectAsync(
            PermissionTestData.SubjectTypeAlpha,
            PermissionTestData.SubjectKeyAlpha,
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public void GetGrantsResult_DefaultIsEmptyList()
    {
        var store = new FakePermissionStore();
        store.GetGrantsResult.IsSuccess.Should().BeTrue();
        store.GetGrantsResult.Value.Should().NotBeNull().And.BeEmpty();
    }
}
