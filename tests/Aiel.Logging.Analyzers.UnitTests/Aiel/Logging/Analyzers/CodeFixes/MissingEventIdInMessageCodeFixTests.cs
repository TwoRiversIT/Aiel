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
// AIEL00010_CodeFix_Tests.cs — AIEL00010 code-fix
// -----------------------------------------------------------------------
using Aiel.Logging.Internal;
using Microsoft.CodeAnalysis.Testing;
using Verifiers;

namespace Aiel.Logging.Analyzers.CodeFixes;

public sealed class MissingEventIdInMessageCodeFixTests
{
    [Fact]
    public async Task MissingPlaceholder_PrependedToSimpleString()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = {|#0:"Service started"|})]
                public static partial void ServiceStarted(this ILogger logger);
            }
            """;

        const String fixedSource = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Service started")]
                public static partial void ServiceStarted(this ILogger logger);
            }
            """;

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticDescriptors.MissingTemplateEventIdPlaceholder.Id)
            .WithLocation(0);

        await AielCodeFixVerifier<MissingTemplateEventIdPlaceholderAnalyzer, MissingEventIdInMessageCodeFix>
            .VerifyCodeFixAsync(source, fixedSource, expected: expected);
    }

    [Fact]
    public async Task MissingPlaceholder_EmptyMessage_PrependedCorrectly()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = {|#0:""|})]
                public static partial void ServiceStarted(this ILogger logger);
            }
            """;

        const String fixedSource = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] ")]
                public static partial void ServiceStarted(this ILogger logger);
            }
            """;

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticDescriptors.MissingTemplateEventIdPlaceholder.Id)
            .WithLocation(0);

        await AielCodeFixVerifier<MissingTemplateEventIdPlaceholderAnalyzer, MissingEventIdInMessageCodeFix>
            .VerifyCodeFixAsync(source, fixedSource, expected: expected);
    }

    [Fact]
    public async Task MissingPlaceholder_WithTemplateParams_PrependedCorrectly()
    {
        const String source = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = {|#0:"Processing {Name} for {UserId}"|})]
                public static partial void Processing(this ILogger logger, string name, int userId);
            }
            """;

        const String fixedSource = """
            using Microsoft.Extensions.Logging;
            public static partial class Log
            {
                [LoggerMessage(EventId = (int)AielEventIds.ServiceStart, Level = LogLevel.Information, Message = "[{EventId}] Processing {Name} for {UserId}")]
                public static partial void Processing(this ILogger logger, string name, int userId);
            }
            """;

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticDescriptors.MissingTemplateEventIdPlaceholder.Id)
            .WithLocation(0);

        await AielCodeFixVerifier<MissingTemplateEventIdPlaceholderAnalyzer, MissingEventIdInMessageCodeFix>
            .VerifyCodeFixAsync(source, fixedSource, expected: expected);
    }
}
