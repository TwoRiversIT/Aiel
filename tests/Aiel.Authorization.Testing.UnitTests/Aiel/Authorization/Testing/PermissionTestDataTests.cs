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

namespace Aiel.Authorization.Testing;

public sealed class PermissionTestDataTests
{
    [Fact]
    public void GrantIdAlpha_IsNonDefault()
        => AuthorizationTestData.GrantIdAlpha.Value.Should().NotBe(Guid.Empty);

    [Fact]
    public void GrantIdBeta_IsNonDefault_AndDiffersFromAlpha()
    {
        AuthorizationTestData.GrantIdBeta.Value.Should().NotBe(Guid.Empty);
        AuthorizationTestData.GrantIdBeta.Should().NotBe(AuthorizationTestData.GrantIdAlpha);
    }

    [Fact]
    public void StableIdAlpha_IsNonEmpty()
        => AuthorizationTestData.StableIdAlpha.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void StableIdBeta_IsNonEmpty_AndDiffersFromAlpha()
    {
        AuthorizationTestData.StableIdBeta.Value.Should().NotBeNullOrEmpty();
        AuthorizationTestData.StableIdBeta.Should().NotBe(AuthorizationTestData.StableIdAlpha);
    }

    [Fact]
    public void AppointmentIdAlpha_IsNonDefault()
        => AuthorizationTestData.AppointmentIdAlpha.Should().NotBe(Guid.Empty);

    [Fact]
    public void AppointmentIdBeta_IsNonDefault_AndDiffersFromAlpha()
    {
        AuthorizationTestData.AppointmentIdBeta.Should().NotBe(Guid.Empty);
        AuthorizationTestData.AppointmentIdBeta.Should().NotBe(AuthorizationTestData.AppointmentIdAlpha);
    }

    [Fact]
    public void PermissionNameRead_IsNonEmpty()
        => AuthorizationTestData.PermissionNameRead.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void PermissionNameWrite_IsNonEmpty_AndDiffersFromRead()
    {
        AuthorizationTestData.PermissionNameWrite.Value.Should().NotBeNullOrEmpty();
        AuthorizationTestData.PermissionNameWrite.Should().NotBe(AuthorizationTestData.PermissionNameRead);
    }

    [Fact]
    public void ScopeTypeAlpha_IsNonEmpty()
        => AuthorizationTestData.ScopeTypeAlpha.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void ScopeKeyAlpha_IsNonEmpty()
        => AuthorizationTestData.ScopeKeyAlpha.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void ScopeKeyBeta_IsNonEmpty_AndDiffersFromAlpha()
    {
        AuthorizationTestData.ScopeKeyBeta.Value.Should().NotBeNullOrEmpty();
        AuthorizationTestData.ScopeKeyBeta.Should().NotBe(AuthorizationTestData.ScopeKeyAlpha);
    }

    [Fact]
    public void AppointmentResourceScopeKeyAlpha_IsNonEmpty()
        => AuthorizationTestData.AppointmentResourceScopeKeyAlpha.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void AppointmentResourceScopeKeyBeta_IsNonEmpty_AndDiffersFromAlpha()
    {
        AuthorizationTestData.AppointmentResourceScopeKeyBeta.Value.Should().NotBeNullOrEmpty();
        AuthorizationTestData.AppointmentResourceScopeKeyBeta.Should().NotBe(AuthorizationTestData.AppointmentResourceScopeKeyAlpha);
    }

    [Fact]
    public void SubjectTypeAlpha_IsNonEmpty()
        => AuthorizationTestData.SubjectTypeAlpha.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void SubjectKeyAlpha_IsNonEmpty()
        => AuthorizationTestData.SubjectKeyAlpha.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void SubjectKeyBeta_IsNonEmpty_AndDiffersFromAlpha()
    {
        AuthorizationTestData.SubjectKeyBeta.Value.Should().NotBeNullOrEmpty();
        AuthorizationTestData.SubjectKeyBeta.Should().NotBe(AuthorizationTestData.SubjectKeyAlpha);
    }

    [Fact]
    public void CreateSampleManifest_ReturnsNonNullManifest()
    {
        var manifest = AuthorizationTestData.CreateSampleManifest();
        manifest.Should().NotBeNull();
        manifest.DisplayName.Should().NotBeNullOrEmpty();
        manifest.ActionType.Should().NotBeNull();
        manifest.Lifecycle.Should().Be(PermissionLifecycle.Active);
        manifest.PreviousNames.Should().BeEmpty();
    }

    [Fact]
    public void CreateSampleManifest_Generic_UsesSpecifiedActionType()
    {
        var manifest = AuthorizationTestData.CreateSampleManifest<Fixtures.BetaTestAction>();
        manifest.ActionType.Should().Be<Fixtures.BetaTestAction>();
    }

    [Fact]
    public void CreateRescheduleAppointmentManifest_UsesPreviousNameMetadata()
    {
        var manifest = AuthorizationTestData.CreateRescheduleAppointmentManifest();

        manifest.StableId.Should().Be(AuthorizationTestData.StableIdAppointment);
        manifest.PermissionName.Should().Be(AuthorizationTestData.PermissionNameRescheduleAppointment);
        manifest.ActionType.Should().Be<Fixtures.RescheduleAppointmentTestAction>();
        manifest.PreviousNames.Should().ContainSingle().Which.Should().Be(AuthorizationTestData.PermissionNameChangeAppointment);
    }
}
