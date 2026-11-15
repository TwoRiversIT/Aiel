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

using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Aiel.Timing;

public interface ISessionStopwatch
{
    IMarkingStopwatch this[String sessionName] { get; }
    IReadOnlyList<StopwatchSession> Sessions { get; }
}

public sealed class SessionStopwatch(TimeProvider? timeProvider = null)
    : DisposableBase, ISessionStopwatch
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    // ConcurrentDictionary gives us lock-free reads and safe writes
    private readonly ConcurrentDictionary<String, StopwatchSession> _sessions = new(StringComparer.OrdinalIgnoreCase);

    public IMarkingStopwatch this[String sessionName]
    {
        get
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(sessionName);

            var session = _sessions.GetOrAdd(sessionName, static (sn, tp)
                => new StopwatchSession(sn, new MarkingStopwatch(tp), tp.GetTimestamp()), _timeProvider);

            return session.Stopwatch;
        }
    }

    /// <summary>
    /// Returns a snapshot of the current sessions in an unspecified order.
    /// </summary>
    public IReadOnlyList<StopwatchSession> Sessions
        => _sessions.Values.ToArray();

    protected override async ValueTask DisposeAsyncCore()
    {
        foreach (var session in _sessions.Values)
        {
            if (session.Stopwatch is IAsyncDisposable asyncSw)
            {
                await asyncSw.DisposeAsync();
            }
            else if (session.Stopwatch is IDisposable syncSw)
            {
                syncSw.Dispose();
            }
        }
    }
}

/// <summary>
/// Represents a single stopwatch session with its name, stopwatch instance, and the timestamp when it was created.
/// </summary>
/// <param name="Name">The name of the session.</param>
/// <param name="Stopwatch">The stopwatch instance associated with the session.</param>
/// <param name="Timestamp">The timestamp when the session was created.</param>
public sealed record StopwatchSession(String Name, IMarkingStopwatch Stopwatch, Int64 Timestamp) : IMarkingStopwatch
{
    public TimeSpan Elapsed => Stopwatch.Elapsed;

    public IReadOnlyList<Mark> Marks => Stopwatch.Marks;
    public Boolean IsRunning => Stopwatch.IsRunning;

    /// <summary>
    /// Convenience method to start the stopwatch.
    /// </summary>
    /// <param name="description"></param>
    public void Start(String? description) => Stopwatch.Start(description);
    public void Mark(String? description) => Stopwatch.Mark(description);
    public void Stop(String? description) => Stopwatch.Stop(description);

    public void Reset()
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<Mark> GetMarksWithDeltas()
    {
        throw new NotImplementedException();
    }
}

public static class SessionStopwatchExtensions
{

    public static IReadOnlyList<StopwatchSession> GetSessionsOrderedByName(this ISessionStopwatch sessionStopwatch)
    {
        return sessionStopwatch.Sessions
            .OrderBy(s => s.Name)
            .ToArray();
    }

    public static IReadOnlyList<StopwatchSession> GetSessionsOrderedByStartTime(this ISessionStopwatch sessionStopwatch)
    {
        return sessionStopwatch.Sessions
            .OrderBy(s => s.Timestamp)
            .ToArray();
    }

    // -----------------------------
    // MARKDOWN EXPORT
    // -----------------------------
    public static String ExportAllAsMarkdown(this ISessionStopwatch instance, String? timeSpanFormat = null)
    {
        var sb = new StringBuilder();

        foreach (var session in instance.Sessions.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase))
        {

            sb.AppendLine($"## Session: {EscapeMarkdown(session.Name)}");
            sb.AppendLine();
            sb.AppendLine(session.Stopwatch.GetAsMarkdown(timeSpanFormat));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    // -----------------------------
    // CSV EXPORT
    // -----------------------------
    public static String ExportAllAsCsv(this ISessionStopwatch instance)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Session,Index,Elapsed,Delta,Description,TimestampUtc");

        foreach (var session in instance.Sessions.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase))
        {
            foreach (var m in session.Stopwatch.GetMarksWithDeltas())
            {
                var desc = EscapeCsv(m.Description);
                sb.AppendLine($"{EscapeCsv(session.Name)},{m.Index},{m.Elapsed},{m.Delta},{desc},{m.WallTime:O}");
            }
        }

        return sb.ToString();
    }

    // -----------------------------
    // JSON EXPORT
    // -----------------------------
    public static String ExportAllAsJson(this ISessionStopwatch instance, JsonSerializerOptions? options = null)
    {
        var export = instance.Sessions
            .OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                kvp => kvp.Name,
                kvp => kvp.Stopwatch.GetMarksWithDeltas()
            );

        return JsonSerializer.Serialize(export, options ?? new JsonSerializerOptions { WriteIndented = true });
    }

    // -----------------------------
    // Helpers
    // -----------------------------
    private static String EscapeMarkdown(String s)
        => s.Replace("|", "\\|").Replace("\r", "").Replace("\n", " ");

    private static String EscapeCsv(String s)
    {
        if (String.IsNullOrWhiteSpace(s))
        {
            return String.Empty;
        }

        var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
        var escaped = s.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }
}
