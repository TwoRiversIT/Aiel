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

using Microsoft.Extensions.Logging;

namespace Aiel.DataAccess.EntityFrameworkCore.Migrations;

/// <summary>
/// Default <see cref="ITenantMigrationRunner"/> that iterates tenant targets, skips completed
/// ones, and collects individual failures without aborting the run.
/// </summary>
public sealed class TenantMigrationRunner(
    ITargetedDatabaseMigrator migrator,
    ITenantMigrationTargetSource targetSource,
    IMigrationTelemetryHook telemetryHook,
    ILogger<TenantMigrationRunner> logger) : ITenantMigrationRunner
{
    /// <inheritdoc />
    public async Task<TenantMigrationResult> ResumeAsync(
        MigrationCheckpoint checkpoint,
        CancellationToken cancellationToken = default)
    {
        var succeeded = new List<TenantMigrationKey>();
        var failed = new List<MigrationFailedTarget>();

        await foreach (var target in targetSource.GetTargetsAsync(cancellationToken))
        {
            if (checkpoint.IsCompleted(target.Key))
            {
                logger.LogSkippingCompleted(target.Label.Value);
                continue;
            }

            telemetryHook.OnStarted(target);

            try
            {
                await migrator.MigrateAsync(target, cancellationToken);

                succeeded.Add(target.Key);
                telemetryHook.OnCompleted(target);
                logger.LogTenantMigrationCompleted(target.Label.Value);
            }
            catch (Exception ex)
            {
                var failure = new MigrationFailedTarget(target.Key, target.Label, ex.GetType().Name);
                failed.Add(failure);
                telemetryHook.OnFailed(target, failure);
                logger.LogTenantMigrationFailed(target.Label.Value, ex.GetType().Name, ex.Message);
            }
        }

        return new TenantMigrationResult(succeeded, failed);
    }
}
