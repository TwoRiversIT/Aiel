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

public sealed class MissingEventIdParameterAnalyzerTests
{
    // ── No-diagnostic cases ──────────────────────────────────────────────

    [Fact]
    public Task NoDiagnostic_WhenOptionalEventIdParameterPresent()
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

        return AielAnalyzerVerifier<MissingEventIdParameterAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    [Fact]
    public Task NoDiagnostic_WhenMethodHasNoLoggerMessageAttribute()
    {
        const string source = """
            public class Foo
            {
                public void Bar() { }
            }
            """;

        return AielAnalyzerVerifier<MissingEventIdParameterAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    [Fact]
    public Task NoDiagnostic_WhenParameterPresentWithDifferentCase()
    {
        // Parameter name matching is case-insensitive.
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(
                    ILogger logger,
                    AielEventIds EventId = AielEventIds.ModuleStart);
            }
            """;

        return AielAnalyzerVerifier<MissingEventIdParameterAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    // ── Diagnostic cases ─────────────────────────────────────────────────

    [Fact]
    public Task Diagnostic_WhenNoEventIdParameterAtAll()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [{|#0:LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")|}]
                public static partial void LogModuleStart(ILogger logger);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.MissingEventIdParameter)
            .WithLocation(0)
            .WithArguments("LogModuleStart", "ModuleStart");

        return AielAnalyzerVerifier<MissingEventIdParameterAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }

    [Fact]
    public Task Diagnostic_WhenEventIdParameterPresentButNotOptional()
    {
        // A required AielEventIds parameter doesn't satisfy AIEL002.
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [{|#0:LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")|}]
                public static partial void LogModuleStart(ILogger logger, AielEventIds eventId);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.MissingEventIdParameter)
            .WithLocation(0)
            .WithArguments("LogModuleStart", "ModuleStart");

        return AielAnalyzerVerifier<MissingEventIdParameterAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }

    [Fact]
    public Task Diagnostic_WhenWrongTypeParameterPresent()
    {
        // An "int eventId" parameter doesn't count – must be AielEventIds.
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [{|#0:LoggerMessage(EventId = (int)AielEventIds.ModuleStop, Level = 0, Message = "[{EventId}] Stopped")|}]
                public static partial void LogModuleStop(ILogger logger, int eventId = 1002);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.MissingEventIdParameter)
            .WithLocation(0)
            .WithArguments("LogModuleStop", "ModuleStop");

        return AielAnalyzerVerifier<MissingEventIdParameterAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }
}

