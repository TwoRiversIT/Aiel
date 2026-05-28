# Aiel.Permissions.Client

Client-side capability caching and refresh helpers for `Aiel.Permissions` capability snapshots.

## What This Package Contains

- `IActionCapabilitySnapshotCache`
- `ActionCapabilitySnapshotCache`
- `ActionCapabilitySnapshotExtensions.CanExecute(...)`

The request and snapshot DTOs remain in `Aiel.Permissions.Application.Contracts` so server and client code share the same contract types without pulling UI adapters inward.

## Typical Usage

```csharp
var request = ActionCapabilityRequest.ForSelectedPermissions(
	PermissionScopeTypeName.From("Location"),
	PermissionScopeKey.From("clinic-west"),
	[PermissionName.From("sample.Scheduling.RescheduleAppointment")],
	CapabilityContinuationToken.Empty);

var snapshotResult = await cache.GetSnapshotAsync(request, cancellationToken);
if (snapshotResult.IsSuccess && snapshotResult.Value.CanExecute(PermissionName.From("sample.Scheduling.RescheduleAppointment")))
{
	// Show the action affordance.
}
```

When an application-service call returns `PermissionDeniedError`, call `HandleAuthorizationFailureAsync(...)` to invalidate the cached entry for the request key and fetch a fresh snapshot version.