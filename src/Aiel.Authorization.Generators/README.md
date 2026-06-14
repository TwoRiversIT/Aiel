# Aiel.Authorization.Generators

Source generator for the `Aiel.Authorization` framework.  For more information, see the [Aiel.Authorization](https://github.com/TwoRiversIT/Aiel/blob/main/src/Aiel.Authorization/README.md) documentation.

## Usage

Consumes `[AuthorizationDefinition]` attributes on action classes and emits:

- A strongly-typed `IActionAuthorizationChecker<TAction>` implementation per action.
- `GeneratedAuthorizationNames`, a `partial static class` of `const string` permission name constants.
- `GeneratedAuthorizationManifests.GetManifests()`, an enumerable of `AuthorizationDefinitionManifest` values for registration with `IPermissionDefinitionRegistry` at startup.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
