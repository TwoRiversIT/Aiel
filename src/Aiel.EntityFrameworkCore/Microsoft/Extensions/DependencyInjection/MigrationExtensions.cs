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
using Microsoft.Extensions.Hosting;
using Aiel.EntityFrameworkCore.Migrations;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Extension methods for registering and invoking Aiel migration infrastructure.</summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Runs all registered <see cref="IDatabaseMigrator"/> types against the host's service provider.
    /// </summary>
    /// <param name="host">The application host.</param>
    /// <param name="cancellationToken">Token used to cancel the migration.</param>
    public static async Task MigrateAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);

        await host.Services.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// Runs all registered <see cref="IDatabaseMigrator"/> types against
    /// <paramref name="serviceProvider"/>.
    /// </summary>
    /// <param name="serviceProvider">The root service provider.</param>
    /// <param name="cancellationToken">Token used to cancel the migration.</param>
    public static async Task MigrateAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            await scope.MigrateAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Runs all registered <see cref="IDatabaseMigrator"/> types within <paramref name="scope"/>.
    /// </summary>
    /// <param name="scope">The DI scope to resolve migrators from.</param>
    /// <param name="cancellationToken">Token used to cancel the migration.</param>
    public static async Task MigrateAsync(this IServiceScope scope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var migrationService = scope.ServiceProvider.GetRequiredService<MigrationManager>();

        await migrationService.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// Registers Aiel's discriminator (single-database) migration infrastructure.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">Optional delegate to configure <see cref="AielMigrationOptions"/>.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddAielMigrations(this IServiceCollection services, Action<AielMigrationOptions>? configure = null)
    {
        return services
            .Configure<AielMigrationOptions>(options => configure?.Invoke(options))
            .AddScoped<MigrationManager>();
    }

    /// <summary>
    /// Registers Aiel's out-of-band tenant migration infrastructure, including
    /// <see cref="ITenantMigrationRunner"/> and a default <see cref="NullMigrationTelemetryHook"/>.
    /// </summary>
    /// <remarks>
    /// This method does NOT register an <see cref="ITenantMigrationTargetSource"/>. Callers must
    /// register their own implementation before resolving <see cref="ITenantMigrationRunner"/>.
    /// This method does NOT invoke any migration — call
    /// <see cref="ITenantMigrationRunner.ResumeAsync"/> explicitly from a CLI tool or background job.
    /// </remarks>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddAielTenantMigrations(this IServiceCollection services)
    {
        services.TryAddScoped<ITenantMigrationRunner, TenantMigrationRunner>();
        services.TryAddSingleton<IMigrationTelemetryHook, NullMigrationTelemetryHook>();
        return services;
    }
}
