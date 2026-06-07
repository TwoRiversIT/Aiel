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
// AIEL00008_Analyzer_Tests.cs — AIEL00008
// -----------------------------------------------------------------------
using Aiel.Logging.Internal;
using Microsoft.CodeAnalysis.Testing;
using Verifiers;

namespace Aiel.Logging.Analyzers;

public sealed class UseAielEventIdsAnalyzerTests
{
    // ── No-diagnostic cases ──────────────────────────────────────────────

    [Fact]
    public async Task LoggerMessageMethod_NoDiagnostic()
    {
        // The invocation is from a [LoggerMessage] partial method — not a direct ILogger call.
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Started")]
                public static partial void ServiceStarted(this ILogger logger, AielEventIds eventId = AielEventIds.ServiceStart);

                public static void StartService(ILogger logger)
                {
                    logger.ServiceStarted();
                }
            }
            """;

        await AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NoAttribute_NoDiagnostic()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static partial void ServiceStarted(this ILogger logger);
            }
            """;

        await AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NonLoggingMethod_NoDiagnostic()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void DoWork(ILogger logger)
                {
                    var x = 1 + 2;
                }
            }
            """;

        await AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AielEventIdsCast_NoDiagnostic()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] ok")]
                public static partial void Ok(this ILogger logger);
            }
            """;

        await AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AttributeAndParameterMatch_NoDiagnostic()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Started")]
                public static partial void ServiceStarted(this ILogger logger, AielEventIds eventId = AielEventIds.ServiceStart);
            }
            """;

        await AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NoParameterPresent_NoDiagnostic()
    {
        // AIEL00009 handles the missing parameter; AIEL00012 only checks when both are present.
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Started")]
                public static partial void ServiceStarted(this ILogger logger);
            }
            """;

        await AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NoAttributeEventId_NoDiagnostic()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(Level = LogLevel.Information, Message = "[{EventId}] Started")]
                public static partial void ServiceStarted(this ILogger logger, AielEventIds eventId = AielEventIds.ServiceStart);
            }
            """;

        await AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task NamedArg_EventName_NoEventId_NoDiagnostic()
    {
        // No EventId argument at all — not AIEL00008's concern (AIEL00009 handles this).
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(Level = LogLevel.Information, Message = "[{EventId}] msg")]
                public static partial void Ok(this ILogger logger);
            }
            """;

        await AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    // ── Diagnostic cases ─────────────────────────────────────────────────

    [Fact]
    public async Task RawInteger_RaisesAIEL00008()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage({|#0:EventId = 1001|}, Level = LogLevel.Information, Message = "[{EventId}] Started")]
                public static partial void ServiceStarted(this ILogger logger);
            }
            """;

        var expected = new[]
        {
            DiagnosticResult.CompilerWarning(DiagnosticDescriptors.UseAielEventIds.Id).WithSpan(4, 30, 4, 34)
        };

        await AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task WrongEnumCast_RaisesAIEL00008()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public enum SomeOtherEnum { Foo = 99 }
            public static partial class Log
            {
                [LoggerMessage({|#0:EventId = (int)SomeOtherEnum.Foo|}, Level = LogLevel.Information, Message = "[{EventId}] msg")]
                public static partial void Foo(this ILogger logger);
            }
            """;

        var expected = new DiagnosticResult[]
        {
            DiagnosticResult.CompilerWarning(DiagnosticDescriptors.UseAielEventIds.Id).WithSpan(5, 30, 5, 52)
        };

        await AielAnalyzerVerifier<UseAielEventIdsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DirectLogInformation_RaisesAIEL00011()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void StartService(ILogger logger)
                {
                    {|#0:logger.LogInformation("Service started")|};
                }
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.NoDirectILoggerCalls.Id)
                .WithLocation(0)
        };

        await AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DirectLogError_RaisesAIEL00011()
    {
        const String source = """
            using System;
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void HandleError(ILogger logger, Exception ex)
                {
                    {|#0:logger.LogError(ex, "An error occurred")|};
                }
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.NoDirectILoggerCalls.Id)
                .WithLocation(0)
        };

        await AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DirectLogWarning_RaisesAIEL00011()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void Warn(ILogger logger)
                {
                    {|#0:logger.LogWarning("Watch out")|};
                }
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.NoDirectILoggerCalls.Id)
                .WithLocation(0)
        };

        await AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AttributeAndParameterDiffer_RaisesAIEL00012()
    {
        /* lang=c#-test */
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [{|#0:LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Started")|}]
                public static partial void ServiceStarted(this ILogger logger, AielEventIds eventId = AielEventIds.ServiceStop);
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.EventIdMismatch.Id)
                .WithSpan(5, 81, 5, 88)
                .WithArguments("AielEventIds.ServiceStart", "AielEventIds.ServiceStop")
        };

        await AielAnalyzerVerifier<EventIdMismatchAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AttributeAndParameterDifferMembers_RaisesAIEL00012()
    {
        /* lang=c#-test */
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [{|#0:LoggerMessage(EventId = (int)AielEventIds.RequestStart, Level = LogLevel.Information, Message = "[{EventId}] Request")|}]
                public static partial void RequestStarted(this ILogger logger, AielEventIds eventId = AielEventIds.ServiceStart);
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.EventIdMismatch.Id)
                .WithSpan(5, 81, 5, 88)
                .WithArguments("AielEventIds.RequestStart", "AielEventIds.ServiceStart")
        };

        await AielAnalyzerVerifier<EventIdMismatchAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }
}
