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

public sealed class UseAielEventIdsAnalyzerTests
{
    // ── No-diagnostic cases ──────────────────────────────────────────────

    [Fact]
    public Task NoDiagnostic_WhenEventIdIsCastFromAielEventIds()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(ILogger logger, AielEventIds eventId = AielEventIds.ModuleStart);
            }
            """;

        return AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    [Fact]
    public Task NoDiagnostic_WhenNoLoggerMessageAttribute()
    {
        const string source = """
            public class Foo
            {
                public void Bar(int eventId = 1001) { }
            }
            """;

        return AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    [Fact]
    public Task NoDiagnostic_WhenEventIdArgumentAbsent()
    {
        // Some overloads of LoggerMessage don't include EventId.
        const string source = """
            using Microsoft.Extensions.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogStart(ILogger logger);
            }
            """;

        return AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    // ── Diagnostic cases ─────────────────────────────────────────────────

    [Fact]
    public Task Diagnostic_WhenEventIdIsRawIntegerLiteral()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = {|#0:1001|}, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(ILogger logger);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.UseAielEventIds)
            .WithLocation(0)
            .WithArguments("1001", "ModuleStart");

        return AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }

    [Fact]
    public Task Diagnostic_WhenEventIdIsRawIntegerWithNoKnownMember()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = {|#0:9999|}, Level = 0, Message = "[{EventId}] Unknown")]
                public static partial void LogUnknown(ILogger logger);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.UseAielEventIds)
            .WithLocation(0)
            .WithArguments("9999", "SomeMember");

        return AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }

    [Fact]
    public Task Diagnostic_WhenPositionalEventIdIsRawInteger()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage({|#0:1002|}, 0, "[{EventId}] Stopped")]
                public static partial void LogModuleStop(ILogger logger);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.UseAielEventIds)
            .WithLocation(0)
            .WithArguments("1002", "ModuleStop");

        return AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }
}

