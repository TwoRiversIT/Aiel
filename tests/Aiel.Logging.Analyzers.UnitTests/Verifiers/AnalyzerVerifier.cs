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

using Aiel.Logging.Configuration;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Verifiers;

/// <summary>
/// Creates <see cref="CSharpAnalyzerTest{TAnalyzer,TVerifier}"/> instances
/// pre-loaded with the Aiel framework stubs so individual test classes stay lean.
/// </summary>
internal static class AielAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{

    // ── Factory ──────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a pre-configured <see cref="CSharpAnalyzerTest{TAnalyzer,TVerifier}"/>
    /// with the standard stubs loaded.
    /// </summary>
    /// <param name="source">C# source with optional diagnostic markers.</param>
    /// <param name="eventIdsSource">Override the EventIds enum stub. <see langword="null"/> uses the default AielEvent stub.</param>
    /// <param name="expected">Diagnostics expected in the source. Pass <see langword="null"/> for no-diagnostic tests.</param>
    public static CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> CreateTest(
        String source,
        String eventIdsSource,
        params DiagnosticResult[] expected)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventIdsSource);

        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            CompilerDiagnostics = CompilerDiagnostics.None
        };

        // Suppress: Partial method '<method>' must have an implementation part because it has accessibility modifiers.
        test.DisabledDiagnostics.Add("CS8795");

        test.TestState.Sources.Add(eventIdsSource);
        test.TestState.Sources.Add(TestCode.LoggerMessageAttrSource);
        test.TestState.Sources.Add(TestCode.ILoggerSource);

        test.TestBehaviors |= TestBehaviors.SkipGeneratedCodeCheck;

        if (eventIdsSource.Contains("Acme"))
        {
            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig",
                $"""
                root = true

                [*]
                build_property.{AnalyzerConfiguration.EditorConfigKey} = Acme.Logging.AcmeEventIds
                """
                ));
        }

        if (expected is not null)
        {
            test.ExpectedDiagnostics.AddRange(expected);
        }

        return test;
    }
}
