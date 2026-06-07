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

using Aiel.Logging.Analyzers;
using Aiel.Logging.Internal;
using Microsoft.CodeAnalysis.Testing;
using Verifiers;

namespace Aiel.Logging.CodeFixes;

public class LoggerMessageCodeFixProviderTests
{
    [Fact]
    public async Task WithNoParameters_ShouldBe_ReplacedWith_LoggerMessage()
    {
        const String testCode = """
            using Microsoft.Extensions.Logging;

            public static partial class Log(ILogger logger)
            {
                public static void SimulateLogging()
                {
                    logger.LogInformation("Started");
                }
            }
            """;

        const String fixedCode = """
            using Microsoft.Extensions.Logging;

            public static partial class Log(ILogger logger)
            {
                public static void SimulateLogging()
                {
                    logger.GiveThisABetterName();
                }

                [LoggerMessage(EventId = (int)AielEventIds.ServiceStarted, Level = LogLevel.Information, Message = "[{EventId}] Started")]
                public static partial void GiveThisABetterName(this ILogger logger);
            }
            """;

        var expected = new[]
        {
            DiagnosticResult.CompilerWarning(DiagnosticDescriptors.NoDirectILoggerCalls.Id).WithSpan(7, 9, 7, 41)
        };

        await AielCodeFixVerifier<NoDirectILoggerCallsAnalyzer, LoggerMessageCodeFixProvider>
            .VerifyCodeFixAsync(testCode, fixedCode, expected: expected);
    }
}
