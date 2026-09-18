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

using Aiel.Framework.DependencyInjection;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aiel.Authorization;

/// <summary>
/// Ensures that the Aiel.Authorization.Application participates in the dependency graph.
/// </summary>
[DependsOn(typeof(AielAuthorizationApplicationContracts))]
public sealed class AielAuthorizationApplication : AielDependency
{
    /// <summary>
    /// Configures the services required for the Aiel.Authorization.Application to function correctly.
    /// </summary>
    /// <param name="context">The configuration context containing the service collection.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    public override ValueTask ConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        if (context.Services.Find<TypeAdapterConfig>().FirstOrDefault() == null)
        {
            context.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        }

        var config = context.Services.GetRequiredSingleton<TypeAdapterConfig>();

        context.Services.TryAddSingleton(config);
        context.Services.TryAddSingleton<IMapper>(new Mapper(config));

        config.Scan(typeof(AielAuthorizationApplication).Assembly);

        return ValueTask.CompletedTask;
    }
}
