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

using Aiel.Internal;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace Aiel.Results.Generators;

[Generator]
public sealed partial class ErrorClassGenerator : SourceGeneratorBase, IIncrementalGenerator
{
    protected override void Generate(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> symbols)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Header(nameof(ErrorClassGenerator)));

        sb.AppendLine("using System;");
        sb.AppendLine();

        sb.AppendLine("#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member");
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

            sb.AppendLine($"{spaces}public partial class {error.ErrorName} : {GeneratorConsts.FqErrorType}");
            sb.AppendLine($"{spaces}{{");

            sb.AppendLine($"{spaces}    static {error.ErrorName}()");
            sb.AppendLine($"{spaces}    {{");
            sb.AppendLine($"{spaces}        {GeneratorConsts.FqRegistryType}.{GeneratorConsts.RegisterMethod}<{error.FqErrorName}>();");
            sb.AppendLine($"{spaces}    }}");
            sb.AppendLine();

            sb.AppendLine($"{spaces}    public {error.ErrorName}(String {GeneratorConsts.MessageParameter})");
            sb.AppendLine($"{spaces}        : base({error.ErrorCodeName}.{GeneratorConsts.Instance}, {GeneratorConsts.MessageParameter}) {{ }}");
            sb.AppendLine();

            sb.AppendLine($"{spaces}    public sealed class {error.ErrorCodeName} : {GeneratorConsts.FqErrorCodeType}");
            sb.AppendLine($"{spaces}    {{");
            sb.AppendLine($"{spaces}        public static readonly {error.ErrorCodeName} {GeneratorConsts.Instance} = new();");
            sb.AppendLine($"{spaces}        protected override String Name => \"{error.ErrorName}\";");
            sb.AppendLine($"{spaces}    }}");
            sb.AppendLine($"{spaces}}}");

            if (error.HasNamespace)
            {
                sb.AppendLine("}");
            }

            sb.AppendLine();
        }

        sb.AppendLine("#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member");

        context.AddSource(GeneratorConsts.ErrorCodeFilename, sb.ToString());
    }
}
