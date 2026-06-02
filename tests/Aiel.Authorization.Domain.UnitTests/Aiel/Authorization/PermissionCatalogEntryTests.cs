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

public sealed class PermissionCatalogEntryTests
{
    [Fact]
    public void Create_RejectsMissingStableId_Name_AndScopeType()
    {
        var result = PermissionCatalogEntry.Create(
            default,
            default,
            default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidPermissionCatalogEntryError>();
    }

    [Fact]
    public void Create_UsesProvidedIdentityAndDefaultsToActiveLifecycle()
    {
        var result = PermissionCatalogEntry.Create(
            PermissionStableId.From("perm_01k0task4catalog000000000001"),
            PermissionName.From("Aviendha.Scheduling.Appointments.ChangeAppointment"),
            AuthorizationScopeTypeName.From("Clinic"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(PermissionStableId.From("perm_01k0task4catalog000000000001"));
        result.Value.PermissionName.Should().Be(PermissionName.From("Aviendha.Scheduling.Appointments.ChangeAppointment"));
        result.Value.ScopeType.Should().Be(AuthorizationScopeTypeName.From("Clinic"));
        result.Value.Lifecycle.Should().Be(PermissionLifecycle.Active);
    }

    [Fact]
    public void TransitionTo_AllowsForwardLifecycleChanges()
    {
        var catalogEntry = CreateCatalogEntry();

        var deprecateResult = catalogEntry.TransitionTo(PermissionLifecycle.Deprecated);
        var removeResult = catalogEntry.TransitionTo(PermissionLifecycle.Removed);

        deprecateResult.IsSuccess.Should().BeTrue();
        removeResult.IsSuccess.Should().BeTrue();
        catalogEntry.Lifecycle.Should().Be(PermissionLifecycle.Removed);
    }

    [Fact]
    public void TransitionTo_RejectsSkippingDeprecatedLifecycleState()
    {
        var catalogEntry = CreateCatalogEntry();

        var result = catalogEntry.TransitionTo(PermissionLifecycle.Removed);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidAuthorizationLifecycleTransitionError>();
        catalogEntry.Lifecycle.Should().Be(PermissionLifecycle.Active);
    }

    [Fact]
    public void TransitionTo_RejectsReverseLifecycleChangesWithoutThrowing()
    {
        var catalogEntry = CreateCatalogEntry(PermissionLifecycle.Deprecated);

        var result = catalogEntry.TransitionTo(PermissionLifecycle.Active);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidAuthorizationLifecycleTransitionError>();
        catalogEntry.Lifecycle.Should().Be(PermissionLifecycle.Deprecated);
    }

    private static PermissionCatalogEntry CreateCatalogEntry(PermissionLifecycle lifecycle = PermissionLifecycle.Active)
    {
        var result = PermissionCatalogEntry.Create(
            PermissionStableId.From("perm_01k0task4catalog000000000002"),
            PermissionName.From("Aviendha.Scheduling.Appointments.ChangeAppointment"),
            AuthorizationScopeTypeName.From("Clinic"),
            lifecycle);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}
