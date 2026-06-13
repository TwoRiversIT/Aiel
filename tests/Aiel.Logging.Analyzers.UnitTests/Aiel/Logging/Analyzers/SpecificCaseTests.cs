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

public class SpecificCaseTests
{
    [Fact]
    public async Task MigrationLoggingExtensions_LogApplyingMigrations_Raises_MissingEventIdParameter()
    {
        const String source = """
            using Microsoft.Extensions.Logging;

            public static partial class MigrationLoggingExtensions
            {
                [LoggerMessage(EventId = (Int32)AielEvent.Migrations_ApplyingMigrationsStarted, Level = LogLevel.Information, Message = "Applying Migrations: {DatabaseName}")]
                public static partial void LogApplyingMigrations(this ILogger logger, String databaseName);
            }
            """;

        const String eventIds = """
            public enum AielEvent
            {
                Migrations_ApplyingMigrationsStarted = 3000,
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.MissingEventIdParameter.Id)
                .WithSpan(6, 32, 6, 53).WithArguments("LogApplyingMigrations", "Replace_With_A_Valid_Member")
        };

        await AielAnalyzerVerifier<MissingEventIdParameterAnalyzer>
            .CreateTest(source, eventIds, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MigrationLoggingExtensions_LogApplyingMigrations_Raises_MissingPlaceholder()
    {
        const String source = """
            using Microsoft.Extensions.Logging;

            public static partial class MigrationLoggingExtensions
            {
                [LoggerMessage(EventId = (Int32)AielEvent.Migrations_ApplyingMigrationsStarted, Level = LogLevel.Information, Message = "Applying Migrations: {DatabaseName}")]
                public static partial void LogApplyingMigrations(this ILogger logger, String databaseName);
            }
            """;

        const String eventIds = """
            public enum AielEvent
            {
                Migrations_ApplyingMigrationsStarted = 3000,
            }
            """;

        var expected = new[]
        {
            DiagnosticResult
                .CompilerWarning(DiagnosticDescriptors.MissingTemplateEventIdPlaceholder.Id)
                .WithSpan(5, 125, 5, 162)
                .WithArguments("Applying Migrations: {DatabaseName}")
        };

        await AielAnalyzerVerifier<MissingTemplateEventIdPlaceholderAnalyzer>
            .CreateTest(source, eventIds, expected)
            .RunAsync(TestContext.Current.CancellationToken);
    }
}
