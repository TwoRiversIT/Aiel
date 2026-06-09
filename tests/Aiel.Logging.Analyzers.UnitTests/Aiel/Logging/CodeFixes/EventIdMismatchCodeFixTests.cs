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
// AIEL00012_CodeFix_Tests.cs — AIEL00012 code-fix
// -----------------------------------------------------------------------
using Aiel.Logging.Analyzers;
using Aiel.Logging.Internal;
using Microsoft.CodeAnalysis.Testing;
using Verifiers;

namespace Aiel.Logging.CodeFixes;

public sealed class EventIdMismatchCodeFixTests
{
    // Fix index 0 = "Update attribute EventId to match parameter default"
    // Fix index 1 = "Update parameter default to match attribute EventId"

    [Fact]
    public async Task Mismatch_Fix0_UpdatesAttributeToMatchParameter()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [{|#0:LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Started")|}]
                public static partial void ServiceStarted(this ILogger logger, AielEventIds eventId = AielEventIds.ServiceStop);
            }
            """;

        // Fix 0: attribute EventId updated to match the parameter default (ServiceStop).
        const String fixedSource = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStop, Level = LogLevel.Information, Message = "[{EventId}] Started")]
                public static partial void ServiceStarted(this ILogger logger, AielEventIds eventId = AielEventIds.ServiceStop);
            }
            """;

        var expected = new DiagnosticResult[]
        {
            DiagnosticResult.CompilerWarning(DiagnosticDescriptors.EventIdMismatch.Id).WithSpan(5, 81, 5, 88),
        };

        await AielCodeFixVerifier<EventIdMismatchAnalyzer, EventIdMismatchCodeFix>
            .VerifyCodeFixAsync(source, fixedSource, codeFixIndex: 0, expected: expected);
    }

    [Fact]
    public async Task Mismatch_Fix1_UpdatesParameterToMatchAttribute()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [{|#0:LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Started")|}]
                public static partial void ServiceStarted(this ILogger logger, AielEventIds eventId = AielEventIds.ServiceStop);
            }
            """;

        // Fix 1: parameter default updated to match the attribute EventId (ServiceStart).
        const String fixedSource = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Started")]
                public static partial void ServiceStarted(this ILogger logger, AielEventIds eventId = AielEventIds.ServiceStart);
            }
            """;

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticDescriptors.EventIdMismatch.Id)
            .WithSpan(5, 81, 5, 88);

        await AielCodeFixVerifier<EventIdMismatchAnalyzer, EventIdMismatchCodeFix>
            .VerifyCodeFixAsync(source, fixedSource, codeFixIndex: 1, expected: expected);
    }
}
