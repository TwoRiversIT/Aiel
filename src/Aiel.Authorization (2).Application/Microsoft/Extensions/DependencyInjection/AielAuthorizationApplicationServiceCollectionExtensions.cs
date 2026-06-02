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

using Microsoft.Extensions.DependencyInjection.Extensions;
using Aiel.Authorization;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Service collection extensions for registering Aiel.Authorization.Application services.
/// </summary>
public static class AielAuthorizationApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the permissions application services required to run the command authorization gate.
    /// </summary>
    /// <remarks>
    /// Registers <see cref="IActionGate{TAction}"/> as <see cref="DefaultActionGate{TAction}"/> (open-generic,
    /// transient) and <see cref="IPermissionManager"/> as <see cref="DefaultPermissionManager"/> (scoped).
    /// Uses <c>TryAdd</c> so repeated calls are safe.
    /// <para>
    /// After calling this method, register pipeline behaviors explicitly in the order you want them to run:
    /// <code>
    /// services.AddAielAuthorizationApplication();
    /// services.AddTransient(typeof(ICommandPipelineBehavior&lt;&gt;), typeof(ActionGateCommandPipelineBehavior&lt;&gt;));
    /// </code>
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddAielAuthorizationApplication(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient(typeof(IActionGate<>), typeof(DefaultActionGate<>));
        services.TryAddScoped<IPermissionManager, DefaultPermissionManager>();

        return services;
    }
}
