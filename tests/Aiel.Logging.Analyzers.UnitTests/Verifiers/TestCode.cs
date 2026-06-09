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

namespace Verifiers;

internal static class TestCode
{
    // ── Shared source stubs ──────────────────────────────────────────────

    /// <summary>A stub of the real AielEventIds shape used by production code.</summary>
    public const String AielEventIdsSource = """
        public enum AielEventIds
        {
            ServiceStart  = 1000,
            ServiceStop   = 1001,
            RequestStart  = 2000,
            RequestEnd    = 2001,
            RequestError  = 2002,
        }
        """;

    /// <summary>
    /// Custom enum stub for configuration tests.
    /// Full type name: <c>Acme.Logging.AcmeEventIds</c>.
    /// </summary>
    public const String AcmeEventIdsSource = """
        namespace Acme.Logging
        {
            public enum AcmeEventIds
            {
                ServiceStart = 2001,
                ServiceStop  = 2002,
                RequestError = 2003,
            }
        }
        """;

    /// <summary>Minimal LoggerMessageAttribute stub.</summary>
    public const String LoggerMessageAttrSource = """
        // Stub — minimal shape of Microsoft.Extensions.Logging.LoggerMessageAttribute.
        namespace Microsoft.Extensions.Logging
        {
            using System;
            [AttributeUsage(AttributeTargets.Method)]
            public sealed class LoggerMessageAttribute : Attribute
            {
                public LoggerMessageAttribute() { }
                public LoggerMessageAttribute(int eventId, LogLevel level, string message)
                {
                    EventId = eventId; Level = level; Message = message;
                }
                public int      EventId  { get; set; }
                public LogLevel Level    { get; set; }
                public string   Message  { get; set; } = string.Empty;
                public string?  EventName { get; set; }
            }
        }
        """;

    /// <summary>Minimal ILogger + LogLevel + extension-method stubs.</summary>
    public const String ILoggerSource = """
        // Stub — minimal shape of Microsoft.Extensions.Logging.ILogger.
        namespace Microsoft.Extensions.Logging
        {
            public interface ILogger { }

            public enum LogLevel
            {
                Trace, Debug, Information, Warning, Error, Critical, None
            }

            public static class LoggerExtensions
            {
                public static void LogInformation(this ILogger logger, string message, params object[] args) { }
                public static void LogWarning(this ILogger logger, string message, params object[] args) { }
                public static void LogError(this ILogger logger, string message, params object[] args) { }
                public static void LogError(this ILogger logger, System.Exception ex, string message, params object[] args) { }
                public static void LogDebug(this ILogger logger, string message, params object[] args) { }
                public static void LogTrace(this ILogger logger, string message, params object[] args) { }
                public static void LogCritical(this ILogger logger, string message, params object[] args) { }
            }
        }
        """;

}
