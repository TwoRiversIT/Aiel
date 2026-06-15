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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Aiel.Testing.CodeAnalysis;

public static class AnalyzeCS<T>
     where T : DiagnosticAnalyzer, new()
{
    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(String testCode, params String[] stubs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(testCode);

        var trees = stubs.Length > 0
            ? stubs.Select(s => CSharpSyntaxTree.ParseText(s)).Append(CSharpSyntaxTree.ParseText(testCode)).ToList()
            : [CSharpSyntaxTree.ParseText(testCode)];

        //var references = new List<MetadataReference>
        //{
        //    MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
        //};

        var references = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is String trustedPlatformAssemblies
            ? trustedPlatformAssemblies
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
                .ToList()
            : [];

        var compilation = CSharpCompilation.Create(
            "AnalyzerUnitTest",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return await compilation.WithAnalyzers([new T()]).GetAnalyzerDiagnosticsAsync();
    }
}
