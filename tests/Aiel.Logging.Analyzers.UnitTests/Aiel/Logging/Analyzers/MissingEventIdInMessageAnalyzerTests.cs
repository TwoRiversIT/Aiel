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
// AIEL00010_Analyzer_Tests.cs — AIEL00010
// -----------------------------------------------------------------------
using Aiel.Logging.Internal;
using Microsoft.CodeAnalysis.Testing;
using Verifiers;

namespace Aiel.Logging.Analyzers;

public sealed class MissingEventIdInMessageAnalyzerTests
{
    // ── No-diagnostic cases ──────────────────────────────────────────────

    [Fact]
    public async Task PlaceholderPresent_NoDiagnostic()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Service started")]
                public static partial void ServiceStarted(this ILogger logger);
            }
            """;

        await AielAnalyzerVerifier<MissingEventIdInMessageAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PlaceholderPresentWithContent_NoDiagnostic()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Processing {Name}")]
                public static partial void Processing(this ILogger logger, string name);
            }
            """;

        await AielAnalyzerVerifier<MissingEventIdInMessageAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

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

        await AielAnalyzerVerifier<MissingEventIdInMessageAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    // ── Diagnostic cases ─────────────────────────────────────────────────

    [Fact]
    public async Task PlaceholderMissing_RaisesAIEL00010()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = {|#0:"Service started"|})]
                public static partial void ServiceStarted(this ILogger logger);
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.MissingEventIdInMessage.Id)
                .WithSpan(4, 102, 4, 119)
                .WithArguments("Service started")
        };

        await AielAnalyzerVerifier<MissingEventIdInMessageAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EventIdPlaceholder_NotInSquareBrackets_RaisesAIEL00010()
    {
        // Has {EventId} but not the wrapping brackets — should still flag.
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = {|#0:"{EventId} started"|})]
                public static partial void ServiceStarted(this ILogger logger);
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.MissingEventIdInMessage.Id)
                .WithSpan(4, 102, 4, 121)
                .WithArguments("{EventId} started")
        };

        await AielAnalyzerVerifier<MissingEventIdInMessageAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EventIdPlaceholder_NotAtTheStartOfTemplate_RaisesAIEL00010()
    {
        // Has {EventId} but not the wrapping brackets — should still flag.
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = {|#0:"Started service [{EventId}]"|})]
                public static partial void ServiceStarted(this ILogger logger);
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.MissingEventIdInMessage.Id)
                .WithSpan(4, 102, 4, 131)
                .WithArguments("Started service [{EventId}]")
        };

        await AielAnalyzerVerifier<MissingEventIdInMessageAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }
}
