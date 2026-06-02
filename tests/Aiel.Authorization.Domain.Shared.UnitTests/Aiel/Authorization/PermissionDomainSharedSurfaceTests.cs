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

namespace Aiel.Authorization;

public sealed class PermissionDomainSharedSurfaceTests
{
    [Fact]
    public void PermissionName_TryCreate_RejectsInvalidValues()
    {
        PermissionName.TryCreate(null, out _).Should().BeFalse();
        PermissionName.TryCreate(String.Empty, out _).Should().BeFalse();
        PermissionName.TryCreate("   ", out _).Should().BeFalse();
        PermissionName.TryCreate("Aviendha..Scheduling.ChangeAppointment", out _).Should().BeFalse();
        PermissionName.TryCreate("Aviendha.Scheduling.Change Appointment", out _).Should().BeFalse();
    }

    [Fact]
    public void PermissionName_From_TrimsAndPreservesCanonicalValue()
    {
        var permissionName = PermissionName.From("  Aviendha.Scheduling.Appointments.RescheduleAppointment  ");

        permissionName.Value.Should().Be("Aviendha.Scheduling.Appointments.RescheduleAppointment");
        permissionName.ToString().Should().Be(permissionName.Value);
    }

    [Fact]
    public void PermissionScopeAndSubjectTypeNames_RejectInvalidValues()
    {
        AuthorizationScopeTypeName.TryCreate(null, out _).Should().BeFalse();
        AuthorizationScopeTypeName.TryCreate("Clinic.Scope", out _).Should().BeFalse();
        AuthorizationScopeTypeName.TryCreate("Clinic Scope", out _).Should().BeFalse();

        AuthorizationSubjectTypeName.TryCreate(null, out _).Should().BeFalse();
        AuthorizationSubjectTypeName.TryCreate("Tenant.User", out _).Should().BeFalse();
        AuthorizationSubjectTypeName.TryCreate("Tenant User", out _).Should().BeFalse();
    }

    [Fact]
    public void PermissionScopeAndSubjectKeys_TrimAndRejectWhitespaceOnlyValues()
    {
        AuthorizationScopeKey.TryCreate("   ", out _).Should().BeFalse();
        AuthorizationSubjectKey.TryCreate("   ", out _).Should().BeFalse();

        AuthorizationScopeKey.TryCreate("  clinic:west  ", out var scopeKey).Should().BeTrue();
        AuthorizationSubjectKey.TryCreate("  user:42  ", out var subjectKey).Should().BeTrue();

        scopeKey.Value.Should().Be("clinic:west");
        subjectKey.Value.Should().Be("user:42");
    }

    [Fact]
    public void StrongIds_AreGeneratedAndUsable()
    {
        AuthorizationGrantId.TryFrom(Guid.NewGuid(), out var grantId).Should().BeTrue();
        PermissionStableId.TryFrom("  perm_01jz9p58d6d8m8n7x3t9q2a4bc  ", out var stableId).Should().BeTrue();
        CapabilitySnapshotVersion.TryFrom("  capability-snapshot-v1  ", out var snapshotVersion).Should().BeTrue();

        grantId.Value.Should().NotBe(Guid.Empty);
        stableId.Value.Should().Be("perm_01jz9p58d6d8m8n7x3t9q2a4bc");
        snapshotVersion.Value.Should().Be("capability-snapshot-v1");
    }

    [Fact]
    public void StrongIds_RejectDefaultOrWhitespaceValues()
    {
        AuthorizationGrantId.TryFrom(Guid.Empty, out _).Should().BeFalse();
        PermissionStableId.TryFrom("   ", out _).Should().BeFalse();
        CapabilitySnapshotVersion.TryFrom(String.Empty, out _).Should().BeFalse();
    }

    [Fact]
    public void PermissionEnums_ExposeTheInitialContractSurface()
    {
        Enum.GetNames<PermissionLifecycle>().Should().Equal("Active", "Deprecated", "Removed");
        Enum.GetNames<AuthorizationGrantDecision>().Should().Equal("Granted", "Prohibited");
    }
}
