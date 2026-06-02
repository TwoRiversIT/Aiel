# Aiel.Authorization.Generators

Source generator for the `Aiel.Authorization` framework. Consumes `[AuthorizationDefinition]` attributes on action classes and emits:

- A strongly-typed `IActionAuthorizationChecker<TAction>` implementation per action.
- `GeneratedAuthorizationNames` — a `partial static class` of `const string` permission name constants.
- `GeneratedAuthorizationManifests.GetManifests()` — an enumerable of `AuthorizationDefinitionManifest` values for registration with `IPermissionDefinitionRegistry` at startup.

The generated checker satisfies the `ActionAuthorizationAnalyzer` (TRPA0001) condition 1 automatically, so no separate handwritten checker is required for permission-protected actions.
