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

public sealed class NoDirectILoggerCallsAnalyzerTests
{
    // ── No-diagnostic cases ──────────────────────────────────────────────

    [Fact]
    public Task NoDiagnostic_WhenCallingLoggerMessageHelper()
    {
        // Calling a [LoggerMessage]-decorated helper is fine.
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(ILogger logger, AielEventIds eventId = AielEventIds.ModuleStart);
            }

            public class MyService
            {
                private readonly ILogger _logger;
                public MyService(ILogger logger) => _logger = logger;

                public void Start()
                {
                    MyLog.LogModuleStart(_logger);   // ← helper call, not a direct ILogger call
                }
            }
            """;

        return AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    [Fact]
    public Task NoDiagnostic_WhenILoggerMethodCalledInsideLoggerMessageMethod()
    {
        // The [LoggerMessage] source generator itself calls ILogger – we should
        // not flag those call sites.
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(ILogger logger, AielEventIds eventId = AielEventIds.ModuleStart);

                // Simulated hand-written implementation (should not trigger AIEL004)
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStop, Level = 0, Message = "[{EventId}] Stopped")]
                public static void LogModuleStopImpl(ILogger logger)
                {
                    logger.LogInformation("[{EventId}] Stopped");  // inside [LoggerMessage] method → no diagnostic
                }
            }
            """;

        return AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .VerifyNoDiagnosticsAsync(source);
    }

    // ── Diagnostic cases ─────────────────────────────────────────────────

    [Fact]
    public Task Diagnostic_WhenLogInformationCalledDirectly()
    {
        const string source = """
            using Microsoft.Extensions.Logging;

            public class MyService
            {
                private readonly ILogger _logger;
                public MyService(ILogger logger) => _logger = logger;

                public void Run()
                {
                    {|#0:_logger.LogInformation("hello")|};
                }
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.NoDirectILoggerCalls)
            .WithLocation(0)
            .WithArguments("LogInformation");

        return AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }

    [Fact]
    public Task Diagnostic_WhenLogErrorCalledDirectly()
    {
        const string source = """
            using Microsoft.Extensions.Logging;

            public class MyService
            {
                private readonly ILogger _logger;
                public MyService(ILogger logger) => _logger = logger;

                public void Fail()
                {
                    {|#0:_logger.LogError("something went wrong")|};
                }
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.NoDirectILoggerCalls)
            .WithLocation(0)
            .WithArguments("LogError");

        return AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }

    [Fact]
    public Task Diagnostic_WhenLogWarningCalledDirectly()
    {
        const string source = """
            using Microsoft.Extensions.Logging;

            public class MyService
            {
                private readonly ILogger _logger;
                public MyService(ILogger logger) => _logger = logger;

                public void Warn()
                {
                    {|#0:_logger.LogWarning("slow response")|};
                }
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.NoDirectILoggerCalls)
            .WithLocation(0)
            .WithArguments("LogWarning");

        return AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .VerifyDiagnosticsAsync(source, expected);
    }

    [Fact]
    public Task Diagnostic_MultipleDirectCalls_AllReported()
    {
        const string source = """
            using Microsoft.Extensions.Logging;

            public class MyService
            {
                private readonly ILogger _logger;
                public MyService(ILogger logger) => _logger = logger;

                public void Process()
                {
                    {|#0:_logger.LogDebug("starting")|};
                    {|#1:_logger.LogInformation("processing")|};
                    {|#2:_logger.LogError("failed")|};
                }
            }
            """;

        return AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .VerifyDiagnosticsAsync(
                source,
                new DiagnosticResult(DiagnosticDescriptors.NoDirectILoggerCalls).WithLocation(0).WithArguments("LogDebug"),
                new DiagnosticResult(DiagnosticDescriptors.NoDirectILoggerCalls).WithLocation(1).WithArguments("LogInformation"),
                new DiagnosticResult(DiagnosticDescriptors.NoDirectILoggerCalls).WithLocation(2).WithArguments("LogError"));
    }
}

