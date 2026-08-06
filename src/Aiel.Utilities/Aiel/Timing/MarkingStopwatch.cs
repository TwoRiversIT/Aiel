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

using Aiel.Framework;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Aiel.Timing;

public interface IMarkingStopwatch
{
    /// <summary>
    /// Gets the total elapsed time since the stopwatch was started.
    /// </summary>
    TimeSpan Elapsed { get; }

    /// <summary>
    /// Gets a value indicating whether the stopwatch is currently running.
    /// </summary>
    Boolean IsRunning { get; }

    /// <summary>
    /// Gets a snapshot of the current marks with no deltas calculated.
    /// </summary>
    IReadOnlyList<Mark> Marks { get; }

    /// <summary>
    /// Starts the stopwatch. If already running, this method has no effect. Marks the start time with an optional description.
    /// </summary>
    /// <param name="description">An optional description for the start mark.</param>
    void Start(String? description = null);

    /// <summary>
    /// Marks the current time with an optional description.
    /// </summary>
    /// <param name="description">An optional description for the mark.</param>
    void Mark(String? description = null);

    /// <summary>
    /// Stops the stopwatch. If not running, this method has no effect. Marks the stop time with an optional description.
    /// </summary>
    /// <param name="description">An optional description for the stop mark.</param>
    void Stop(String? description = null);

    /// <summary>
    /// Resets the stopwatch and clears all marks.
    /// </summary>
    void Reset();

    /// <summary>
    /// Gets a snapshot of the current marks with deltas calculated.
    /// </summary>
    IReadOnlyList<Mark> GetMarksWithDeltas();
}

