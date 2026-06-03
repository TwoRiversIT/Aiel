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

public sealed class EventIdMismatchCodeFixTests
{
    // ── Fix 1: sync parameter default → attribute value ──────────────────

    [Fact]
    public Task Fix_SyncParam_UpdatesParameterDefaultToMatchAttribute()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(
                    ILogger logger,
                    AielEventIds {|#0:eventId|} = AielEventIds.ModuleStop);
            }
            """;

        const string fixedSource = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(
                    ILogger logger,
                    AielEventIds eventId = AielEventIds.ModuleStart);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.EventIdMismatch)
            .WithLocation(0)
            .WithArguments("AielEventIds.ModuleStart", "AielEventIds.ModuleStop");

        return AielCodeFixVerifier<EventIdMismatchAnalyzer, EventIdMismatchCodeFix>
            .VerifyCodeFixAsync(
                source, expected, fixedSource,
                equivalenceKey: "Sync parameter default to match [LoggerMessage] EventId");
    }

    // ── Fix 2: sync attribute EventId → parameter value ──────────────────

    [Fact]
    public Task Fix_SyncAttr_UpdatesAttributeEventIdToMatchParameter()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(
                    ILogger logger,
                    AielEventIds {|#0:eventId|} = AielEventIds.ModuleStop);
            }
            """;

        const string fixedSource = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStop, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(
                    ILogger logger,
                    AielEventIds eventId = AielEventIds.ModuleStop);
            }
            """;

        var expected = new DiagnosticResult(DiagnosticDescriptors.EventIdMismatch)
            .WithLocation(0)
            .WithArguments("AielEventIds.ModuleStart", "AielEventIds.ModuleStop");

        return AielCodeFixVerifier<EventIdMismatchAnalyzer, EventIdMismatchCodeFix>
            .VerifyCodeFixAsync(
                source, expected, fixedSource,
                equivalenceKey: "Sync [LoggerMessage] EventId to match parameter default");
    }

    // ── FixAll: sync parameter defaults ──────────────────────────────────

    [Fact]
    public Task FixAll_SyncParam_FixesBothMismatchedMethods()
    {
        const string source = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(
                    ILogger logger,
                    AielEventIds {|#0:eventId|} = AielEventIds.ModuleStop);

                [LoggerMessage(EventId = (int)AielEventIds.RequestStart, Level = 0, Message = "[{EventId}] Request")]
                public static partial void LogRequestStart(
                    ILogger logger,
                    AielEventIds {|#1:eventId|} = AielEventIds.RequestStop);
            }
            """;

        const string fixedSource = """
            using Microsoft.Extensions.Logging;
            using Aiel.Logging;

            public static partial class MyLog
            {
                [LoggerMessage(EventId = (int)AielEventIds.ModuleStart, Level = 0, Message = "[{EventId}] Started")]
                public static partial void LogModuleStart(
                    ILogger logger,
                    AielEventIds eventId = AielEventIds.ModuleStart);

                [LoggerMessage(EventId = (int)AielEventIds.RequestStart, Level = 0, Message = "[{EventId}] Request")]
                public static partial void LogRequestStart(
                    ILogger logger,
                    AielEventIds eventId = AielEventIds.RequestStart);
            }
            """;

        return AielCodeFixVerifier<EventIdMismatchAnalyzer, EventIdMismatchCodeFix>
            .VerifyFixAllAsync(
                source, fixedSource,
                new DiagnosticResult(DiagnosticDescriptors.EventIdMismatch)
                    .WithLocation(0)
                    .WithArguments("AielEventIds.ModuleStart", "AielEventIds.ModuleStop"),
                new DiagnosticResult(DiagnosticDescriptors.EventIdMismatch)
                    .WithLocation(1)
                    .WithArguments("AielEventIds.RequestStart", "AielEventIds.RequestStop"));
    }
}

