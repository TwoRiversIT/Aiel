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
using System.Reflection;
using static Aiel.Framework.Stubs.Stubs;

namespace Aiel.Framework.Verifiers;

public static class SourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : IIncrementalGenerator, new()
{
    public static GeneratorResult Generate(String testCode, Boolean includeHostApplication = false, Boolean includeWebApplication = false, Boolean includeWebAssembly = false)
    {
        var syntaxTrees = new List<SyntaxTree>
            {
                CSharpSyntaxTree.ParseText(testCode)
            };

        //foreach (var dependency in AielDependencies)
        //{
        //    syntaxTrees.Add(CSharpSyntaxTree.ParseText(dependency));
        //}

        if (includeHostApplication)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(HostApplication));
        }

        if (includeWebApplication)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(WebApplication));
        }

        if (includeWebAssembly)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(WebAssembly));
        }

        var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(AielFrameworkAbstractions).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(String).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new TSourceGenerator();

        var runResult = CSharpGeneratorDriver
            .Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics)
            .GetRunResult();

        return new GeneratorResult(
            runResult.GeneratedTrees
                .Select(t => new Tree(t.FilePath, t.GetText()))
                .ToImmutableArray(),
            diagnostics);
    }

    public sealed record Tree(String HintName, SourceText SourceText);

    public sealed record GeneratorResult(ImmutableArray<Tree> GeneratedSources, ImmutableArray<Diagnostic> Diagnostics);
}
