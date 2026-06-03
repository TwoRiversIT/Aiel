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

public sealed class MissingEventIdInMessageCodeFixTests
{
    [Fact]
    public Task Fix_PrependsBracketedPlaceholderToMessage()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = {|#0:"Module started"|})]
                public static partial void LogModuleStart(ILogger logger);
            }
            """;

        const string fixedSource = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Module started")]
                public static partial void LogModuleStart(ILogger logger);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.MissingEventIdInMessage)
            .WithLocation(0)
            .WithArguments("Module started");

        return AielCodeFixVerifier<MissingEventIdInMessageAnalyzer, MissingEventIdInMessageCodeFix>
            .VerifyCodeFixAsync(source, expected, fixedSource);
    }

    [Fact]
    public Task Fix_PrependsBracketedPlaceholderToEmptyMessage()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStop, Level = 0, Message = {|#0:""|})]
                public static partial void LogModuleStop(ILogger logger);
            }
            """;

        const string fixedSource = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStop, Level = 0, Message = "[{EventId}] ")]
                public static partial void LogModuleStop(ILogger logger);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.MissingEventIdInMessage)
            .WithLocation(0)
            .WithArguments("");

        return AielCodeFixVerifier<MissingEventIdInMessageAnalyzer, MissingEventIdInMessageCodeFix>
            .VerifyCodeFixAsync(source, expected, fixedSource);
    }

    [Fact]
    public Task FixAll_PrependsBracketedPlaceholderToAllMessages()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = {|#0:"Module started"|})]
                public static partial void LogModuleStart(ILogger logger);

                [LoggerMessage(EventId = (int)AielEventIds.ModuleStop, Level = 0, Message = {|#1:"Module stopped"|})]
                public static partial void LogModuleStop(ILogger logger);
            }
            """;

        const string fixedSource = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Module started")]
                public static partial void LogModuleStart(ILogger logger);

                [LoggerMessage(EventId = (int)AielEventIds.ModuleStop, Level = 0, Message = "[{EventId}] Module stopped")]
                public static partial void LogModuleStop(ILogger logger);
            }
            """;

        return AielCodeFixVerifier<MissingEventIdInMessageAnalyzer, MissingEventIdInMessageCodeFix>
            .VerifyFixAllAsync(
                source, fixedSource,
                new DiagnosticResult(DiagnosticDescriptors.MissingEventIdInMessage).WithLocation(0).WithArguments("Module started"),
                new DiagnosticResult(DiagnosticDescriptors.MissingEventIdInMessage).WithLocation(1).WithArguments("Module stopped"));
    }
}

