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

// -----------------------------------------------------------------------
// WellKnownTypes.cs
// Centralises fully-qualified type / member names for fixed framework
// types (ILogger, LoggerMessageAttribute, etc.).
//
// NOTE: The EventIds enum type name is intentionally NOT listed here.
//       It is configurable per-project via AnalyzerConfiguration and
//       defaults to "Aiel.Logging.AielEventIds".
// -----------------------------------------------------------------------

namespace Aiel.Roslyn;

internal static class WellKnownTypes
{
    // ── Microsoft.Extensions.Logging ────────────────────────────────────
    public const String ILogger = "Microsoft.Extensions.Logging.ILogger";
    public const String ILoggerOfT = "Microsoft.Extensions.Logging.ILogger`1";
    public const String LoggerMessageAttr = "Microsoft.Extensions.Logging.LoggerMessageAttribute";
    public const String EventIdType = "Microsoft.Extensions.Logging.EventId";

    // ── LoggerMessage attribute named arguments ──────────────────────────
    public const String EventIdArgName = "EventId";
    public const String MessageArgName = "Message";

    // ── Message template placeholder ────────────────────────────────────
    /// <summary>
    /// The placeholder that must appear in every log message template.
    /// This is fixed — it is not configurable.
    /// </summary>
    public const String EventIdPlaceholder = "[{EventId}]";

    // ── ILogger method names that must NOT be called directly ───────────
    public static readonly IReadOnlyList<String> DirectLoggerMethods =
    [
        "LogInformation", "LogWarning", "LogError", "LogCritical", "Log", "BeginScope"
    ];

    // ── Parameter name used in logging helpers ─────────────────
    public const String EventIdParamName = "eventId";
}
