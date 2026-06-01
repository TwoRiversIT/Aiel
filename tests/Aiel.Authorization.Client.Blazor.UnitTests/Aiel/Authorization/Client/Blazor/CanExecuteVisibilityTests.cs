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

namespace Aiel.Authorization.Client.Blazor;

public sealed class CanExecuteVisibilityTests
{
    private static readonly PermissionName RescheduleAppointmentPermission =
        PermissionName.From("sample.Scheduling.RescheduleAppointment");

    private static readonly PermissionScopeTypeName LocationScopeType = PermissionScopeTypeName.From("Location");

    private static readonly PermissionScopeKey LocationScopeKey = PermissionScopeKey.From("clinic-west");

    [Fact]
    public void CanExecute_WhenRescheduleAppointmentIsUnavailable_ReturnsFalse()
    {
        var snapshot = CreateSnapshot(PermissionGrantDecision.Prohibited);

        ActionCapabilityVisibility.CanExecute(snapshot, RescheduleAppointmentPermission).Should().BeFalse();
    }

    [Fact]
    public void CanExecute_WhenRescheduleAppointmentIsGranted_ReturnsTrue()
    {
        var snapshot = CreateSnapshot(PermissionGrantDecision.Granted);

        ActionCapabilityVisibility.CanExecute(snapshot, RescheduleAppointmentPermission).Should().BeTrue();
    }

    private static ActionCapabilitySnapshot CreateSnapshot(PermissionGrantDecision decision)
        => new(
            CapabilitySnapshotVersion.From("snapshot-v1"),
            LocationScopeType,
            LocationScopeKey,
            [
                new ClientPermissionCapability
                {
                    SnapshotVersion = CapabilitySnapshotVersion.From("snapshot-v1"),
                    PermissionName = RescheduleAppointmentPermission,
                    ScopeType = LocationScopeType,
                    ScopeKey = LocationScopeKey,
                    Decision = decision
                }
            ]);
}
