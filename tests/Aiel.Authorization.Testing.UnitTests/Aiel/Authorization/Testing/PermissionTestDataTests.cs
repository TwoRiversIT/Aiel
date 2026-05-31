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
        => PermissionTestData.GrantIdAlpha.Value.Should().NotBe(Guid.Empty);

    [Fact]
    public void GrantIdBeta_IsNonDefault_AndDiffersFromAlpha()
    {
        PermissionTestData.GrantIdBeta.Value.Should().NotBe(Guid.Empty);
        PermissionTestData.GrantIdBeta.Should().NotBe(PermissionTestData.GrantIdAlpha);
    }

    [Fact]
    public void StableIdAlpha_IsNonEmpty()
        => PermissionTestData.StableIdAlpha.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void StableIdBeta_IsNonEmpty_AndDiffersFromAlpha()
    {
        PermissionTestData.StableIdBeta.Value.Should().NotBeNullOrEmpty();
        PermissionTestData.StableIdBeta.Should().NotBe(PermissionTestData.StableIdAlpha);
    }

    [Fact]
    public void AppointmentIdAlpha_IsNonDefault()
        => PermissionTestData.AppointmentIdAlpha.Should().NotBe(Guid.Empty);

    [Fact]
    public void AppointmentIdBeta_IsNonDefault_AndDiffersFromAlpha()
    {
        PermissionTestData.AppointmentIdBeta.Should().NotBe(Guid.Empty);
        PermissionTestData.AppointmentIdBeta.Should().NotBe(PermissionTestData.AppointmentIdAlpha);
    }

    [Fact]
    public void PermissionNameRead_IsNonEmpty()
        => PermissionTestData.PermissionNameRead.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void PermissionNameWrite_IsNonEmpty_AndDiffersFromRead()
    {
        PermissionTestData.PermissionNameWrite.Value.Should().NotBeNullOrEmpty();
        PermissionTestData.PermissionNameWrite.Should().NotBe(PermissionTestData.PermissionNameRead);
    }

    [Fact]
    public void ScopeTypeAlpha_IsNonEmpty()
        => PermissionTestData.ScopeTypeAlpha.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void ScopeKeyAlpha_IsNonEmpty()
        => PermissionTestData.ScopeKeyAlpha.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void ScopeKeyBeta_IsNonEmpty_AndDiffersFromAlpha()
    {
        PermissionTestData.ScopeKeyBeta.Value.Should().NotBeNullOrEmpty();
        PermissionTestData.ScopeKeyBeta.Should().NotBe(PermissionTestData.ScopeKeyAlpha);
    }

    [Fact]
    public void AppointmentResourceScopeKeyAlpha_IsNonEmpty()
        => PermissionTestData.AppointmentResourceScopeKeyAlpha.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void AppointmentResourceScopeKeyBeta_IsNonEmpty_AndDiffersFromAlpha()
    {
        PermissionTestData.AppointmentResourceScopeKeyBeta.Value.Should().NotBeNullOrEmpty();
        PermissionTestData.AppointmentResourceScopeKeyBeta.Should().NotBe(PermissionTestData.AppointmentResourceScopeKeyAlpha);
    }

    [Fact]
    public void SubjectTypeAlpha_IsNonEmpty()
        => PermissionTestData.SubjectTypeAlpha.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void SubjectKeyAlpha_IsNonEmpty()
        => PermissionTestData.SubjectKeyAlpha.Value.Should().NotBeNullOrEmpty();

    [Fact]
    public void SubjectKeyBeta_IsNonEmpty_AndDiffersFromAlpha()
    {
        PermissionTestData.SubjectKeyBeta.Value.Should().NotBeNullOrEmpty();
        PermissionTestData.SubjectKeyBeta.Should().NotBe(PermissionTestData.SubjectKeyAlpha);
    }

    [Fact]
    public void CreateSampleManifest_ReturnsNonNullManifest()
    {
        var manifest = PermissionTestData.CreateSampleManifest();
        manifest.Should().NotBeNull();
        manifest.DisplayName.Should().NotBeNullOrEmpty();
        manifest.ActionType.Should().NotBeNull();
        manifest.Lifecycle.Should().Be(PermissionLifecycle.Active);
        manifest.PreviousNames.Should().BeEmpty();
    }

    [Fact]
    public void CreateSampleManifest_Generic_UsesSpecifiedActionType()
    {
        var manifest = PermissionTestData.CreateSampleManifest<Fixtures.BetaTestAction>();
        manifest.ActionType.Should().Be<Fixtures.BetaTestAction>();
    }

    [Fact]
    public void CreateRescheduleAppointmentManifest_UsesPreviousNameMetadata()
    {
        var manifest = PermissionTestData.CreateRescheduleAppointmentManifest();

        manifest.StableId.Should().Be(PermissionTestData.StableIdAppointment);
        manifest.PermissionName.Should().Be(PermissionTestData.PermissionNameRescheduleAppointment);
        manifest.ActionType.Should().Be<Fixtures.RescheduleAppointmentTestAction>();
        manifest.PreviousNames.Should().ContainSingle().Which.Should().Be(PermissionTestData.PermissionNameChangeAppointment);
    }
}
