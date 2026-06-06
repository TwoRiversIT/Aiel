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

using Aiel.Logging.CodeFixes;
using Aiel.Roslyn;
using Microsoft.CodeAnalysis.Testing;
using Verifiers;

namespace Aiel.Logging.Analyzers;

public sealed class NoDirectILoggerCallsCodeFixTests
{
    // Fix index 0 = "Replace with TODO comment"
    // Fix index 1 = "Remove direct ILogger call"

    [Fact]
    public async Task DirectLogCall_ReplacedWithTodoComment()
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

        // Fix 0: replace with a TODO comment (empty statement with leading comment).
        const String fixedSource = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void StartService(ILogger logger)
                {
                    // TODO (AIEL00011): replace with Aiel logging helper — was: logger.LogInformation("Service started")
                    ;
                }
            }
            """;

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticDescriptors.NoDirectILoggerCalls.Id)
            .WithLocation(0);

        await AielCodeFixVerifier<NoDirectILoggerCallsAnalyzer, NoDirectILoggerCallsCodeFix>
            .VerifyCodeFixAsync(source, fixedSource, codeFixIndex: 0, expected: expected);
    }

    [Fact]
    public async Task DirectLogCall_RemovedEntirely()
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

        // Fix 1: remove the statement completely.
        const String fixedSource = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                public static void StartService(ILogger logger)
                {
                }
            }
            """;

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticDescriptors.NoDirectILoggerCalls.Id)
            .WithLocation(0);

        await AielCodeFixVerifier<NoDirectILoggerCallsAnalyzer, NoDirectILoggerCallsCodeFix>
            .VerifyCodeFixAsync(source, fixedSource, codeFixIndex: 1, expected: expected);
    }
}
