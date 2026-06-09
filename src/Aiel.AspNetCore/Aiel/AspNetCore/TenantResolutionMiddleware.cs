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
using Microsoft.AspNetCore.Http;

namespace Aiel.AspNetCore;

internal sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tenantResolver);

        var tenantResolution = await tenantResolver.ResolveAsync(context.RequestAborted);
        context.Features.Set<ITenantResolutionFeature>(new TenantResolutionFeature(tenantResolution));

        if (HasTenantOverrideConflict(context.Request.Headers, tenantResolution))
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            return;
        }

        await _next(context);
    }

    private static Boolean HasTenantOverrideConflict(IHeaderDictionary headers, TenantResolution tenantResolution)
    {
        if (tenantResolution is not TenantResolution.Resolved resolvedResolution)
        {
            return false;
        }

        if (!TryGetTenantOverride(headers, out var overriddenTenantId))
        {
            return false;
        }

        return overriddenTenantId != resolvedResolution.TenantIdentity.TenantId;
    }

    private static Boolean TryGetTenantOverride(IHeaderDictionary headers, out TenantId overriddenTenantId)
    {
        ArgumentNullException.ThrowIfNull(headers);

        overriddenTenantId = default;

        if (!headers.TryGetValue(TenantResolutionConstants.TenantIdOverrideHeaderName, out var headerValues))
        {
            return false;
        }

        if (headerValues.Count != 1)
        {
            return false;
        }

        var headerValue = headerValues[0];
        if (String.IsNullOrWhiteSpace(headerValue))
        {
            return false;
        }

        if (!Guid.TryParse(headerValue, out var tenantIdValue))
        {
            return false;
        }

        return TenantId.TryFrom(tenantIdValue, out overriddenTenantId);
    }
}
