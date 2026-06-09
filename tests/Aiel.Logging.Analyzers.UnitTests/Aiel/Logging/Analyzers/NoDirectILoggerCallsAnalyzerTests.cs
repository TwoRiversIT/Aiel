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

using Aiel.Logging.Internal;
using Microsoft.CodeAnalysis.Testing;
using Verifiers;

namespace Aiel.Logging.Analyzers;

public class NoDirectILoggerCallsAnalyzerTests
{
    [Fact]
    public async Task DirectLogTraceCall_NoDiagnostic()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void StartService(ILogger logger)
                {
                    {|#0:logger.LogTrace("Trace")|};
                }
            }
            """;

        await AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DirectLogDebugCall_NoDiagnostic()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void StartService(ILogger logger)
                {
                    {|#0:logger.LogDebug("Debug")|};
                }
            }
            """;

        await AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DirectLogInformationCall_RaisesAIEL00011()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void StartService(ILogger logger)
                {
                    {|#0:logger.LogInformation("Information")|};
                }
            }
            """;

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticDescriptors.NoDirectILoggerCalls.Id)
            .WithLocation(0);

        await AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DirectLogWarningCall_RaisesAIEL00011()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void StartService(ILogger logger)
                {
                    {|#0:logger.LogWarning("Warning")|};
                }
            }
            """;

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticDescriptors.NoDirectILoggerCalls.Id)
            .WithLocation(0);

        await AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DirectLogErrorCall_RaisesAIEL00011()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void StartService(ILogger logger)
                {
                    {|#0:logger.LogError("Error")|};
                }
            }
            """;

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticDescriptors.NoDirectILoggerCalls.Id)
            .WithLocation(0);

        await AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DirectLogCriticalCall_RaisesAIEL00011()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void StartService(ILogger logger)
                {
                    {|#0:logger.LogCritical("Critical")|};
                }
            }
            """;

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticDescriptors.NoDirectILoggerCalls.Id)
            .WithLocation(0);

        await AielAnalyzerVerifier<NoDirectILoggerCallsAnalyzer>
            .CreateTest(source, TestCode.AielEventIdsSource, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }
}
