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

public sealed class MissingEventIdInMessageAnalyzerTests
{
    // ── No-diagnostic cases ──────────────────────────────────────────────

    [Fact]
    public Task NoDiagnostic_WhenMessageContainsPlaceholder()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Module started")]
                public static partial void LogModuleStart(ILogger logger);
            }
            """;

        return AielAnalyzerVerifier<MissingEventIdInMessageAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    [Fact]
    public Task NoDiagnostic_WhenPlaceholderIsInMiddleOfMessage()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "Module started [{EventId}] successfully")]
                public static partial void LogModuleStart(ILogger logger);
            }
            """;

        return AielAnalyzerVerifier<MissingEventIdInMessageAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    [Fact]
    public Task NoDiagnostic_WhenNoLoggerMessageAttribute()
    {
        const string source = """
            public class Foo
            {
                public string Bar() => "hello world";
            }
            """;

        return AielAnalyzerVerifier<MissingEventIdInMessageAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    // ── Diagnostic cases ─────────────────────────────────────────────────

    [Fact]
    public Task Diagnostic_WhenMessageMissingPlaceholder()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = {|#0:"Module started"|})]
                public static partial void LogModuleStart(ILogger logger);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.MissingEventIdInMessage)
            .WithLocation(0)
            .WithArguments("Module started");

        return AielAnalyzerVerifier<MissingEventIdInMessageAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }

    [Fact]
    public Task Diagnostic_WhenMessageIsEmptyString()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStop, Level = 0, Message = {|#0:""|})]
                public static partial void LogModuleStop(ILogger logger);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.MissingEventIdInMessage)
            .WithLocation(0)
            .WithArguments("");

        return AielAnalyzerVerifier<MissingEventIdInMessageAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }

    [Fact]
    public Task Diagnostic_WhenMessageHasOnlyPartialPlaceholder_MissingBrackets()
    {
        // "{EventId}" without surrounding brackets does NOT satisfy the rule.
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStop, Level = 0, Message = {|#0:"{EventId} Module stopped"|})]
                public static partial void LogModuleStop(ILogger logger);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.MissingEventIdInMessage)
            .WithLocation(0)
            .WithArguments("{EventId} Module stopped");

        return AielAnalyzerVerifier<MissingEventIdInMessageAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }

    [Fact]
    public Task Diagnostic_WhenPositionalMessageArgumentMissingPlaceholder()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(1003, 0, {|#0:"Request started"|})]
                public static partial void LogRequestStart(ILogger logger);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.MissingEventIdInMessage)
            .WithLocation(0)
            .WithArguments("Request started");

        return AielAnalyzerVerifier<MissingEventIdInMessageAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }
}

