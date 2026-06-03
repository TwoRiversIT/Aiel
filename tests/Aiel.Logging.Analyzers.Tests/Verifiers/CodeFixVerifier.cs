// -----------------------------------------------------------------------
// CodeFixVerifier.cs
// Thin wrapper for code-fix tests using the Microsoft.CodeAnalysis.Testing
// harness. Injects the same Aiel framework stubs as AnalyzerVerifier.
// -----------------------------------------------------------------------

using Aiel.Logging.Analyzers.Tests.Verifiers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Aiel.Logging.Analyzers.Tests.Verifiers;

/// <summary>
/// Creates and runs a <see cref="CSharpCodeFixTest{TAnalyzer,TCodeFix,TVerifier}"/>
/// pre-configured with the Aiel framework stubs.
/// </summary>
internal static class AielCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix  : CodeFixProvider, new()
{
    /// <summary>
    /// Verifies that applying the code fix to <paramref name="source"/>
    /// (which is expected to have <paramref name="expected"/> diagnostics)
    /// produces <paramref name="fixedSource"/>.
    /// </summary>
    public static async Task VerifyCodeFixAsync(
        string source,
        DiagnosticResult expected,
        string fixedSource,
        string? equivalenceKey = null)
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        {
            TestCode        = source,
            FixedCode       = fixedSource,
            CompilerDiagnostics = CompilerDiagnostics.None,
        };

        test.SolutionTransforms.Add((solution, projectId) =>
            solution.WithProjectParseOptions(
                projectId,
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12)));

        if (equivalenceKey is not null)
        {
            test.CodeActionEquivalenceKey = equivalenceKey;
        }

        // Inject framework stubs.
        test.TestState.Sources.Add(AielAnalyzerVerifier<TAnalyzer>.AielEventIdsSource);
        test.TestState.Sources.Add(AielAnalyzerVerifier<TAnalyzer>.LoggerMessageAttrSource);
        test.TestState.Sources.Add(AielAnalyzerVerifier<TAnalyzer>.ILoggerSource);

        // Mirror stubs into the fixed-code state so it compiles cleanly too.
        test.FixedState.Sources.Add(AielAnalyzerVerifier<TAnalyzer>.AielEventIdsSource);
        test.FixedState.Sources.Add(AielAnalyzerVerifier<TAnalyzer>.LoggerMessageAttrSource);
        test.FixedState.Sources.Add(AielAnalyzerVerifier<TAnalyzer>.ILoggerSource);

        test.ExpectedDiagnostics.Add(expected);

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the FixAll provider applied to <paramref name="source"/>
    /// produces <paramref name="fixedSource"/>.
    /// </summary>
    public static async Task VerifyFixAllAsync(
        string source,
        string fixedSource,
        params DiagnosticResult[] expected)
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        {
            TestCode        = source,
            FixedCode       = fixedSource,
            CompilerDiagnostics = CompilerDiagnostics.None,
        };

        test.SolutionTransforms.Add((solution, projectId) =>
            solution.WithProjectParseOptions(
                projectId,
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12)));

        test.TestState.Sources.Add(AielAnalyzerVerifier<TAnalyzer>.AielEventIdsSource);
        test.TestState.Sources.Add(AielAnalyzerVerifier<TAnalyzer>.LoggerMessageAttrSource);
        test.TestState.Sources.Add(AielAnalyzerVerifier<TAnalyzer>.ILoggerSource);

        test.FixedState.Sources.Add(AielAnalyzerVerifier<TAnalyzer>.AielEventIdsSource);
        test.FixedState.Sources.Add(AielAnalyzerVerifier<TAnalyzer>.LoggerMessageAttrSource);
        test.FixedState.Sources.Add(AielAnalyzerVerifier<TAnalyzer>.ILoggerSource);

        test.ExpectedDiagnostics.AddRange(expected);

        await test.RunAsync();
    }
}

