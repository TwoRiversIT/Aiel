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
// AIEL00009_Analyzer_Tests.cs — AIEL00009
// -----------------------------------------------------------------------
using Aiel.Roslyn;
using Microsoft.CodeAnalysis.Testing;
using Verifiers;

namespace Aiel.Logging.Analyzers;

public sealed class MissingEventIdParameterAnalyzerTests
{
    // ── No-diagnostic cases ──────────────────────────────────────────────

    [Fact]
    public async Task NoLoggerMessageAttribute_NoDiagnostic()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void ServiceStarted(this ILogger logger) { }
            }
            """;

        await AielAnalyzerVerifier<MissingEventIdParameterAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ParameterPresent_NoDiagnostic()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Started")]
                public static partial void ServiceStarted(this ILogger logger, AielEventIds eventId = AielEventIds.ServiceStart);
            }
            """;

        await AielAnalyzerVerifier<MissingEventIdParameterAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    // ── Diagnostic cases ─────────────────────────────────────────────────

    [Fact]
    public async Task ParameterMissing_RaisesAIEL00009()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Started")]
                public static partial void ServiceStarted(this ILogger logger);
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.MissingEventIdParameter.Id)
                .WithSpan(5, 32, 5, 46)
                .WithArguments("ServiceStarted", "ServiceStart")
        };

        await AielAnalyzerVerifier<MissingEventIdParameterAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ParameterWrongType_RaisesAIEL00009()
    {
        // Has a parameter named eventId but it's int, not AielEventIds — should flag.
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Started")]
                public static partial void ServiceStarted(this ILogger logger, int eventId = 1000);
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.MissingEventIdParameter.Id)
                .WithSpan(5, 32, 5, 46)
                .WithArguments("ServiceStarted", "ServiceStart")
        };

        await AielAnalyzerVerifier<MissingEventIdParameterAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }
}
