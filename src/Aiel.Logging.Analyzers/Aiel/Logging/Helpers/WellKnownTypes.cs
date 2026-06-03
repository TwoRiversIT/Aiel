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

namespace Aiel.Logging.Helpers;

public static class WellKnownTypes
{
    // ── Aiel framework ──────────────────────────────────────────────────
    /// <summary>Fully-qualified name of the Aiel event-id enum.</summary>
    public const string AielEventIds = "Aiel.Logging.AielEventIds";

    /// <summary>Short (unqualified) name used in attribute / parameter matching.</summary>
    public const string AielEventIdsShort = "AielEventIds";

    // ── Microsoft.Extensions.Logging ────────────────────────────────────
    public const string ILogger = "Microsoft.Extensions.Logging.ILogger";
    public const string ILoggerOfT = "Microsoft.Extensions.Logging.ILogger`1";
    public const string LoggerMessageAttr = "Microsoft.Extensions.Logging.LoggerMessageAttribute";
    public const string EventIdType = "Microsoft.Extensions.Logging.EventId";

    // ── LoggerMessage attribute named arguments ──────────────────────────
    public const string EventIdArgName = "EventId";
    public const string MessageArgName = "Message";

    // ── Message template placeholder ────────────────────────────────────
    /// <summary>The placeholder that must appear in every log message template.</summary>
    public const string EventIdPlaceholder = "[{EventId}]";

    // ── ILogger method names that must NOT be called directly ───────────
    public static readonly IReadOnlyList<string> DirectLoggerMethods =
    [
        "LogTrace", "LogDebug", "LogInformation", "LogWarning",
        "LogError", "LogCritical", "Log", "BeginScope"
    ];

    // ── Optional parameter name used in Aiel logging helpers ────────────
    public const string EventIdParamName = "eventId";
}

