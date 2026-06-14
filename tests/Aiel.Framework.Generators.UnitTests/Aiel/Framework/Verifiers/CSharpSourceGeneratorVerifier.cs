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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

using static Aiel.Framework.Stubs.Stubs;

namespace Aiel.Framework.Verifiers;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : IIncrementalGenerator, new()
{
    public class Test : CSharpSourceGeneratorTest<TSourceGenerator, DefaultVerifier>;

    public static readonly ImmutableArray<PackageIdentity> HostPackages = [new PackageIdentity("Microsoft.Extensions.Hosting", "10.0.9")];
    public static readonly ImmutableArray<PackageIdentity> WebPackages = [new PackageIdentity("Microsoft.AspNetCore.App.Ref", "10.0.9")];
    public static readonly ImmutableArray<PackageIdentity> WebAssemblyPackages = [new PackageIdentity("Microsoft.AspNetCore.Components.WebAssembly", "10.0.9")];

    [SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Readbility")]
    public static async Task TestAsync(
        String sourceCode,
        String? expectedCode = null,
        String generatedFileName = "AielDependencyGraph.g.cs",
        IEnumerable<DiagnosticDescriptor>? expectedDiagnostics = null,
        Boolean includeHostApplication = false,
        Boolean includeWebApplication = false,
        Boolean includeWebAssembly = false,
        params String[] additionalSources)
    {
        var test = new CSharpSourceGeneratorVerifier<TSourceGenerator>.Test
        {
            TestCode = sourceCode
        };

        foreach (var dependency in AielDependencies)
        {
            test.TestState.Sources.Add(dependency);
        }

        if (!String.IsNullOrWhiteSpace(expectedCode))
        {
            test.TestState.GeneratedSources.Add(
                (typeof(TSourceGenerator), generatedFileName, SourceText.From(expectedCode.Replace("\r\n", "\n"), Encoding.UTF8, SourceHashAlgorithm.Sha256)));
        }

        if (expectedDiagnostics != null)
        {
            var diagnosticResults = expectedDiagnostics
                .Select(d => DiagnosticResult.CompilerError(d.Id))
                .ToArray();
            test.TestState.ExpectedDiagnostics.AddRange(diagnosticResults);
        }

        if (includeHostApplication && includeWebApplication)
        {
            test.TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net100
                .WithPackages(HostPackages)
                .WithPackages(WebPackages);
        }
        else if (includeHostApplication)
        {
            test.TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net100
                .WithPackages(HostPackages);
        }
        else if (includeWebApplication)
        {
            test.TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net100
                .WithPackages(WebPackages);
        }
        else if (includeWebAssembly)
        {
            test.TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net100
                .WithPackages(WebAssemblyPackages);
        }
        else
        {
            test.TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net100;
        }

        foreach (var additionalSource in additionalSources)
        {
            test.TestState.Sources.Add(additionalSource);
        }

        await test.RunAsync(TestContext.Current.CancellationToken);
    }
}
