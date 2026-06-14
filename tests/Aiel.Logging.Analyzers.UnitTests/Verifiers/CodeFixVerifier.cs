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

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Verifiers;

/// <summary>
/// Creates <see cref="CSharpCodeFixTest{TAnalyzer,TCodeFix,TVerifier}"/> instances
/// pre-loaded with the Aiel framework stubs so individual test classes stay lean.
/// </summary>
internal static class AielCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    /// <summary>
    /// Creates a code-fix test that expects <paramref name="testCode"/> to be
    /// transformed into <paramref name="fixedCode"/> after the fix runs.
    /// </summary>
    /// <param name="testCode">C# testCode with diagnostic markers.</param>
    /// <param name="fixedCode">Expected C# testCode after the fix.</param>
    /// <param name="eventIdsSource">
    /// Override the default <c>AielEvent</c> stub.  Pass <see langword="null"/>
    /// to use the Aiel default.
    /// </param>
    /// <param name="codeFixIndex">
    /// Zero-based index of the fix to apply when multiple fixes are registered.
    /// </param>
    /// <param name="expected">Diagnostics expected in the unfixed testCode.</param>
    public static CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier> CreateTest(
        String testCode,
        String fixedCode,
        String? eventIdsSource = null,
        Int32 codeFixIndex = 0,
        params DiagnosticResult[] expected)
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            CodeActionIndex = codeFixIndex,
            CompilerDiagnostics = CompilerDiagnostics.None
        };

        test.TestState.Sources.Add(eventIdsSource ?? TestCode.AielEventIdsSource);
        test.TestState.Sources.Add(TestCode.LoggerMessageAttrSource);
        test.TestState.Sources.Add(TestCode.ILoggerSource);

        // Mirror the fixed-state stubs so the verifier can compile the fixed code too.
        test.FixedState.Sources.Add(eventIdsSource ?? TestCode.AielEventIdsSource);
        test.FixedState.Sources.Add(TestCode.LoggerMessageAttrSource);
        test.FixedState.Sources.Add(TestCode.ILoggerSource);

        if (expected is not null)
        {
            test.ExpectedDiagnostics.AddRange(expected);
        }

        return test;
    }

    /// <summary>Convenience runner — create and run in one call.</summary>
    public static Task VerifyCodeFixAsync(
        String source,
        String fixedSource,
        String? eventIdsSource = null,
        Int32 codeFixIndex = 0,
        params DiagnosticResult[] expected)
        => CreateTest(source, fixedSource, eventIdsSource, codeFixIndex, expected).RunAsync();
}
