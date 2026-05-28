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

using Microsoft.AspNetCore.Http;
using Aiel.MultiTenancy;
using System.Diagnostics.CodeAnalysis;

namespace Aiel.AspNetCore;

/// <summary>
/// Provides access to the current request's resolved tenant resolution outcome.
/// </summary>
public static class HttpContextTenantResolutionExtensions
{
    private const String MissingTenantResolutionMessage =
        "Tenant resolution is unavailable for the current request. Ensure UseAielTenantResolution() runs before request handlers.";

    /// <summary>
    /// Returns the current request's tenant resolution outcome.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>The non-null tenant resolution outcome for the request.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="AielTenantResolutionApplicationBuilderExtensions.UseAielTenantResolution(Microsoft.AspNetCore.Builder.IApplicationBuilder)"/>
    /// has not populated the current request.
    /// </exception>
    public static TenantResolution GetTenantResolution(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!TryGetTenantResolutionFeature(context, out var tenantResolutionFeature))
        {
            throw new InvalidOperationException(MissingTenantResolutionMessage);
        }

        return tenantResolutionFeature.Resolution;
    }

    internal static Boolean TryGetTenantResolutionFeature(
        HttpContext context,
        [NotNullWhen(true)] out ITenantResolutionFeature? tenantResolutionFeature)
    {
        ArgumentNullException.ThrowIfNull(context);

        tenantResolutionFeature = context.Features.Get<ITenantResolutionFeature>();
        return tenantResolutionFeature is not null;
    }
}
