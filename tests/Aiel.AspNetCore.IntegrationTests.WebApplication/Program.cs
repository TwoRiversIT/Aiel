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

using Aiel.MultiTenancy;
using System.Diagnostics;

namespace Aiel.AspNetCore;

public sealed class Program
{
    private static async Task Main(String[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        await builder.AddApplicationAsync();
        builder.Services.AddSingleton<ITenantResolver, UnconfiguredTenantResolver>();

        var app = builder.Build();

        // Intentionally runs before routing to prove tenant-required endpoints remain fail-closed.
        app.UseAielTenantResolution();
        app.UseRouting();

        app.MapGet("/tenant-required", () => TypedResults.Text(TestEndpointResponses.TenantRequired)).RequireTenant();
        app.MapGet("/tenant-optional", () => TypedResults.Text(TestEndpointResponses.TenantOptional));
        app.MapGet(
            "/tenant-resolution",
            (HttpContext context) => TypedResults.Text(TestEndpointResponses.DescribeTenantResolution(context.GetTenantResolution())));
        app.MapGet(
            "/tenant-accessor",
            async (ITenantAccessor tenantAccessor, CancellationToken cancellationToken = default) =>
            {
                var tenantIdentity = await tenantAccessor.GetCurrentTenantAsync(cancellationToken);
                return TypedResults.Text(tenantIdentity.TenantId.Value.ToString("D"));
            });

        await app.RunAsync();
    }
}

public static class TestEndpointResponses
{
    public const String TenantRequired = "tenant-required-handler";

    public const String TenantOptional = "tenant-optional-handler";

    public const String TenantResolutionMissing = "tenant-resolution-missing";

    public const String TenantResolutionAmbiguous = "tenant-resolution-ambiguous";

    public static String DescribeTenantResolution(TenantResolution tenantResolution)
    {
        ArgumentNullException.ThrowIfNull(tenantResolution);

        return tenantResolution switch
        {
            TenantResolution.Resolved resolved => resolved.TenantIdentity.TenantId.Value.ToString("D"),
            TenantResolution.Missing => TenantResolutionMissing,
            TenantResolution.Ambiguous => TenantResolutionAmbiguous,
            TenantResolution.Rejected rejected => $"tenant-resolution-rejected:{rejected.Reason}",
            TenantResolution.Error error => $"tenant-resolution-error:{error.Reason}",
            _ => throw new UnreachableException("Unknown tenant resolution outcome.")
        };
    }
}

internal sealed class UnconfiguredTenantResolver : ITenantResolver
{
    public ValueTask<TenantResolution> ResolveAsync(CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(
            "ITenantResolver has not been configured. Replace this service in your WebApplicationFactory before running tests.");
}
