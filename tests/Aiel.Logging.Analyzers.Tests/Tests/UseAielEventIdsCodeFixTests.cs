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

public sealed class UseAielEventIdsCodeFixTests
{
    [Fact]
    public Task Fix_ReplacesNamedIntLiteralWithEnumCast()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = {|#0:1001|}, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(ILogger logger);
            }
            """;

        const string fixedSource = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(ILogger logger);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.UseAielEventIds)
            .WithLocation(0)
            .WithArguments("1001", "ModuleStart");

        return AielCodeFixVerifier<UseAielEventIdsAnalyzer, UseAielEventIdsCodeFix>
            .VerifyCodeFixAsync(source, expected, fixedSource);
    }

    [Fact]
    public Task Fix_ReplacesPositionalIntLiteralWithEnumCast()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage({|#0:1002|}, 0, "[{EventId}] Stopped")]
                public static partial void LogModuleStop(ILogger logger);
            }
            """;

        const string fixedSource = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage((int)AielEventIds.ModuleStop, 0, "[{EventId}] Stopped")]
                public static partial void LogModuleStop(ILogger logger);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.UseAielEventIds)
            .WithLocation(0)
            .WithArguments("1002", "ModuleStop");

        return AielCodeFixVerifier<UseAielEventIdsAnalyzer, UseAielEventIdsCodeFix>
            .VerifyCodeFixAsync(source, expected, fixedSource);
    }

    [Fact]
    public Task FixAll_ReplacesBothLiteralsInSameClass()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = {|#0:1001|}, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(ILogger logger);

                [LoggerMessage(EventId = {|#1:1002|}, Level = 0, Message = "[{EventId}] Stopped")]
                public static partial void LogModuleStop(ILogger logger);
            }
            """;

        const string fixedSource = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(ILogger logger);

                [LoggerMessage(EventId = (int)AielEventIds.ModuleStop, Level = 0, Message = "[{EventId}] Stopped")]
                public static partial void LogModuleStop(ILogger logger);
            }
            """;

        return AielCodeFixVerifier<UseAielEventIdsAnalyzer, UseAielEventIdsCodeFix>
            .VerifyFixAllAsync(
                source, fixedSource,
                new DiagnosticResult(DiagnosticDescriptors.UseAielEventIds).WithLocation(0).WithArguments("1001", "ModuleStart"),
                new DiagnosticResult(DiagnosticDescriptors.UseAielEventIds).WithLocation(1).WithArguments("1002", "ModuleStop"));
    }
}

