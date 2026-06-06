// -----------------------------------------------------------------------
// SampleCompliant.cs — Template / demo of correct Aiel logging usage
//
// All methods below satisfy AIEL001–AIEL005:
//   AIEL001 ✅  EventId uses (int)AielEventIds.XXX cast
//   AIEL002 ✅  Optional AielEventIds parameter present
//   AIEL003 ✅  Message template begins with "[{EventId}]"
//   AIEL004 ✅  No direct ILogger.Log* calls
//   AIEL005 ✅  Attribute EventId and parameter default agree
// -----------------------------------------------------------------------

using Aiel.Logging.Examples;
using Microsoft.Extensions.Logging;

namespace Aiel.Logging.Template;

/// <summary>
/// Demonstrates fully compliant Aiel logging patterns using
/// <see cref="LoggerMessageAttribute"/>.
/// </summary>
public static partial class SampleLog
{
    // ── Service lifecycle ──────────────────────────────────────────────

    /// <summary>Logs service startup.</summary>
    [LoggerMessage(
        EventId  = (int)AielEventIds.ServiceStart,
        Level    = LogLevel.Information,
        Message  = "[{EventId}] Service started successfully")]
    public static partial void ServiceStarted(
        this ILogger         logger,
        AielEventIds         eventId = AielEventIds.ServiceStart);

    /// <summary>Logs graceful service shutdown.</summary>
    [LoggerMessage(
        EventId  = (int)AielEventIds.ServiceStop,
        Level    = LogLevel.Information,
        Message  = "[{EventId}] Service stopped")]
    public static partial void ServiceStopped(
        this ILogger         logger,
        AielEventIds         eventId = AielEventIds.ServiceStop);

    // ── Request pipeline ──────────────────────────────────────────────

    /// <summary>Logs the start of an incoming request.</summary>
    [LoggerMessage(
        EventId  = (int)AielEventIds.RequestStart,
        Level    = LogLevel.Debug,
        Message  = "[{EventId}] Request {RequestId} received for {Path}")]
    public static partial void RequestReceived(
        this ILogger         logger,
        string               requestId,
        string               path,
        AielEventIds         eventId = AielEventIds.RequestStart);

    /// <summary>Logs successful request completion.</summary>
    [LoggerMessage(
        EventId  = (int)AielEventIds.RequestEnd,
        Level    = LogLevel.Information,
        Message  = "[{EventId}] Request {RequestId} completed in {ElapsedMs}ms with status {StatusCode}")]
    public static partial void RequestCompleted(
        this ILogger         logger,
        string               requestId,
        long                 elapsedMs,
        int                  statusCode,
        AielEventIds         eventId = AielEventIds.RequestEnd);

    /// <summary>Logs a failed request with full exception details.</summary>
    [LoggerMessage(
        EventId  = (int)AielEventIds.RequestError,
        Level    = LogLevel.Error,
        Message  = "[{EventId}] Request {RequestId} failed")]
    public static partial void RequestFailed(
        this ILogger         logger,
        string               requestId,
        Exception            exception,
        AielEventIds         eventId = AielEventIds.RequestError);

    // ── Database operations ───────────────────────────────────────────

    /// <summary>Logs a slow database query warning.</summary>
    [LoggerMessage(
        EventId  = (int)AielEventIds.DbQueryEnd,
        Level    = LogLevel.Warning,
        Message  = "[{EventId}] Slow query on {Table}: {ElapsedMs}ms (threshold {ThresholdMs}ms)")]
    public static partial void SlowQuery(
        this ILogger         logger,
        string               table,
        long                 elapsedMs,
        long                 thresholdMs,
        AielEventIds         eventId = AielEventIds.DbQueryEnd);

    // ── Security ──────────────────────────────────────────────────────

    /// <summary>Logs an authentication failure (no PII in message).</summary>
    [LoggerMessage(
        EventId  = (int)AielEventIds.AuthFailure,
        Level    = LogLevel.Warning,
        Message  = "[{EventId}] Authentication failed for user {UserId} from {IpAddress}")]
    public static partial void AuthFailed(
        this ILogger         logger,
        string               userId,
        string               ipAddress,
        AielEventIds         eventId = AielEventIds.AuthFailure);
}
