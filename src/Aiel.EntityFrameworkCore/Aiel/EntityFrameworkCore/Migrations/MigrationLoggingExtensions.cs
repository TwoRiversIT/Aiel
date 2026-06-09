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

namespace Aiel.EntityFrameworkCore.Migrations;

public static partial class MigrationLoggingExtensions
{
    [LoggerMessage(EventId = (Int32)AielEventIds.Migrations_MigrationsFound, Level = LogLevel.Information, Message = "[{EventId}] Migrations Found: {Count}")]
    public static partial void LogMigrationsFound(this ILogger logger, Int32 count, AielEventIds eventId = AielEventIds.Migrations_MigrationsFound);

    [LoggerMessage(EventId = (Int32)AielEventIds.Migrations_ApplyingMigrationsStarted, Level = LogLevel.Information, Message = "Applying Migrations: {DatabaseName}")]
    public static partial void LogApplyingMigrations(this ILogger logger, String databaseName);

    [LoggerMessage(EventId = 1, Level = LogLevel.Critical, Message = "Migrating {DatabaseName} failed: {Exception} - {Message}")]
    public static partial void LogMigrationFailed(this ILogger logger, String databaseName, String exception, String message);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "No migrations to apply: {DatabaseName}")]
    public static partial void LogNoMigrationsToApply(this ILogger logger, String databaseName);

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Migration completed in {ElapsedMilliseconds}ms")]
    public static partial void LogMigrationCompleted(this ILogger logger, long elapsedMilliseconds);

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Skipping {Tenant}: already completed.")]
    public static partial void LogSkippingCompleted(this ILogger logger, String tenant);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Migration completed for {Tenant}.")]
    public static partial void LogTenantMigrationCompleted(this ILogger logger, String tenant);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Migration failed for {Tenant}: {ExceptionType} - {ExceptionMessage}")]
    public static partial void LogTenantMigrationFailed(this ILogger logger, String tenant, String exceptionType, String exceptionMessage);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Starting migrations...")]
    public static partial void LogStartingMigrations(this ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Completed migrations.")]
    public static partial void LogCompletedMigrations(this ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Migrating {Name} database...")]
    public static partial void LogMigratingDatabase(this ILogger logger, String name);

    [LoggerMessage(EventId = (Int32)AielEventIds.Migrations_RetryingMigration, Level = LogLevel.Warning, Message = "[{EventId}] {Message}: The operation will be tried {RetryCount} times more.")]
    public static partial void LogRetryingMigration(this ILogger logger, String message, Int32 retryCount, AielEventIds eventId = AielEventIds.Migrations_RetryingMigration);
}
