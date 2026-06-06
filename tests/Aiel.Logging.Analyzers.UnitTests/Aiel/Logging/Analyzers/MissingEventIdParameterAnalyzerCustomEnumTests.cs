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

using Aiel.Roslyn;
using Microsoft.CodeAnalysis.Testing;
using Verifiers;

namespace Aiel.Logging.Analyzers;

// ═══════════════════════════════════════════════════════════════════════
// Custom-enum tests — AIEL00009
// ═══════════════════════════════════════════════════════════════════════

public sealed class MissingEventIdParameterAnalyzerCustomEnumTests
{
    [Fact]
    public async Task AcmeEventIdsParam_NoDiagnostic()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            using Acme.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AcmeEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] ok")]
                public static partial void Ok(this ILogger logger, AcmeEventIds eventId = AcmeEventIds.ServiceStart);
            }
            """;

        await AielAnalyzerVerifier<MissingEventIdParameterAnalyzer>
            .CreateTest(source, TestCode.AcmeEventIdsSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MissingParam_WithCustomEnum_RaisesAIEL00009()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            using Acme.Logging;
            public static partial class Log
            {
                [{|#0:LoggerMessage(EventId = (int)AcmeEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] ok")|}]
                public static partial void Ok(this ILogger logger);
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.MissingEventIdParameter.Id)
                .WithSpan(6, 32, 6, 34)
                .WithArguments("Ok", "ServiceStart")
        };

        await AielAnalyzerVerifier<MissingEventIdParameterAnalyzer>
            .CreateTest(source, TestCode.AcmeEventIdsSource, expected: expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }
}
