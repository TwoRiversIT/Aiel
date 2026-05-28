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

using Microsoft.AspNetCore.Builder;

namespace Aiel.AspNetCore;

/// <summary>
/// Adds the Aiel tenant resolution middleware to the ASP.NET Core request pipeline.
/// </summary>
/// <remarks>
/// Register this middleware after authentication and before tenant-scoped handlers so
/// the current request's tenant resolution is available to downstream middleware, filters,
/// and handlers. Tenant-required endpoint enforcement occurs at endpoint execution, so this
/// middleware may run before or after routing as long as it runs before the handler.
/// </remarks>
public static class AielTenantResolutionApplicationBuilderExtensions
{
    /// <summary>
    /// Resolves the current request tenant and publishes it for downstream ASP.NET Core consumers.
    /// </summary>
    /// <param name="applicationBuilder">The application builder to configure.</param>
    /// <returns>The same <paramref name="applicationBuilder"/> instance.</returns>
    public static IApplicationBuilder UseAielTenantResolution(this IApplicationBuilder applicationBuilder)
    {
        ArgumentNullException.ThrowIfNull(applicationBuilder);

        return applicationBuilder.UseMiddleware<TenantResolutionMiddleware>();
    }
}