public sealed class MarkingStopwatch(TimeProvider? timeProvider = null)
    : DisposableBase, IMarkingStopwatch
{
    //private sealed record RawMark(Int64 Timestamp, TimeSpan Elapsed, String Description, DateTimeOffset WallTimeUtc);

    private const String DefaultStarted = "Started";
    private const String DefaultStopped = "Stopped";
    private const String DefaultMark = "Mark";

    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
    private readonly List<Mark> _raw = [];
    private readonly Lock _sync = new();

    private Int64 _startedTimeStamp;
    private Int64 _markedTimeStamp;
    private Int64 _stoppedTimeStamp;

    /// <inheritdoc />
    public Boolean IsRunning { get; private set; }

    /// <inheritdoc />
    public TimeSpan Elapsed
    {
        get
        {
            lock (_sync)
            {
                if (_startedTimeStamp == 0)
                {
                    return TimeSpan.Zero;
                }

                return _timeProvider.GetElapsedTime(_startedTimeStamp);
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<Mark> Marks => GetMarksWithDeltas();

    /// <inheritdoc />
    public void Start(String? description = null)
    {
        lock (_sync)
        {
            if (IsRunning)
            {
                return;
            }

            // Record a start mark at zero or current elapsed if stopwatch was previously used
            _startedTimeStamp = _markedTimeStamp = _timeProvider.GetTimestamp();
            _raw.Add(new Mark(description ?? DefaultStarted, TimeSpan.Zero, _timeProvider.GetUtcNow(), _startedTimeStamp, _raw.Count, TimeSpan.Zero));
            IsRunning = true;
        }
    }

    /// <inheritdoc />
    public void Mark(String? description = null)
    {
        lock (_sync)
        {
            if (_startedTimeStamp == 0)
            {
                // not started: record zero elapsed mark
                _raw.Add(new Mark(description ?? DefaultMark, TimeSpan.Zero, _timeProvider.GetUtcNow(), 0, _raw.Count, TimeSpan.Zero));
                return;
            }

            _markedTimeStamp = _timeProvider.GetTimestamp();
            var elapsed = _timeProvider.GetElapsedTime(_startedTimeStamp);
            _raw.Add(new Mark(description ?? DefaultMark, elapsed, _timeProvider.GetUtcNow(), _markedTimeStamp, _raw.Count, TimeSpan.Zero));
        }
    }

    /// <inheritdoc />
    public void Stop(String? description = null)
    {
        lock (_sync)
        {
            if (!IsRunning)
            {
                return;
            }

            _stoppedTimeStamp = _markedTimeStamp = _timeProvider.GetTimestamp();
            _raw.Add(new Mark(description ?? DefaultStopped, _timeProvider.GetElapsedTime(_startedTimeStamp), _timeProvider.GetUtcNow(), _stoppedTimeStamp, _raw.Count, TimeSpan.Zero));
            IsRunning = false;
        }
    }

    /// <inheritdoc />
    public void Reset()
    {
        lock (_sync)
        {
            _raw.Clear();
            _startedTimeStamp = 0;
            _markedTimeStamp = 0;
            _stoppedTimeStamp = 0;
            IsRunning = false;
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<Mark> GetMarksWithDeltas()
    {
        lock (_sync)
        {
            var list = new List<Mark>(_raw.Count);
            var prev = TimeSpan.Zero;

            for (var i = 0; i < _raw.Count; i++)
            {
                var r = _raw[i];
                var delta = r.Elapsed - prev;
                list.Add(r with { Index = i, Delta = delta });
                prev = r.Elapsed;
            }

            return list.AsReadOnly();
        }
    }

    /// <inheritdoc />
    protected override ValueTask DisposeAsyncCore()
    {
        lock (_sync)
        {
            if (IsRunning)
            {
                _raw.Add(new Mark("Disposed", _timeProvider.GetElapsedTime(_startedTimeStamp), _timeProvider.GetUtcNow(), _timeProvider.GetTimestamp(), _raw.Count, TimeSpan.Zero));
                IsRunning = false;
            }
            // Do not clear marks automatically; caller may want results after disposal.
        }

        return default;
    }
}

/// <summary>
/// Represents a single mark in the stopwatch with its description, elapsed time, wall clock time, timestamp, index, and delta from the previous mark.
/// </summary>
/// <param name="Description"></param>
/// <param name="Elapsed"></param>
/// <param name="WallTime"></param>
/// <param name="Timestamp"></param>
/// <param name="Index"></param>
/// <param name="Delta"></param>
public sealed record Mark(String Description, TimeSpan Elapsed, DateTimeOffset WallTime, Int64 Timestamp, Int32 Index, TimeSpan Delta)
{
    public Mark(TimeSpan elapsed, String description, DateTimeOffset wallTime, Int64 timestamp)
        : this(description, elapsed, wallTime, timestamp, 0, TimeSpan.Zero) { }
}

public static class LoggingStopwatchExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private const String TimeSpanFormat = @"hh\:mm\:ss\.ffff";

    /// <summary>
    /// Returns a Markdown table with escaped description cells and consistent TimeSpan formatting.
    /// </summary>
    public static String GetAsMarkdown(this IMarkingStopwatch stopwatch, String? timeSpanFormat = null)
    {
        var format = String.IsNullOrWhiteSpace(timeSpanFormat) ? TimeSpanFormat : timeSpanFormat;
        var marks = stopwatch.GetMarksWithDeltas(); // capture snapshot to minimize lock duration
        var descLenth = marks.Max(m => EscapeMarkdownCell(m.Description).Length);

        var sb = new StringBuilder();

        sb.AppendLine($"| Index | Elapsed       | Delta         | WallTime (UTC)                   | {PadEnd("Description", descLenth)} |");
        sb.AppendLine($"| ----: | :------------ | :------------ | :-------------------------------- | :{new String('-', descLenth - 1)} |");
        //              |     0 | 00:00:00.0000 | 00:00:00.0000 | 2026-06-17T00:13:27.0959174+00:00 | Creating application host

        for (var i = 0; i < marks.Count; i++)
        {
            sb.AppendLine($"| {PadStart(marks[i].Index.ToString(CultureInfo.InvariantCulture), 5)} | {marks[i].Elapsed.ToString(format)} | {marks[i].Delta.ToString(format)} | {marks[i].WallTime:O} | {PadEnd(EscapeMarkdownCell(marks[i].Description), descLenth)} |");
        }

        return sb.ToString();
    }

    private static String PadEnd(String s, Int32 totalWidth, Char paddingChar = ' ')
    {
        if (s.Length >= totalWidth)
        {
            return s;
        }

        return s + new String(paddingChar, totalWidth - s.Length);
    }

    private static String PadStart(String s, Int32 totalWidth, Char paddingChar = ' ')
    {
        if (s.Length >= totalWidth)
        {
            return s;
        }

        return new String(paddingChar, totalWidth - s.Length) + s;
    }

    /// <summary>
    /// Logs results to xUnit ITestOutputHelper if available, using duck-typing to avoid hard dependency.
    /// </summary>
    /// <param name="stopwatch"></param>
    /// <param name="testOutputHelper"></param>
    public static void LogToXunit(this IMarkingStopwatch stopwatch, Object testOutputHelper)
    {
        ArgumentNullException.ThrowIfNull(stopwatch);
        ArgumentNullException.ThrowIfNull(testOutputHelper);

        // Avoid a hard dependency on xUnit types; use duck-typing to call WriteLine if available.
        var writeLine = testOutputHelper.GetType().GetMethod("WriteLine", [typeof(String)]);
        writeLine?.Invoke(testOutputHelper, [stopwatch.GetAsMarkdown()]);
    }

    /// <summary>
    /// Export marks to CSV (single-line cells, pipes/newlines escaped).
    /// </summary>
    public static String ExportCsv(this IMarkingStopwatch stopwatch)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Index,Elapsed,Delta,Description,TimestampUtc");
        var marks = stopwatch.GetMarksWithDeltas(); // capture snapshot to minimize lock duration
        for (var i = 0; i < marks.Count; i++)
        {
            var m = marks[i];
            var desc = EscapeCsvCell(m.Description);
            sb.AppendLine($"{m.Index},{m.Elapsed.ToString(TimeSpanFormat)},{m.Delta.ToString(TimeSpanFormat)},{desc},{m.WallTime:O}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Export marks to JSON using System.Text.Json.
    /// </summary>
    public static String ExportJson(this IMarkingStopwatch stopwatch, JsonSerializerOptions? options = null)
    {
        var marks = stopwatch.GetMarksWithDeltas();
        return JsonSerializer.Serialize(marks, options ?? JsonOptions);
    }

    [return: NotNull]
    private static String EscapeMarkdownCell(String cell)
    {
        if (String.IsNullOrWhiteSpace(cell))
        {
            return String.Empty;
        }

        // Replace pipe and newlines to keep table integrity
        return cell.Replace("|", "\\|").Replace("\r", "").Replace("\n", " ");
    }

    [return: NotNull]
    private static String EscapeCsvCell(String cell)
    {
        if (String.IsNullOrWhiteSpace(cell))
        {
            return String.Empty;
        }

        var needsQuotes = cell.Contains(',') || cell.Contains('"') || cell.Contains('\n') || cell.Contains('\r');
        var escaped = cell.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }
}
