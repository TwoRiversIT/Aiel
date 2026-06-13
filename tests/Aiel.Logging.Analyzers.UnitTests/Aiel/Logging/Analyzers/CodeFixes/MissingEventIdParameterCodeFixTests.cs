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
// AIEL00009_CodeFix_Tests.cs — AIEL00009 code-fix
// -----------------------------------------------------------------------
using Aiel.Logging.Internal;
using Microsoft.CodeAnalysis.Testing;
using Verifiers;

namespace Aiel.Logging.Analyzers.CodeFixes;

public sealed class MissingEventIdParameterCodeFixTests
{
    [Fact]
    public async Task HasPlaceholder_MissingEventIdParameter_ShouldAppendEventIdParameter_WithAielEventIdsMatchingMember_AtEnd()
    {
        const String testCode = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [{|#0:LoggerMessage(EventId = (int)AielEvent.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Started")|}]
                public static partial void ServiceStarted(this ILogger logger);
            }
            """;

        // The fix appends `AielEvent eventId = AielEvent.<MatchingMember>` as the last parameter. In this case, the matching member is `AielEvent.ServiceStart`.
        const String fixedCode = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEvent.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Started")]
                public static partial void ServiceStarted(this ILogger logger, AielEvent eventId = AielEvent.ServiceStart);
            }
            """;

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticDescriptors.MissingEventIdParameter.Id)
            .WithSpan(5, 32, 5, 46);

        await AielCodeFixVerifier<MissingEventIdParameterAnalyzer, MissingEventIdParameterCodeFix>
            .VerifyCodeFixAsync(testCode, fixedCode, expected: expected);
    }

    [Fact]
    public async Task HasPlaceholderAndExistingParameters_MissingEventIdParameter_ShouldAppendEventIdParameter_WithAielEventIdsMatchingMember_AtEnd()
    {
        const String testCode = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [{|#0:LoggerMessage(EventId = (int)AielEvent.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] {Name}")|}]
                public static partial void ServiceStarted(this ILogger logger, string name);
            }
            """;

        // The fix appends `AielEvent eventId = AielEvent.<MatchingMember>` as the last parameter. In this case, the matching member is `AielEvent.ServiceStart`.
        const String fixedCode = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEvent.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] {Name}")]
                public static partial void ServiceStarted(this ILogger logger, string name, AielEvent eventId = AielEvent.ServiceStart);
            }
            """;

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticDescriptors.MissingEventIdParameter.Id)
            .WithSpan(5, 32, 5, 46);

        await AielCodeFixVerifier<MissingEventIdParameterAnalyzer, MissingEventIdParameterCodeFix>
            .VerifyCodeFixAsync(testCode, fixedCode, expected: expected);
    }
}
