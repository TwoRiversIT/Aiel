# Aiel.AspNetCore

ASP.NET Core integration points for Aiel framework features.

## Tenant resolution pipeline

`Aiel.AspNetCore` now exposes two pipeline helpers for multitenant HTTP applications:

```csharp
builder.Services.AddAielTenantAccess();

var app = builder.Build();

app.UseAielTenantResolution();

app.MapGet("/tenant-required", handler).RequireTenant();
app.MapGet("/tenant-optional", handler);
```

### Behavior

- `UseAielTenantResolution()` resolves `ITenantResolver` once per request.
- `RequireTenant()` marks an endpoint as fail-closed for unresolved tenant outcomes.
- Endpoints without `RequireTenant()` remain tenant-optional and continue to the handler.
- `GetTenantResolution()` exposes the per-request `TenantResolution` through `HttpContext`.
- `AddAielTenantAccess()` registers an ASP.NET Core-backed `ITenantAccessor` for handlers that only run on resolved-tenant paths.
- `X-Tenant-ID` conflicts with an already resolved tenant short-circuit with `409 Conflict`.

### Downstream access

```csharp
app.MapGet("/tenant-info", async Task<IResult> (
    HttpContext context,
    ITenantAccessor tenantAccessor,
    CancellationToken cancellationToken) =>
{
    var tenantResolution = context.GetTenantResolution();

    if (tenantResolution is TenantResolution.Resolved)
    {
        var tenantIdentity = await tenantAccessor.GetCurrentTenantAsync(cancellationToken);
        return TypedResults.Ok(tenantIdentity.TenantId);
    }

    return tenantResolution switch
    {
        TenantResolution.Missing => TypedResults.Unauthorized(),
        TenantResolution.Ambiguous => TypedResults.BadRequest(),
        TenantResolution.Rejected => TypedResults.Forbid(),
        TenantResolution.Error => TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable),
        _ => TypedResults.StatusCode(StatusCodes.Status500InternalServerError)
    };
});
```

`GetTenantResolution()` is the safe read path for all five resolution outcomes. `ITenantAccessor`
is reserved for handlers that already require a resolved tenant and should fail closed otherwise.

### Tenant-required status mapping

| Resolution outcome | Status code |
| --- | --- |
| `Resolved` | `200 OK` |
| `Missing` | `401 Unauthorized` |
| `Ambiguous` | `400 Bad Request` |
| `Rejected` | `403 Forbidden` |
| `Error` | `503 Service Unavailable` |

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
