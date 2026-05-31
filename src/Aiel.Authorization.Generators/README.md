# Aiel.Permissions.Generators

Source generator for the `Aiel.Permissions` framework. Consumes `[DefinesPermission]` attributes on action classes and emits:

- A strongly-typed `IActionPermissionChecker<TAction>` implementation per action.
- `GeneratedPermissionNames` — a `partial static class` of `const string` permission name constants.
- `GeneratedPermissionManifests.GetManifests()` — an enumerable of `PermissionDefinitionManifest` values for registration with `IPermissionDefinitionRegistry` at startup.

The generated checker satisfies the `ActionAuthorizationAnalyzer` (TRPA0001) condition 1 automatically, so no separate handwritten checker is required for permission-protected actions.
