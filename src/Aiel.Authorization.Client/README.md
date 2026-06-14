# Aiel.Authorization.Client

Client-side capability caching and refresh helpers for `Aiel.Authorization` capability snapshots.

## What This Package Contains

- `IActionCapabilitySnapshotCache`
- `ActionCapabilitySnapshotCache`
- `ActionCapabilitySnapshotExtensions.CanExecute(...)`

The request and snapshot DTOs remain in `Aiel.Authorization.Application.Contracts` so server and client code share the same contract types without pulling UI adapters inward.

## Typical Usage

```csharp
var request = ActionCapabilityRequest.ForSelectedPermissions(
	AuthorizationScopeTypeName.From("Location"),
	AuthorizationScopeKey.From("clinic-west"),
	[PermissionName.From("sample.Scheduling.RescheduleAppointment")],
	CapabilityContinuationToken.Empty);

var snapshotResult = await cache.GetSnapshotAsync(request, cancellationToken);
if (snapshotResult.IsSuccess && snapshotResult.Value.CanExecute(PermissionName.From("sample.Scheduling.RescheduleAppointment")))
{
	// Show the action affordance.
}
```

When an application-service call returns `AuthorizationDeniedError`, call `HandleAuthorizationFailureAsync(...)` to invalidate the cached entry for the request key and fetch a fresh snapshot version.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
