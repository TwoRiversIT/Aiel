# Aiel.StrongIds.AspNetCore

ASP.NET Core binding helpers for `Aiel.StrongIds`. For more information, see the [Aiel.StrongIds](https://github.com/TwoRiversIT/Aiel/blob/main/src/Aiel.StrongIds/README.md) documentation.

## Usage

```csharp
using Aiel.StrongIds.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStrongIdTypeConvertersFromAssemblyContaining<OrderId>();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.ConfigureForStrongIds());
```

Generated Strong IDs now expose `TryParse(string?, IFormatProvider?, out TStrongId)` for minimal API route and query binding. `AddStrongIdTypeConvertersFromAssemblyContaining<T>()` registers runtime `TypeConverter` support for MVC-style simple type binding and other `TypeDescriptor`-based consumers.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
