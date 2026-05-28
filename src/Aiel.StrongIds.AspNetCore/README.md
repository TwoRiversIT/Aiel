# Aiel.StrongIds.AspNetCore

ASP.NET Core binding helpers for `Aiel.StrongIds`.

## Usage

```csharp
using Aiel.StrongIds.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStrongIdTypeConvertersFromAssemblyContaining<OrderId>();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.ConfigureForStrongIds());
```

Generated Strong IDs now expose `TryParse(string?, IFormatProvider?, out TStrongId)` for minimal API route and query binding. `AddStrongIdTypeConvertersFromAssemblyContaining<T>()` registers runtime `TypeConverter` support for MVC-style simple type binding and other `TypeDescriptor`-based consumers.