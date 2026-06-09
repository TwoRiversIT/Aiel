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

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Verifiers;

/// <summary>
/// Creates <see cref="CSharpAnalyzerTest{TAnalyzer,TVerifier}"/> instances
/// pre-loaded with StrongIds stubs so individual test classes stay lean.
/// </summary>
internal static class StrongIdAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    // ── Factory ──────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a pre-configured <see cref="CSharpAnalyzerTest{TAnalyzer,TVerifier}"/>
    /// with the standard stubs loaded.
    /// </summary>
    /// <param name="source">C# source with optional diagnostic markers.</param>
    /// <param name="expected">Diagnostics expected in the source. Pass empty for no-diagnostic tests.</param>
    public static CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> CreateTest(
        String source,
        params DiagnosticResult[] expected)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            CompilerDiagnostics = CompilerDiagnostics.None,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100
        };

        // Add StrongIds attribute and interface stubs
        test.TestState.Sources.Add(TestCode.StrongIdAttributeSource);
        test.TestState.Sources.Add(TestCode.IStrongIdSource);

        test.TestBehaviors |= TestBehaviors.SkipGeneratedCodeCheck;

        // Add expected diagnostics
        foreach (var diagnostic in expected)
        {
            test.ExpectedDiagnostics.Add(diagnostic);
        }

        return test;
    }
}

/// <summary>
/// Common test code stubs for StrongIds analyzer tests.
/// </summary>
internal static class TestCode
{
    /// <summary>
    /// Stub for the StrongIdAttribute&lt;TValue&gt; attribute.
    /// </summary>
    public const String StrongIdAttributeSource = """
        namespace Aiel.StrongIds
        {
            using System;

            /// <summary>Marks a type as a StrongId.</summary>
            [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public sealed class StrongIdAttribute<TValue> : Attribute
                where TValue : notnull
            {
                /// <summary>Gets or sets a value indicating whether to generate TryFrom overloads.</summary>
                public Boolean DisallowDefault { get; init; } = true;

                /// <summary>Gets or sets the backing kind.</summary>
                public StrongIdBackingKind BackingKind { get; init; } = StrongIdBackingKind.Value;

                /// <summary>Gets or sets a value indicating whether to generate TryFrom overloads.</summary>
                public Boolean GenerateTryFrom { get; init; } = true;
            }

            /// <summary>
            /// Backing type options for StrongIds.
            /// </summary>
            public enum StrongIdBackingKind
            {
                /// <summary>Value backing type.</summary>
                Value,

                /// <summary>GUID backing type.</summary>
                Guid,

                /// <summary>32-bit integer backing type.</summary>
                Int32,

                /// <summary>64-bit integer backing type.</summary>
                Int64,

                /// <summary>String backing type.</summary>
                String
            }
        }
        """;

    /// <summary>
    /// Stub for the IStrongId and IStrongId&lt;TValue&gt; interfaces.
    /// </summary>
    public const String IStrongIdSource = """
        namespace Aiel.StrongIds
        {
            using System;

            /// <summary>
            /// Non-generic marker interface for all StrongId types.
            /// </summary>
            public interface IStrongId
            {
            }

            /// <summary>
            /// Generic interface that all StrongId types implement.
            /// </summary>
            /// <typeparam name="TValue">The backing type of the StrongId.</typeparam>
            public interface IStrongId<TValue> : IStrongId
                where TValue : notnull
            {
                /// <summary>Gets the underlying value.</summary>
                TValue Value { get; }
            }
        }
        """;
}
