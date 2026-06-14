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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Aiel.DataAccess.EntityFrameworkCore.Migrations;

public partial class DbContextMigrator<TDbContext>(IServiceProvider serviceProvider)
    : DatabaseMigratorBase, IDatabaseMigrator
    where TDbContext : DbContext
{
    private ILogger? _logger;
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    protected override ILogger Logger => _logger
        ??= ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger<TDbContext>()
        ?? new NullLogger<TDbContext>();

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await TryAsync(ApplyMigrationsAsync, cancellationToken: cancellationToken);
    }

    protected virtual async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = ServiceProvider.GetRequiredService<TDbContext>();
        var name = dbContext.GetType().Name;
        var pendingMigrations = await dbContext
            .Database
            .GetPendingMigrationsAsync(cancellationToken: cancellationToken);

        var count = pendingMigrations.Count();
        if (count > 0)
        {
            Logger.LogMigrationsFound(count);

            try
            {
                Logger.LogMigratingDatabase(name);

                var strategy = dbContext.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(dbContext.Database.MigrateAsync, cancellationToken);
            }
            catch (Exception ex)
            {
                var exName = ex.GetType().Name;
                Logger.LogMigrationFailed(name, exName, ex.Message);
                throw;
            }
        }
        else
        {
            Logger.LogNoMigrationsToApply(name);
        }
    }
}
