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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Aiel.EntityFrameworkCore.Migrations;
using System.Reflection;

namespace Aiel.EntityFrameworkCore.Migrations;

/// <summary>
/// Red gate for issue #19: Aiel.EntityFrameworkCore migration primitives.
/// All tests in this class must FAIL until the types they reference are implemented.
/// </summary>
/// <remarks>
/// These tests are contract assertions — they verify that the correct abstractions exist
/// in the migrations assembly. They use reflection so the test project continues to compile
/// while the production types are absent. Each failure message names exactly what to build.
///
/// Out of scope for these tests: Aviendha catalog tables, tenant provisioning workflow,
/// admin UI/CLI, and ADR #15 storage ownership.
/// </remarks>
public sealed class MigrationPrimitivesIssue19ContractTests
{
    private static readonly Assembly MigrationsAssembly = typeof(MigrationManager).Assembly;

    // -------------------------------------------------------------------------
    // Type existence — interface and data contract layer
    // -------------------------------------------------------------------------

    [Fact]
    public void Issue19_requires_ITenantMigrationTarget_in_migrations_namespace()
    {
        var type = MigrationsAssembly.GetType("Aiel.EntityFrameworkCore.Migrations.ITenantMigrationTarget");

        type.Should().NotBeNull(
            "issue #19 requires an ITenantMigrationTarget abstraction so runners receive opaque " +
            "tenant migration units without baking Aviendha catalog knowledge into Aiel.");
        type!.IsInterface.Should().BeTrue("ITenantMigrationTarget must be an interface.");
    }

    [Fact]
    public void Issue19_ITenantMigrationTarget_must_not_expose_raw_string_properties()
    {
        var type = MigrationsAssembly.GetType("Aiel.EntityFrameworkCore.Migrations.ITenantMigrationTarget");

        type.Should().NotBeNull("ITenantMigrationTarget must exist before its shape can be validated.");

        if (type is null)
        {
            return;
        }

        var stringProperties = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(static p => p.PropertyType == typeof(String))
            .ToArray();

        stringProperties.Should().BeEmpty(
            "ITenantMigrationTarget must not expose raw strings that could carry connection strings or other " +
            "secrets; use typed identifiers and opaque handles instead.");
    }

    [Fact]
    public void Issue19_requires_ITenantMigrationTargetSource_in_migrations_namespace()
    {
        var type = MigrationsAssembly.GetType("Aiel.EntityFrameworkCore.Migrations.ITenantMigrationTargetSource");

        type.Should().NotBeNull(
            "issue #19 requires an ITenantMigrationTargetSource interface that out-of-band runners " +
            "consume explicitly; Aviendha wires a catalog-backed implementation, Aiel owns only the contract.");
        type!.IsInterface.Should().BeTrue("ITenantMigrationTargetSource must be an interface.");
    }

    [Fact]
    public void Issue19_requires_MigrationCheckpoint_type_for_resume_behavior()
    {
        var type = MigrationsAssembly.GetType("Aiel.EntityFrameworkCore.Migrations.MigrationCheckpoint");

        type.Should().NotBeNull(
            "issue #19 requires a MigrationCheckpoint type so out-of-band runners can resume " +
            "from a known position after partial failure or restart.");
    }

    [Fact]
    public void Issue19_requires_MigrationFailedTarget_type_for_failed_target_handling()
    {
        var type = MigrationsAssembly.GetType("Aiel.EntityFrameworkCore.Migrations.MigrationFailedTarget");

        type.Should().NotBeNull(
            "issue #19 requires a MigrationFailedTarget record so runners can continue past " +
            "individual tenant failures and report the complete set of failed targets.");
    }

    [Fact]
    public void Issue19_MigrationFailedTarget_must_not_expose_raw_connection_string()
    {
        var type = MigrationsAssembly.GetType("Aiel.EntityFrameworkCore.Migrations.MigrationFailedTarget");

        type.Should().NotBeNull("MigrationFailedTarget must exist before its shape can be validated.");

        if (type is null)
        {
            return;
        }

        var stringProperties = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(static p => p.PropertyType == typeof(String))
            .Select(static p => p.Name)
            .ToArray();

        stringProperties.Should().NotContain(
            static name => name.Contains("Connection", StringComparison.OrdinalIgnoreCase)
                        || name.Contains("Password", StringComparison.OrdinalIgnoreCase)
                        || name.Contains("Secret", StringComparison.OrdinalIgnoreCase)
                        || name.Contains("Credential", StringComparison.OrdinalIgnoreCase),
            "MigrationFailedTarget must not leak connection strings or secrets in its public surface.");
    }

    [Fact]
    public void Issue19_requires_IMigrationReadinessContributor_for_health_check_integration()
    {
        var type = MigrationsAssembly.GetType("Aiel.EntityFrameworkCore.Migrations.IMigrationReadinessContributor");

        type.Should().NotBeNull(
            "issue #19 requires an IMigrationReadinessContributor contract so infrastructure " +
            "and health-check consumers can determine when all migrations have completed and " +
            "the application is safe to accept traffic.");
        type!.IsInterface.Should().BeTrue("IMigrationReadinessContributor must be an interface.");
    }

