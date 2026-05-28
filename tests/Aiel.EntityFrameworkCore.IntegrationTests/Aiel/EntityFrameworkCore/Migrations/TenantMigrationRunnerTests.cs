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
using Microsoft.Extensions.Logging.Abstractions;

namespace Aiel.EntityFrameworkCore.Migrations;

public sealed class TenantMigrationRunnerTests
{
    // ---------------------------------------------------------------------------
    // Shared test fixtures
    // ---------------------------------------------------------------------------

    private sealed record TestTarget(TenantMigrationKey Key, TenantMigrationLabel Label) : ITenantMigrationTarget;

    private static TestTarget MakeTarget(String key, String label = "")
        => new(new TenantMigrationKey(key), new TenantMigrationLabel(label.Length > 0 ? label : key));

    private sealed class StubTargetSource(IEnumerable<ITenantMigrationTarget> targets) : ITenantMigrationTargetSource
    {
        public async IAsyncEnumerable<ITenantMigrationTarget> GetTargetsAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var target in targets)
            {
                yield return target;
                await Task.Yield();
            }
        }
    }

    private sealed class RecordingMigrator : ITargetedDatabaseMigrator
    {
        public List<TenantMigrationKey> Migrated { get; } = [];
        public Task MigrateAsync(ITenantMigrationTarget target, CancellationToken cancellationToken = default)
        {
            Migrated.Add(target.Key);
            return Task.CompletedTask;
        }
    }

    private sealed class FailingMigrator(params TenantMigrationKey[] failKeys) : ITargetedDatabaseMigrator
    {
        private readonly HashSet<TenantMigrationKey> _failKeys = [.. failKeys];

        public List<TenantMigrationKey> Attempted { get; } = [];

        public Task MigrateAsync(ITenantMigrationTarget target, CancellationToken cancellationToken = default)
        {
            Attempted.Add(target.Key);
            if (_failKeys.Contains(target.Key))
            {
                throw new InvalidOperationException("Simulated migration failure.");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class RecordingTelemetryHook : IMigrationTelemetryHook
    {
        public List<TenantMigrationKey> Started { get; } = [];
        public List<TenantMigrationKey> Completed { get; } = [];
        public List<MigrationFailedTarget> Failed { get; } = [];

        public void OnStarted(ITenantMigrationTarget target) => Started.Add(target.Key);
        public void OnCompleted(ITenantMigrationTarget target) => Completed.Add(target.Key);
        public void OnFailed(ITenantMigrationTarget target, MigrationFailedTarget failure) => Failed.Add(failure);
    }

    private static TenantMigrationRunner MakeRunner(
        ITargetedDatabaseMigrator migrator,
        ITenantMigrationTargetSource source,
        IMigrationTelemetryHook? hook = null)
        => new(migrator, source, hook ?? new NullMigrationTelemetryHook(), NullLogger<TenantMigrationRunner>.Instance);

    // ---------------------------------------------------------------------------
    // ResumeAsync — basic run
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ResumeAsync_with_empty_checkpoint_migrates_all_targets()
    {
        var t1 = MakeTarget("t1");
        var t2 = MakeTarget("t2");
        var migrator = new RecordingMigrator();
        var runner = MakeRunner(migrator, new StubTargetSource([t1, t2]));

        var result = await runner.ResumeAsync(MigrationCheckpoint.Empty, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ResumeAsync_skips_targets_already_in_checkpoint()
    {
        var t1 = MakeTarget("t1");
        var t2 = MakeTarget("t2");
        var t3 = MakeTarget("t3");
        var migrator = new RecordingMigrator();
        var checkpoint = new MigrationCheckpoint([t2.Key]);
        var runner = MakeRunner(migrator, new StubTargetSource([t1, t2, t3]));

        var result = await runner.ResumeAsync(checkpoint, TestContext.Current.CancellationToken);

        Assert.Equal([t1.Key, t3.Key], result.Succeeded);
        Assert.Empty(result.Failed);
        Assert.DoesNotContain(t2.Key, migrator.Migrated);
    }

    [Fact]
    public async Task ResumeAsync_skips_all_targets_when_checkpoint_is_complete()
    {
        var t1 = MakeTarget("t1");
        var t2 = MakeTarget("t2");
        var migrator = new RecordingMigrator();
        var checkpoint = new MigrationCheckpoint([t1.Key, t2.Key]);
        var runner = MakeRunner(migrator, new StubTargetSource([t1, t2]));

        var result = await runner.ResumeAsync(checkpoint, TestContext.Current.CancellationToken);

        Assert.Empty(result.Succeeded);
        Assert.Empty(result.Failed);
        Assert.Empty(migrator.Migrated);
    }

    [Fact]
    public async Task ResumeAsync_returns_empty_result_for_empty_source()
    {
        var migrator = new RecordingMigrator();
        var runner = MakeRunner(migrator, new StubTargetSource([]));

        var result = await runner.ResumeAsync(MigrationCheckpoint.Empty, TestContext.Current.CancellationToken);

        Assert.Empty(result.Succeeded);
        Assert.Empty(result.Failed);
        Assert.False(result.HasFailures);
    }

    // ---------------------------------------------------------------------------
    // ResumeAsync — failure isolation (continue-past-failure)
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ResumeAsync_continues_past_individual_failure_and_records_it()
    {
        var t1 = MakeTarget("t1");
        var t2 = MakeTarget("t2", "Tenant Two");
        var t3 = MakeTarget("t3");
        var migrator = new FailingMigrator(t2.Key);
        var runner = MakeRunner(migrator, new StubTargetSource([t1, t2, t3]));

        var result = await runner.ResumeAsync(MigrationCheckpoint.Empty, TestContext.Current.CancellationToken);

        Assert.Equal([t1.Key, t3.Key], result.Succeeded);
        Assert.Single(result.Failed);
        Assert.True(result.HasFailures);
        Assert.Equal(t2.Key, result.Failed[0].Key);
        Assert.Equal(t2.Label, result.Failed[0].Label);
        Assert.Equal(nameof(InvalidOperationException), result.Failed[0].ExceptionTypeName);
        Assert.Equal([t1.Key, t2.Key, t3.Key], migrator.Attempted);
    }

    [Fact]
    public async Task ResumeAsync_all_failures_gives_HasFailures_true_and_empty_succeeded()
    {
        var t1 = MakeTarget("t1");
        var t2 = MakeTarget("t2");
        var migrator = new FailingMigrator(t1.Key, t2.Key);
        var runner = MakeRunner(migrator, new StubTargetSource([t1, t2]));

        var result = await runner.ResumeAsync(MigrationCheckpoint.Empty, TestContext.Current.CancellationToken);

        Assert.Empty(result.Succeeded);
        Assert.Equal(2, result.Failed.Count);
        Assert.True(result.HasFailures);
    }

    // ---------------------------------------------------------------------------
    // Telemetry hook calls
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ResumeAsync_calls_OnStarted_and_OnCompleted_for_each_success()
    {
        var t1 = MakeTarget("t1");
        var t2 = MakeTarget("t2");
        var hook = new RecordingTelemetryHook();
        var runner = MakeRunner(new RecordingMigrator(), new StubTargetSource([t1, t2]), hook);

        await runner.ResumeAsync(MigrationCheckpoint.Empty, TestContext.Current.CancellationToken);

        Assert.Equal([t1.Key, t2.Key], hook.Started);
        Assert.Equal([t1.Key, t2.Key], hook.Completed);
        Assert.Empty(hook.Failed);
    }

    [Fact]
    public async Task ResumeAsync_calls_OnFailed_not_OnCompleted_for_failing_target()
    {
        var t1 = MakeTarget("t1");
        var hook = new RecordingTelemetryHook();
        var runner = MakeRunner(new FailingMigrator(t1.Key), new StubTargetSource([t1]), hook);

        await runner.ResumeAsync(MigrationCheckpoint.Empty, TestContext.Current.CancellationToken);

        Assert.Single(hook.Started);
        Assert.Empty(hook.Completed);
        Assert.Single(hook.Failed);
        Assert.Equal(t1.Key, hook.Failed[0].Key);
    }

    [Fact]
    public async Task ResumeAsync_does_not_call_OnStarted_for_checkpoint_skipped_targets()
    {
        var t1 = MakeTarget("t1");
        var hook = new RecordingTelemetryHook();
        var checkpoint = new MigrationCheckpoint([t1.Key]);
        var runner = MakeRunner(new RecordingMigrator(), new StubTargetSource([t1]), hook);

        await runner.ResumeAsync(checkpoint, TestContext.Current.CancellationToken);

        Assert.Empty(hook.Started);
        Assert.Empty(hook.Completed);
        Assert.Empty(hook.Failed);
    }

    // ---------------------------------------------------------------------------
    // AddAielTenantMigrations DI registration
    // ---------------------------------------------------------------------------

    [Fact]
    public void AddAielTenantMigrations_registers_ITenantMigrationRunner()
    {
        var services = new ServiceCollection()
            .AddAielTenantMigrations()
            .AddSingleton<ITargetedDatabaseMigrator>(new RecordingMigrator())
            .AddSingleton<ITenantMigrationTargetSource>(new StubTargetSource([]))
            .AddLogging();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var runner = scope.ServiceProvider.GetService<ITenantMigrationRunner>();

        Assert.NotNull(runner);
        Assert.IsType<TenantMigrationRunner>(runner);
    }

    [Fact]
    public void AddAielTenantMigrations_registers_NullMigrationTelemetryHook_as_default()
    {
        var services = new ServiceCollection().AddAielTenantMigrations();
        using var provider = services.BuildServiceProvider();

        var hook = provider.GetService<IMigrationTelemetryHook>();

        Assert.IsType<NullMigrationTelemetryHook>(hook);
    }

    [Fact]
    public void AddAielTenantMigrations_does_not_override_custom_telemetry_hook()
    {
        var custom = new RecordingTelemetryHook();
        var services = new ServiceCollection()
            .AddSingleton<IMigrationTelemetryHook>(custom)
            .AddAielTenantMigrations();

        using var provider = services.BuildServiceProvider();
        var hook = provider.GetService<IMigrationTelemetryHook>();

        Assert.Same(custom, hook);
    }
}
