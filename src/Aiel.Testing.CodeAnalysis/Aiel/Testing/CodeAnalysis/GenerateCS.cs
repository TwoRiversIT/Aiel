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
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Aiel.Testing.CodeAnalysis;

public static class GenerateCS<T>
     where T : IIncrementalGenerator, new()
{
    public static GeneratorRunResult Generate(String testCode, IReadOnlyCollection<String>? stubs = null, IReadOnlyCollection<String>? ignored = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(testCode);

        var trees = stubs?.Count > 0
            ? stubs.Select(s => CSharpSyntaxTree.ParseText(s)).Append(CSharpSyntaxTree.ParseText(testCode)).ToList()
            : [CSharpSyntaxTree.ParseText(testCode)];

        var references = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is String trustedPlatformAssemblies
            ? trustedPlatformAssemblies
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
                .ToList()
            : [];

        var specificDiagnosticOptions = new Dictionary<String, ReportDiagnostic>();
        foreach (var id in ignored ?? [])
        {
            specificDiagnosticOptions[id] = ReportDiagnostic.Suppress;
        }

        var compilation = CSharpCompilation.Create(
            "GeneratorUnitTests",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithSpecificDiagnosticOptions(specificDiagnosticOptions));

        var runResult = CSharpGeneratorDriver
            .Create(new T())
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics)
            .GetRunResult();

        return new GeneratorRunResult(
            runResult.GeneratedTrees.Select(static tree => new GeneratedSource(tree.FilePath, tree.GetText())).ToImmutableArray(),
            generatorDiagnostics,
            outputCompilation.GetDiagnostics(),
            outputCompilation);
    }
}

public sealed record GeneratedSource(String FilePath, SourceText Source);

public sealed record GeneratorRunResult(
    ImmutableArray<GeneratedSource> GeneratedSources,
    ImmutableArray<Diagnostic> GeneratorDiagnostics,
    ImmutableArray<Diagnostic> CompilationDiagnostics,
    Compilation OutputCompilation);
