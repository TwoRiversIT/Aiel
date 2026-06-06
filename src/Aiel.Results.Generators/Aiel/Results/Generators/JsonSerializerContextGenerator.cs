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

using Aiel.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace Aiel.Results.Generators;

//[Generator]
public sealed class JsonSerializerContextGenerator : SourceGeneratorBase, IIncrementalGenerator
{
    protected override void Generate(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> symbols)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Header(nameof(JsonSerializerContextGenerator)));

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Text.Json.Serialization;");
        sb.AppendLine();

        foreach (var error in symbols.Select(ErrorInf.FromSymbol))
        {
            var spaces = "";

            if (error.HasNamespace)
            {
                sb.AppendLine($"namespace {error.Namespace}");
                sb.AppendLine("{");
                spaces = "    ";
            }

            sb.AppendLine($"{spaces}[JsonSerializable(typeof({error.ErrorName}))]");
            sb.AppendLine($"{spaces}[JsonSerializable(typeof({error.ErrorAndCodeName}))]");
            sb.AppendLine($"{spaces}public partial class {error.ErrorName}{GeneratorConsts.Resolver} : JsonSerializerContext {{ }}");

            if (error.HasNamespace)
            {
                sb.AppendLine("}");
            }

            sb.AppendLine();
        }

        context.AddSource(GeneratorConsts.JsonTypeInfoResolverFilename, sb.ToString());
    }
}
