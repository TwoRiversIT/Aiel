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

// ═══════════════════════════════════════════════════════════════════════
// Custom-enum tests — AIEL00012
// ═══════════════════════════════════════════════════════════════════════

public sealed class EventIdMismatchAnalyzerTests
{
    private const String AcmeSource = TestCode.AcmeEventIdsSource;

    [Fact]
    public async Task AcmeEnumMatch_NoDiagnostic()
    {
        /* lang=c#-test */
        const String source = """
            using Microsoft.Extensions.Logging;
            using Acme.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AcmeEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] ok")]
                public static partial void Ok(this ILogger logger, AcmeEventIds eventId = AcmeEventIds.ServiceStart);
            }
            """;

        await AielAnalyzerVerifier<EventIdMismatchAnalyzer>
            .CreateTest(source, eventIdsSource: AcmeSource)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AcmeEnumMismatch_RaisesAIEL00012()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            using Acme.Logging;
            public static partial class Log
            {
                [{|#0:LoggerMessage(EventId = (int)AcmeEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] ok")|}]
                public static partial void Ok(this ILogger logger, AcmeEventIds eventId = AcmeEventIds.ServiceStop);
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.EventIdMismatch.Id)
                .WithSpan(6, 69, 6, 76)
                .WithArguments("AcmeEventIds.ServiceStart", "AcmeEventIds.ServiceStop")
        };

        await AielAnalyzerVerifier<EventIdMismatchAnalyzer>
            .CreateTest(source, eventIdsSource: AcmeSource, expected: expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }
}