    [Fact]
    public void Issue19_requires_IMigrationTelemetryHook_for_observable_per_target_events()
    {
        var type = MigrationsAssembly.GetType("Aiel.EntityFrameworkCore.Migrations.IMigrationTelemetryHook");

        type.Should().NotBeNull(
            "issue #19 requires an IMigrationTelemetryHook seam so callers can observe per-target " +
            "migration events without the hook surface leaking connection strings or secrets.");
        type!.IsInterface.Should().BeTrue("IMigrationTelemetryHook must be an interface.");
    }

    [Fact]
    public void Issue19_IMigrationTelemetryHook_methods_must_not_accept_raw_string_parameters()
    {
        var type = MigrationsAssembly.GetType("Aiel.EntityFrameworkCore.Migrations.IMigrationTelemetryHook");

        type.Should().NotBeNull("IMigrationTelemetryHook must exist before its parameter surface can be validated.");

        if (type is null)
        {
            return;
        }

        var suspiciousParameters = type
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .SelectMany(static m => m.GetParameters())
            .Where(static p => p.ParameterType == typeof(String))
            .Select(static p => p.Name ?? "<unnamed>")
            .ToArray();

        suspiciousParameters.Should().BeEmpty(
            "telemetry hook parameters must not accept raw strings that could inadvertently carry " +
            "connection strings or other secrets through the observability seam.");
    }

    // -------------------------------------------------------------------------
    // Out-of-band runner contract
    // -------------------------------------------------------------------------

    [Fact]
    public void Issue19_requires_ITenantMigrationRunner_interface_for_out_of_band_execution()
    {
        var type = MigrationsAssembly.GetType("Aiel.EntityFrameworkCore.Migrations.ITenantMigrationRunner");

        type.Should().NotBeNull(
            "issue #19 requires a dedicated ITenantMigrationRunner interface so the out-of-band runner " +
            "is distinct from normal web startup; callers must opt in explicitly by resolving this contract.");
        type!.IsInterface.Should().BeTrue("ITenantMigrationRunner must be an interface.");
    }

    [Fact]
    public void Issue19_ITenantMigrationRunner_must_expose_an_explicit_resume_method_accepting_MigrationCheckpoint()
    {
        var runnerType = MigrationsAssembly.GetType("Aiel.EntityFrameworkCore.Migrations.ITenantMigrationRunner");
        var checkpointType = MigrationsAssembly.GetType("Aiel.EntityFrameworkCore.Migrations.MigrationCheckpoint");

        runnerType.Should().NotBeNull("ITenantMigrationRunner must exist.");
        checkpointType.Should().NotBeNull("MigrationCheckpoint must exist.");

        if (runnerType is null || checkpointType is null)
        {
            return;
        }

        var methods = runnerType.GetMethods(BindingFlags.Instance | BindingFlags.Public);

        var resumeMethod = methods.FirstOrDefault(static m =>
            m.Name.Contains("Resume", StringComparison.OrdinalIgnoreCase));

        resumeMethod.Should().NotBeNull(
            "the out-of-band runner must expose an explicit resume entry point; " +
            "runners must not silently restart from the beginning after partial failure.");

        if (resumeMethod is null)
        {
            return;
        }

        resumeMethod.GetParameters()
            .Should().Contain(
                p => p.ParameterType == checkpointType,
                "the resume method must accept a MigrationCheckpoint so the caller controls " +
                "exactly where execution restarts.");
    }

    // -------------------------------------------------------------------------
    // Startup isolation: normal web startup must not enumerate tenant targets
    // -------------------------------------------------------------------------

    [Fact]
    public void Issue19_AielMigrationOptions_must_have_an_optional_TenantTargetSource_property()
    {
        var optionsType = typeof(AielMigrationOptions);

        var property = optionsType.GetProperty(
            "TenantTargetSource",
            BindingFlags.Instance | BindingFlags.Public);

        property.Should().NotBeNull(
            "issue #19 requires AielMigrationOptions to carry an optional ITenantMigrationTargetSource " +
            "so normal web startup leaves it null and out-of-band runners configure it explicitly; " +
            "this is the seam that prevents implicit tenant enumeration during normal startup.");
    }

    [Fact]
    public void Issue19_TenantTargetSource_must_be_null_by_default_on_freshly_constructed_options()
    {
        var optionsType = typeof(AielMigrationOptions);

        var property = optionsType.GetProperty(
            "TenantTargetSource",
            BindingFlags.Instance | BindingFlags.Public);

        property.Should().NotBeNull("TenantTargetSource must exist on AielMigrationOptions.");

        if (property is null)
        {
            return;
        }

        var options = new AielMigrationOptions();
        var value = property.GetValue(options);

        value.Should().BeNull(
            "TenantTargetSource must be null by default so that normal web startup does not " +
            "enumerate tenant targets; tenant migration is opt-in for out-of-band runners only.");
    }

    [Fact]
    public void Issue19_AddAielMigrations_does_not_register_a_TenantTargetSource_by_default()
    {
        var services = new ServiceCollection();
        services.AddAielMigrations();
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<AielMigrationOptions>>().Value;
        var optionsType = typeof(AielMigrationOptions);

        var property = optionsType.GetProperty(
            "TenantTargetSource",
            BindingFlags.Instance | BindingFlags.Public);

        property.Should().NotBeNull("TenantTargetSource must exist so this assertion is meaningful.");

        if (property is null)
        {
            return;
        }

        property.GetValue(options).Should().BeNull(
            "AddAielMigrations() must not configure a TenantTargetSource; " +
            "normal web startup must not enumerate tenant targets; " +
            "out-of-band runners opt in explicitly via the options builder.");
    }
}
