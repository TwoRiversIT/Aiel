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
public sealed partial class TypeInfoResolverGenerator : SourceGeneratorBase, IIncrementalGenerator
{
    protected override void Generate(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> symbols)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Header(nameof(ErrorClassGenerator)));

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Text.Json;");
        sb.AppendLine("using System.Text.Json.Serialization;");
        sb.AppendLine("using System.Text.Json.Serialization.Metadata;");
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

            sb.AppendLine($"{spaces}public sealed class {error.ErrorName}JsonResolver : IJsonTypeInfoResolver");
            sb.AppendLine($"{spaces}{{");
            sb.AppendLine($"{spaces}    private static readonly IJsonTypeInfoResolver _fallback = new DefaultJsonTypeInfoResolver();");
            sb.AppendLine("");
            sb.AppendLine($"{spaces}    public JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)");
            sb.AppendLine($"{spaces}    {{");
            sb.AppendLine($"{spaces}        if (type == typeof({error.ErrorName}))");
            sb.AppendLine($"{spaces}        {{");
            sb.AppendLine($"{spaces}            return JsonMetadataServices.CreateObjectInfo<{error.ErrorName}>(options,");
            sb.AppendLine($"{spaces}                createObject: () => new {error.ErrorName}(\"\"),");
            sb.AppendLine();
            sb.AppendLine($"{spaces}            properties: new JsonPropertyInfo[] {{");
            sb.AppendLine($"{spaces}                JsonMetadataServices.CreatePropertyInfo<String>(options,");
            sb.AppendLine($"{spaces}                    isProperty: true,");
            sb.AppendLine($"{spaces}                    declaringType: typeof({error.ErrorName}),");
            sb.AppendLine($"{spaces}                    propertyType: typeof(String),");
            sb.AppendLine($"{spaces}                    propertyName: \"Description\",");
            sb.AppendLine($"{spaces}                getter: obj => (({error.ErrorName})obj).Description,");
            sb.AppendLine($"{spaces}                setter: (obj, value) => (({error.ErrorName})obj).Description = value!)");
            sb.AppendLine($"{spaces}            }},");
            sb.AppendLine($"{spaces}            objectType: typeof({error.ErrorName}));");
            sb.AppendLine($"{spaces}        }}");
            sb.AppendLine();
            sb.AppendLine($"{spaces}        if (type == typeof({error.ErrorName}.{error.ErrorCodeName}))");
            sb.AppendLine($"{spaces}        {{");
            sb.AppendLine($"{spaces}            return JsonMetadataServices.CreateObjectInfo<{error.ErrorName}.{error.ErrorCodeName}>(options,");
            sb.AppendLine($"{spaces}                createObject: () => {error.ErrorName}.{error.ErrorCodeName}.Instance, properties: Array.Empty<JsonPropertyInfo>(),");
            sb.AppendLine($"{spaces}                objectType: typeof({error.ErrorName}.{error.ErrorCodeName}));");
            sb.AppendLine($"{spaces}        }}");
            sb.AppendLine();
            sb.AppendLine($"{spaces}        return _fallback.GetTypeInfo(type, options);");
            sb.AppendLine($"{spaces}    }}");
            sb.AppendLine($"{spaces}}}");

            if (error.HasNamespace)
            {
                sb.AppendLine("}");
            }

            sb.AppendLine();

        }

        context.AddSource(GeneratorConsts.JsonTypeInfoResolverFilename, sb.ToString());
    }
}
