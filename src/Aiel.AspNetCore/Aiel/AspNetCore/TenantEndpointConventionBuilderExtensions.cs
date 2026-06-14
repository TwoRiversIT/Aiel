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
using Microsoft.AspNetCore.Http;

namespace Aiel.AspNetCore;

/// <summary>
/// Adds tenant enforcement metadata to ASP.NET Core endpoints.
/// </summary>
public static class TenantEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Marks an endpoint as requiring a resolved tenant before its handler may execute.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The builder to configure.</param>
    /// <returns>The same <paramref name="builder"/> instance.</returns>
    public static TBuilder RequireTenant<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Add(static endpointBuilder =>
        {
            endpointBuilder.Metadata.Add(TenantRequirementMetadata.Required);
            endpointBuilder.FilterFactories.Add(static (_, next) => async invocationContext =>
            {
                if (!HttpContextTenantResolutionExtensions.TryGetTenantResolutionFeature(
                        invocationContext.HttpContext,
                        out var tenantResolutionFeature))
                {
                    return Microsoft.AspNetCore.Http.Results.StatusCode(StatusCodes.Status500InternalServerError);
                }

                var tenantResolution = tenantResolutionFeature.Resolution;
                if (TenantResolutionRequirementPolicy.AllowsEndpointExecution(tenantResolution))
                {
                    return await next(invocationContext);
                }

                return Microsoft.AspNetCore.Http.Results.StatusCode(TenantResolutionRequirementPolicy.MapStatusCode(tenantResolution));
            });
        });

        return builder;
    }
}
