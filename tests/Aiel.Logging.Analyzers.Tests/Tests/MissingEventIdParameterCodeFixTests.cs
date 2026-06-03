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
using Aiel.Logging.CodeFixes;
using Aiel.Roslyn;
using Microsoft.CodeAnalysis.Testing;

namespace Aiel.Logging.Analyzers.Tests.Tests;

public sealed class MissingEventIdParameterCodeFixTests
{
    [Fact]
    public Task Fix_AddsOptionalParameterWithCorrectDefault()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [{|#0:LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")|}]
                public static partial void LogModuleStart(ILogger logger);
            }
            """;

        const string fixedSource = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(ILogger logger, AielEventIds eventId = AielEventIds.ModuleStart);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.MissingEventIdParameter)
            .WithLocation(0)
            .WithArguments("LogModuleStart", "ModuleStart");

        return AielCodeFixVerifier<MissingEventIdParameterAnalyzer, MissingEventIdParameterCodeFix>
            .VerifyCodeFixAsync(source, expected, fixedSource);
    }

    [Fact]
    public Task Fix_AppendsAfterExistingParameters()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [{|#0:LoggerMessage(EventId = (int)AielEventIds.RequestStart, Level = 0, Message = "[{EventId}] Request")|}]
                public static partial void LogRequestStart(ILogger logger, string requestId);
            }
            """;

        const string fixedSource = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.RequestStart, Level = 0, Message = "[{EventId}] Request")]
                public static partial void LogRequestStart(ILogger logger, string requestId, AielEventIds eventId = AielEventIds.RequestStart);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.MissingEventIdParameter)
            .WithLocation(0)
            .WithArguments("LogRequestStart", "RequestStart");

        return AielCodeFixVerifier<MissingEventIdParameterAnalyzer, MissingEventIdParameterCodeFix>
            .VerifyCodeFixAsync(source, expected, fixedSource);
    }

    [Fact]
    public Task FixAll_AddsParameterToAllMissingMethods()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [{|#0:LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")|}]
                public static partial void LogModuleStart(ILogger logger);

                [{|#1:LoggerMessage(EventId = (int)AielEventIds.ModuleStop, Level = 0, Message = "[{EventId}] Stopped")|}]
                public static partial void LogModuleStop(ILogger logger);
            }
            """;

        const string fixedSource = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(ILogger logger, AielEventIds eventId = AielEventIds.ModuleStart);

                [LoggerMessage(EventId = (int)AielEventIds.ModuleStop, Level = 0, Message = "[{EventId}] Stopped")]
                public static partial void LogModuleStop(ILogger logger, AielEventIds eventId = AielEventIds.ModuleStop);
            }
            """;

        return AielCodeFixVerifier<MissingEventIdParameterAnalyzer, MissingEventIdParameterCodeFix>
            .VerifyFixAllAsync(
                source, fixedSource,
                new DiagnosticResult(DiagnosticDescriptors.MissingEventIdParameter).WithLocation(0).WithArguments("LogModuleStart", "ModuleStart"),
                new DiagnosticResult(DiagnosticDescriptors.MissingEventIdParameter).WithLocation(1).WithArguments("LogModuleStop", "ModuleStop"));
    }
}

