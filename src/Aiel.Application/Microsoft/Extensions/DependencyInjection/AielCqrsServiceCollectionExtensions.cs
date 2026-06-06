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

using Aiel.Commands;
using Aiel.Domain;
using Aiel.Queries;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Service collection extensions for registering the Aiel CQRS infrastructure.
/// </summary>
public static class AielCqrsServiceCollectionExtensions
{
    /// <summary>
    /// Registers all CQRS infrastructure and scans <paramref name="assemblies"/> for
    /// command, query, and domain event handler implementations.
    /// </summary>
    /// <remarks>
    /// Registers <see cref="ICommandDispatcher"/>, <see cref="IQueryDispatcher"/>, and
    /// <see cref="IDomainEventDispatcher"/> using <c>TryAdd</c> so repeated calls are safe.
    /// Handler implementations are registered as scoped services for each closed interface
    /// they implement.
    /// Pipeline behaviors are intentionally not auto-registered; register them explicitly
    /// after this call to control ordering.
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="assemblies">Assemblies to scan for handler implementations.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddAielCqrs(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ICommandDispatcher, DefaultCommandDispatcher>();
        services.TryAddScoped<IQueryDispatcher, DefaultQueryDispatcher>();
        services.TryAddScoped<IDomainEventDispatcher, DefaultDomainEventDispatcher>();

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract || type.IsGenericTypeDefinition)
                {
                    continue;
                }

                foreach (var iface in type.GetInterfaces())
                {
                    if (!iface.IsGenericType)
                    {
                        continue;
                    }

                    var genericDef = iface.GetGenericTypeDefinition();
                    if (genericDef == typeof(ICommandHandler<>) ||
                        genericDef == typeof(IQueryHandler<,>) ||
                        genericDef == typeof(IDomainEventHandler<>))
                    {
                        services.AddScoped(iface, type);
                    }
                }
            }
        }

        return services;
    }
}
