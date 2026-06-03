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

using Aiel.Logging.Analyzers.Tests.Verifiers;
using Aiel.Roslyn;
using Microsoft.CodeAnalysis.Testing;

namespace Aiel.Logging.Analyzers.Tests.Tests;

public sealed class EventIdMismatchAnalyzerTests
{
    // ── No-diagnostic cases ──────────────────────────────────────────────

    [Fact]
    public Task NoDiagnostic_WhenAttributeAndParameterAgree()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(
                    ILogger logger,
                    AielEventIds eventId = AielEventIds.ModuleStart);
            }
            """;

        return AielAnalyzerVerifier<EventIdMismatchAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    [Fact]
    public Task NoDiagnostic_WhenNoEventIdParameterPresent()
    {
        // AIEL002 covers the missing-parameter case; AIEL005 stays silent.
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(ILogger logger);
            }
            """;

        return AielAnalyzerVerifier<EventIdMismatchAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    [Fact]
    public Task NoDiagnostic_WhenNoLoggerMessageAttribute()
    {
        const string source = """
            using Aiel.Logging;

            public class Foo
            {
                public void Bar(AielEventIds eventId = AielEventIds.ModuleStart) { }
            }
            """;

        return AielAnalyzerVerifier<EventIdMismatchAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    // ── Diagnostic cases ─────────────────────────────────────────────────

    [Fact]
    public Task Diagnostic_WhenAttributeAndParameterDiffer()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(
                    ILogger logger,
                    AielEventIds {|#0:eventId|} = AielEventIds.ModuleStop);  // ← wrong default
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.EventIdMismatch)
            .WithLocation(0)
            .WithArguments("AielEventIds.ModuleStart", "AielEventIds.ModuleStop");

        return AielAnalyzerVerifier<EventIdMismatchAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }

    [Fact]
    public Task Diagnostic_WhenParameterDefaultIsFromDifferentModuleRange()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.RequestStart, Level = 0, Message = "[{EventId}] Request started")]
                public static partial void LogRequestStart(
                    ILogger logger,
                    AielEventIds {|#0:eventId|} = AielEventIds.ModuleStart);  // wrong range
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.EventIdMismatch)
            .WithLocation(0)
            .WithArguments("AielEventIds.RequestStart", "AielEventIds.ModuleStart");

        return AielAnalyzerVerifier<EventIdMismatchAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }

    [Fact]
    public Task Diagnostic_TwoMismatchedMethodsInSameClass()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(
                    ILogger logger,
                    AielEventIds {|#0:eventId|} = AielEventIds.ModuleStop);

                [LoggerMessage(EventId = (int)AielEventIds.RequestStop, Level = 0, Message = "[{EventId}] Request stopped")]
                public static partial void LogRequestStop(
                    ILogger logger,
                    AielEventIds {|#1:eventId|} = AielEventIds.RequestStart);
            }
            """;

        return AielAnalyzerVerifier<EventIdMismatchAnalyzer>
            .VerifyDiagnosticsAsync(
                source,
                new DiagnosticResult(DiagnosticDescriptors.EventIdMismatch)
                    .WithLocation(0)
                    .WithArguments("AielEventIds.ModuleStart", "AielEventIds.ModuleStop"),
                new DiagnosticResult(DiagnosticDescriptors.EventIdMismatch)
                    .WithLocation(1)
                    .WithArguments("AielEventIds.RequestStop", "AielEventIds.RequestStart"));
    }
}

