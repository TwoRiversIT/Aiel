# Aiel.Authorization.Client.Blazor

Blazor visibility helpers for `Aiel.Authorization.Client` capability snapshots.

## What This Package Contains

- `ActionCapabilityVisibility`
- `CanExecute` component

The default sample posture is hide-on-denial: if the requested permission is not granted in the current snapshot, the child content is not rendered.

## Sample

```razor
<CanExecute Snapshot="@snapshot"
			Permission="@PermissionName.From(\"sample.Scheduling.RescheduleAppointment\")">
	<button @onclick="RescheduleAppointmentAsync">Reschedule appointment</button>
</CanExecute>
```

`ActionCapabilityVisibility.CanExecute(...)` exposes the same decision as pure logic for view-model or component-code checks.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
