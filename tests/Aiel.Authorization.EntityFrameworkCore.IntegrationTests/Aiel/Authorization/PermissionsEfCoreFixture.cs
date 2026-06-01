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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Aiel.Authorization.EntityFrameworkCore;
using Aiel.Authorization.Testing;
using Aiel.Testing;
using Testcontainers.PostgreSql;

namespace Aiel.Authorization;

public sealed class PermissionsEfCoreFixture : IntegrationTestFixture
{
    private const String PostgreSqlImage = "postgres:15-alpine";
    private const String PostgreSqlUserName = "postgres";
    private const String PostgreSqlPassword = "postgres";

    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder(PostgreSqlImage)
        .WithDatabase($"permissions_{Guid.NewGuid():N}")
        .WithUsername(PostgreSqlUserName)
        .WithPassword(PostgreSqlPassword)
        .Build();

    private String _connectionString = String.Empty;

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<TimeProvider>(TimeProvider);

        var manifests = new[]
        {
            PermissionTestData.CreateSampleManifest(),
            PermissionTestData.CreateRescheduleAppointmentManifest(),
        };

        services.AddSingleton<IPermissionDefinitionRegistry>(new FakePermissionDefinitionRegistry(manifests));

        services.AddScoped<IPermissionManager, DefaultPermissionManager>();

        services.AddPermissionsNpgsql(GetConnectionString, options => options.EnableRetryOnFailure());
    }

    protected override async ValueTask InitializeFixtureAsync(IServiceProvider services)
    {
        await _postgresContainer.StartAsync(TestContext.Current.CancellationToken);
        _connectionString = _postgresContainer.GetConnectionString();

        var initializer = services.GetRequiredService<PermissionsDbInitializer>();
        await initializer.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsyncCore();
    }

    private String GetConnectionString()
    {
        return String.IsNullOrWhiteSpace(_connectionString)
            ? throw new InvalidOperationException("The PostgreSQL test container has not been started yet.")
            : _connectionString;
    }
}
