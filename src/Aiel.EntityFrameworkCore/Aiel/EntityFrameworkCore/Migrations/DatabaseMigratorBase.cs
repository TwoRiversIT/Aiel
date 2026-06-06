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
using System.Diagnostics;

namespace Aiel.EntityFrameworkCore.Migrations;

public abstract class DatabaseMigratorBase
{
    public const String ActivitySourceName = "Migrations";

    private readonly ActivitySource _activitySource = new(ActivitySourceName);
    private readonly Lazy<Random> _random = new(() => new Random());
    private Random Random => _random.Value;

    protected abstract ILogger Logger { get; }

    public async Task TryAsync(Func<CancellationToken, Task> task, Int32 retryCount = 3, CancellationToken cancellationToken = default)
    {
        try
        {
            using var activity = _activitySource.StartActivity("Migrating Database", ActivityKind.Client);

            var sw = Stopwatch.StartNew();

            await task(cancellationToken);

            Logger.LogMigrationCompleted(sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            retryCount--;

            if (retryCount <= 0)
            {
                throw;
            }

            Logger.LogWarning(ex, "{Message}: The operation will be tried {retryCount} times more.",
                ex.Message, retryCount);

            await Task.Delay(Random.Next(5000, 15000), cancellationToken);

            await TryAsync(task, retryCount, cancellationToken);
        }
    }
}

