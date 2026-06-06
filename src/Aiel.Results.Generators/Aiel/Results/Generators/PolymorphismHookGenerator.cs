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

[Generator]
public sealed class PolymorphismHookGenerator : SourceGeneratorBase, IIncrementalGenerator
{
    protected override void Generate(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> symbols)
    {
        if (symbols.Length == 0)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine(Header(nameof(PolymorphismHookGenerator)));
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine("using System.Text.Json.Serialization.Metadata;");
        sb.AppendLine();
        sb.AppendLine($"namespace {GeneratorConsts.Root};");
        sb.AppendLine();
        sb.AppendLine($"internal static class {GeneratorConsts.GeneratedPolymorphism}");
        sb.AppendLine("{");
        sb.AppendLine("    private const String NotEmpty = \"NotEmpty\";");
        sb.AppendLine();

        sb.AppendLine("    [ModuleInitializer]");
        sb.AppendLine($"    public static void {GeneratorConsts.Initializer}()");
        sb.AppendLine("    {");
        sb.AppendLine("        // Ensure the static constructors are called to register them in the error code registry.");
        foreach (var error in symbols.Select(ErrorInf.FromSymbol))
        {
            sb.AppendLine($"        _ = new {error.FqErrorName}(NotEmpty);");
        }

        sb.AppendLine();
        sb.AppendLine($"        {GeneratorConsts.FqPolymorphismType}.{GeneratorConsts.RegisterMethod}(Configure);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static void Configure(JsonTypeInfo ti)");
        sb.AppendLine("    {");

        sb.AppendLine();
        //sb.AppendLine($"        var errorCodeType = typeof({GeneratorConsts.FqErrorCodeType});");
        //sb.AppendLine($"        if (errorCodeType.IsAssignableFrom(ti.Type))");
        //sb.AppendLine($"        {{");
        sb.AppendLine("            ti.PolymorphismOptions ??= new JsonPolymorphismOptions");
        sb.AppendLine("            {");
        sb.AppendLine($"                {GeneratorConsts.TypeDiscriminatorPropertyName} = \"$code\"");
        sb.AppendLine("            };");
        sb.AppendLine();

        foreach (var error in symbols.Select(ErrorInf.FromSymbol))
        {
            sb.AppendLine("            ti.PolymorphismOptions.DerivedTypes.Add(");
            sb.AppendLine($"                new JsonDerivedType(typeof({error.FqErrorCodeName}),");
            sb.AppendLine($"                typeDiscriminator: \"{error.TypeDiscriminator}\")");
            sb.AppendLine("            );");
            sb.AppendLine();
        }

        sb.AppendLine("            throw new Exception();");
        //sb.AppendLine($"        }}");

        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource(GeneratorConsts.PolymorphismFilename, sb.ToString());
    }
}
