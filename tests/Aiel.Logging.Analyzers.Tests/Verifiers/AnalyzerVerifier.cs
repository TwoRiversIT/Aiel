// -----------------------------------------------------------------------
// AnalyzerVerifier.cs
// Thin wrappers around the Microsoft.CodeAnalysis.Testing harness that
// inject the shared Aiel framework source stubs so every test can compile
// against them without referencing a real Aiel runtime package.
// -----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System.Collections.Immutable;

namespace Aiel.Logging.Analyzers.Tests.Verifiers;

/// <summary>
/// Bootstraps an analyzer test with:
/// <list type="bullet">
///   <item>The <c>AielEventIds</c> stub enum.</item>
///   <item>The MEL <c>LoggerMessageAttribute</c> stub.</item>
///   <item>The MEL <c>ILogger</c> / extension-method stubs.</item>
/// </list>
/// </summary>
internal static class AielAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    // ── Framework stubs (embedded so tests have no runtime dependency) ───

    /// <summary>
    /// Minimal <c>AielEventIds</c> enum used in all tests.
    /// Add members here as new test cases require them.
    /// </summary>
    public const string AielEventIdsSource = """
        namespace Aiel.Logging
        {
            public enum AielEventIds
            {
                ModuleStart  = 1001,
                ModuleStop   = 1002,
                RequestStart = 1003,
                RequestStop  = 1004,
            }
        }
        """;

    /// <summary>Stub for <c>Microsoft.Extensions.Logging.LoggerMessageAttribute</c>.</summary>
    public const string LoggerMessageAttrSource = """
        using System;
        namespace Microsoft.Extensions.Logging
        {
            [AttributeUsage(AttributeTargets.Method)]
            public sealed class LoggerMessageAttribute : Attribute
            {
                public int    EventId  { get; set; }
                public int    Level    { get; set; }
                public string Message  { get; set; } = "";
                public LoggerMessageAttribute() { }
                public LoggerMessageAttribute(int eventId, int level, string message)
                {
                    EventId = eventId; Level = level; Message = message;
                }
            }
        }
        """;

    /// <summary>Stub for <c>ILogger</c> and its common extension methods.</summary>
    public const string ILoggerSource = """
        using System;
        namespace Microsoft.Extensions.Logging
        {
            public interface ILogger { }
            public interface ILogger<T> : ILogger { }

            public static class LoggerExtensions
            {
                public static void LogTrace      (this ILogger l, string msg, params object[] args) { }
                public static void LogDebug      (this ILogger l, string msg, params object[] args) { }
                public static void LogInformation(this ILogger l, string msg, params object[] args) { }
                public static void LogWarning    (this ILogger l, string msg, params object[] args) { }
                public static void LogError      (this ILogger l, string msg, params object[] args) { }
                public static void LogCritical   (this ILogger l, string msg, params object[] args) { }
            }
        }
        """;

    // ── Verifier factory ─────────────────────────────────────────────────

    public static CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> CreateTest(
        string source,
        params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode        = source,
            CompilerDiagnostics = CompilerDiagnostics.None,
        };

        test.SolutionTransforms.Add((solution, projectId) =>
            solution.WithProjectParseOptions(
                projectId,
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12)));

        // Inject framework stubs as additional files.
        test.TestState.Sources.Add(AielEventIdsSource);
        test.TestState.Sources.Add(LoggerMessageAttrSource);
        test.TestState.Sources.Add(ILoggerSource);

        test.ExpectedDiagnostics.AddRange(expected);

        return test;
    }

    /// <summary>Run test and assert: no diagnostics expected.</summary>
    public static async Task VerifyNoDiagnosticsAsync(string source)
    {
        var test = CreateTest(source);
        await test.RunAsync();
    }

    /// <summary>Run test and assert: specified diagnostics expected.</summary>
    public static async Task VerifyDiagnosticsAsync(
        string source,
        params DiagnosticResult[] expected)
    {
        var test = CreateTest(source, expected);
        await test.RunAsync();
    }
}

