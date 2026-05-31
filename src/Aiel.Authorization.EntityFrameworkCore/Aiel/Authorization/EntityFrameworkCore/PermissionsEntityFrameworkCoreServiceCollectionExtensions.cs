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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aiel.Authorization.EntityFrameworkCore;

/// <summary>
/// Extension methods for registering EF Core permission infrastructure services.
/// </summary>
public static class PermissionsEntityFrameworkCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers the EF Core <see cref="IPermissionStore"/> implementation and the
    /// <see cref="PermissionMigrationRunner"/> using the supplied database context configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureOptions">A delegate that configures the <see cref="DbContextOptionsBuilder"/>.</param>
    /// <returns>The same <paramref name="services"/> instance to allow chaining.</returns>
    public static IServiceCollection AddPermissionsEntityFrameworkCore(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.TryAddSingleton(TimeProvider.System);
        services.AddDbContext<PermissionsDbContext>(configureOptions);
        services.AddScoped<IPermissionStore, EfCorePermissionStore>();
        services.AddScoped<PermissionMigrationRunner>();
        services.AddScoped<PermissionsDbInitializer>();

        return services;
    }
}
