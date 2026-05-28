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

namespace Aiel.Permissions.Client;

public sealed class ActionCapabilitySnapshotCacheTests
{
    private static readonly PermissionName RescheduleAppointmentPermission =
        PermissionName.From("sample.Scheduling.RescheduleAppointment");

    private static readonly PermissionScopeTypeName LocationScopeType = PermissionScopeTypeName.From("Location");

    private static readonly PermissionScopeKey LocationScopeKey = PermissionScopeKey.From("clinic-west");

    [Fact]
    public void ForSelectedPermissions_UsesExplicitEmptyContinuationToken()
    {
        var request = ActionCapabilityRequest.ForSelectedPermissions(
            LocationScopeType,
            LocationScopeKey,
            [RescheduleAppointmentPermission],
            CapabilityContinuationToken.Empty);

        request.Mode.Should().Be(ActionCapabilityRequestMode.SelectedPermissions);
        request.RequestedPermissions.Should().ContainSingle().Which.Should().Be(RescheduleAppointmentPermission);
        request.ContinuationToken.IsEmpty.Should().BeTrue();
        request.ContinuationToken.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAuthorizationFailureAsync_WhenPermissionDenied_RefreshesSnapshot()
    {
        var request = ActionCapabilityRequest.ForSelectedPermissions(
            LocationScopeType,
            LocationScopeKey,
            [RescheduleAppointmentPermission],
            CapabilityContinuationToken.Empty);
        var capabilityService = new RecordingActionCapabilityService(
            CreateSnapshot("snapshot-v1", PermissionGrantDecision.Prohibited),
            CreateSnapshot("snapshot-v2", PermissionGrantDecision.Granted));
        var cache = new ActionCapabilitySnapshotCache(capabilityService);

        var initial = await cache.GetSnapshotAsync(request, TestContext.Current.CancellationToken);
        var refreshed = await cache.HandleAuthorizationFailureAsync(
            request,
            Result.Failure(PermissionErrors.PermissionDenied(RescheduleAppointmentPermission)),
            TestContext.Current.CancellationToken);

        initial.IsSuccess.Should().BeTrue();
        initial.Value.Version.Should().Be(CapabilitySnapshotVersion.From("snapshot-v1"));
        refreshed.IsSuccess.Should().BeTrue();
        refreshed.Value.Version.Should().Be(CapabilitySnapshotVersion.From("snapshot-v2"));
        capabilityService.CallCount.Should().Be(2);
    }

    private static ActionCapabilitySnapshot CreateSnapshot(String version, PermissionGrantDecision decision)
        => new(
            CapabilitySnapshotVersion.From(version),
            LocationScopeType,
            LocationScopeKey,
            [
                new ClientPermissionCapability
                {
                    SnapshotVersion = CapabilitySnapshotVersion.From(version),
                    PermissionName = RescheduleAppointmentPermission,
                    ScopeType = LocationScopeType,
                    ScopeKey = LocationScopeKey,
                    Decision = decision
                }
            ]);

    private sealed class RecordingActionCapabilityService(params ActionCapabilitySnapshot[] snapshots) : IActionCapabilityService
    {
        private readonly Queue<ActionCapabilitySnapshot> _snapshots = new(snapshots);

        public Int32 CallCount { get; private set; }

        public ValueTask<Result<ActionCapabilitySnapshot>> GetSnapshotAsync(
            ActionCapabilityRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            var snapshot = _snapshots.Count > 1 ? _snapshots.Dequeue() : _snapshots.Peek();
            return ValueTask.FromResult(Result.Success(snapshot));
        }
    }
}
