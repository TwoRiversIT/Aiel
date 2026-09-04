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

using Aiel.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using static Aiel.StrongIds.Generators.Consts;

namespace Aiel.StrongIds.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class StrongIdSourceGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat TypeNameFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var candidates = context.SyntaxProvider.ForAttributeWithMetadataName(
            StrongIdAttributeMetadataName,
            static (node, _) => node is TypeDeclarationSyntax,
            static (attributeContext, _) => Transform(attributeContext));

        context.RegisterSourceOutput(candidates, static (productionContext, candidate) => Emit(productionContext, candidate));
    }

    private static StrongIdCandidate Transform(GeneratorAttributeSyntaxContext context)
    {
        return new StrongIdCandidate((INamedTypeSymbol)context.TargetSymbol, context.Attributes[0]);
    }

    private static void Emit(SourceProductionContext context, StrongIdCandidate candidate)
    {
        // Only emit code for candidates that match the exact valid shape.
        // All validation diagnostics are handled by analyzers in Aiel.StrongIds.Analyzers.
        if (!IsValidStrongIdShape(candidate.TypeSymbol))
        {
            return;
        }

        var valueType = GetBackingType(candidate.AttributeData);
        if (valueType?.IsSupportedBackingType() != true)
        {
            return;
        }

        var model = CreateModel(candidate, valueType);
        var source = Render(model);
        context.AddSource(GetHintName(model.TypeSymbol), SourceText.From(source, Encoding.UTF8));
    }

    private static StrongIdModel CreateModel(StrongIdCandidate candidate, ITypeSymbol valueType)
    {
        return new StrongIdModel(
            candidate.TypeSymbol,
            valueType,
            GetBooleanNamedArgument(candidate.AttributeData, AllowDefaultPropertyName, defaultValue: false),
            GetBooleanNamedArgument(candidate.AttributeData, GenerateTryFromPropertyName, defaultValue: true),
            GetBooleanNamedArgument(candidate.AttributeData, GenerateTryParsePropertyName, defaultValue: true),
            IsReadOnlyRecordStruct(candidate.TypeSymbol),
            GetBackingKind(candidate.AttributeData));
    }

    private static String Render(StrongIdModel model)
    {
        var builder = new StringBuilder();
        builder.AppendLine(Header(model.TypeSymbol.Name));
        builder.AppendLine("#nullable enable");
        builder.AppendLine();

        if (!model.TypeSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            builder.AppendLine($"namespace {model.TypeSymbol.ContainingNamespace.ToDisplayString()};");
            builder.AppendLine();
        }

        builder.AppendLine("using Aiel.StrongIds;");
        builder.AppendLine();

        builder.AppendLine($"//  BackingType: {model.BackingTypeName}");
        builder.AppendLine($"//         Kind: {model.BackingKind}");
        builder.AppendLine($"// DefaultValue: {model.DefaultValue}");
        builder.AppendLine($"// AllowDefault: {model.AllowDefault}");
        builder.AppendLine($"//      TryFrom: {model.GenerateTryFrom}");
        builder.AppendLine($"//     TryParse: {model.GenerateTryParse}");
        builder.AppendLine(GetTypeDeclaration(model));
        builder.AppendLine("{");
        EmitEmpty(builder, model, 1);
        builder.AppendLine($"    public {model.BackingTypeName} Value {{ get; }}");
        builder.AppendLine();

        // Constructor
        var constructorAccessibility = model.BackingKind == StrongIdBackingKindOption.Reference ? "private" : "public";
        builder.AppendLine($"    {constructorAccessibility} {model.TypeSymbol.Name}({model.BackingTypeName} {ValueParameterName})");
        builder.AppendLine("    {");
        EmitValidation(builder, model, ValueParameterName, 2);
        builder.AppendLine($"        {BackingPropertyName} = {model.NormalizedValue};");
        builder.AppendLine("    }");
        builder.AppendLine();

        builder.AppendLine($"    public static {model.TypeSymbol.Name} From({model.BackingTypeName} {ValueParameterName}) => new({ValueParameterName});");

        if (model.GenerateTryFrom)
        {
            builder.AppendLine();
            builder.AppendLine($"    public static global::System.Boolean TryFrom({model.BackingTypeName} {ValueParameterName}, out {model.TypeSymbol.Name} id)");
            builder.AppendLine("    {");
            EmitTryFrom(builder, model, ValueParameterName, 2);
            builder.AppendLine("    }");
        }

        if (model.GenerateTryParse)
        {
            builder.AppendLine();
            builder.AppendLine($"    public static global::System.Boolean TryParse(global::System.String? value, global::System.IFormatProvider? provider, out {model.TypeSymbol.Name} id)");
            builder.AppendLine("    {");
            EmitTryParse(builder, model, ParsedParameterName);
            builder.AppendLine($"        id = {GetDefaultAssignment(model)};");
            builder.AppendLine("        return false;");
            builder.AppendLine("    }");

            builder.AppendLine();
            builder.AppendLine($"    public static global::System.Boolean TryParse(global::System.String value, out {model.TypeSymbol.Name} id) => TryParse(value, null, out id);");
        }

        builder.AppendLine();
        builder.AppendLine($"    public global::System.Boolean IsDefault => {model.DefaultExpression(BackingPropertyName)};");
        builder.AppendLine();
        builder.AppendLine($"    public override global::System.String ToString() => {model.ToStringExpression};");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static void EmitEmpty(StringBuilder builder, StrongIdModel model, Int32 indentLevel)
    {
        var indent = new String(' ', indentLevel * Spaces);

        if (model.AllowDefault)
        {
            if (String.Equals(model.BackingTypeName, "global::System.String", StringComparison.Ordinal))
            {
                // String.Empty is considered a default value for string-based strong IDs.
                builder.AppendLine($"{indent}public static readonly {model.TypeSymbol.Name} None = new(global::System.String.Empty);");
            }
            else
            {
                builder.AppendLine($"{indent}public static readonly {model.TypeSymbol.Name} None = new(default);");
            }

            builder.AppendLine();
        }
    }

    private static void EmitValidation(StringBuilder builder, StrongIdModel model, String parameterName, Int32 indentLevel)
    {
        var indent = new String(' ', indentLevel * Spaces);

        if (model.AllowDefault)
        {
            // For string types, we must disallow null
            if (String.Equals(model.BackingTypeName, "global::System.String", StringComparison.Ordinal))
            {
                // String.Empty is considered a default value for string-based strong IDs, so we check for that as well as null or whitespace.
                builder.AppendLine($"{indent}if (global::System.String.IsNullOrWhiteSpace({parameterName}))");
                builder.AppendLine($"{indent}{{");
                builder.AppendLine($"{indent}    {parameterName} = global::System.String.Empty;");
                builder.AppendLine($"{indent}}}");
                builder.AppendLine();
            }

            return;
        }

        builder.AppendLine($"{indent}if ({model.InvalidValueExpression(parameterName)})");
        builder.AppendLine($"{indent}{{");
        builder.AppendLine($"{indent}    throw new global::System.ArgumentException(\"{model.ValidationErrorMessage}\", nameof({parameterName}));");
        builder.AppendLine($"{indent}}}");
        builder.AppendLine();
    }

    private static void EmitTryFrom(StringBuilder builder, StrongIdModel model, String valueParameterName, Int32 indentLevel)
    {
        var indent = new String(' ', indentLevel * Spaces);

        if (!model.AllowDefault)
        {
            builder.AppendLine($"{indent}if ({model.InvalidValueExpression(valueParameterName)})");
            builder.AppendLine($"{indent}{{");
            builder.AppendLine($"{indent}    id = {GetDefaultAssignment(model)};");
            builder.AppendLine($"{indent}    return false;");
            builder.AppendLine($"{indent}}}");
            builder.AppendLine();
        }

        builder.AppendLine($"{indent}id = new({model.AssignValue(valueParameterName)});");
        builder.AppendLine($"{indent}return true;");
    }

    private static void EmitTryParse(StringBuilder builder, StrongIdModel model, String parameterName)
    {
        switch (model.ValueType.SpecialType)
        {
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt16:
            case SpecialType.System_UInt32:
            case SpecialType.System_UInt64:
                builder.AppendLine($"        if ({model.BackingTypeName}.TryParse(value, provider, out var {parameterName}))");
                builder.AppendLine("        {");
                EmitTryFrom(builder, model, parameterName, 3);
                builder.AppendLine("        }");
                builder.AppendLine();
                break;
            case SpecialType.System_String:
                builder.AppendLine("        if (value is not null)");
                builder.AppendLine("        {");
                EmitTryFrom(builder, model, "value", 3);
                builder.AppendLine("        }");
                builder.AppendLine();
                return;

            default:
                builder.AppendLine($"        if (global::System.Guid.TryParse(value, provider, out var {parameterName}))");
                builder.AppendLine("        {");
                EmitTryFrom(builder, model, parameterName, 3);
                builder.AppendLine("        }");
                builder.AppendLine();
                return;
        }
    }

    private static String GetTypeDeclaration(StrongIdModel model)
    {
        var accessibility = model.TypeSymbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public ",
            Accessibility.Internal => "internal ",
            Accessibility.Private => "private ",
            Accessibility.Protected => "protected ",
            Accessibility.ProtectedAndInternal => "private protected ",
            Accessibility.ProtectedOrInternal => "protected internal ",
            _ => String.Empty
        };

        if (model.TypeSymbol.TypeKind == TypeKind.Struct)
        {
            var readOnlyModifier = model.IsReadOnlyRecordStruct ? "readonly " : String.Empty;
            return $"{accessibility}{readOnlyModifier}partial record struct {model.TypeSymbol.Name} : global::Aiel.StrongIds.IStrongId<{model.BackingTypeName}>";
        }

        var sealedModifier = model.TypeSymbol.IsSealed ? "sealed " : String.Empty;
        return $"{accessibility}{sealedModifier}partial record {model.TypeSymbol.Name} : global::Aiel.StrongIds.IStrongId<{model.BackingTypeName}>";
    }

    private static String GetDefaultAssignment(StrongIdModel model)
    {
        return model.BackingKind == StrongIdBackingKindOption.Reference ? "default!" : "default";
    }

    private static String GetHintName(INamedTypeSymbol symbol)
    {
        var qualifiedName = symbol.ToDisplayString(TypeNameFormat)
            .Replace("global::", String.Empty)
            .Replace('<', '_')
            .Replace('>', '_')
            .Replace('.', '_');

        return $"{qualifiedName}.StrongId.g.cs";
    }

    private static ITypeSymbol? GetBackingType(AttributeData attributeData)
    {
        return attributeData.AttributeClass?.TypeArguments.Length == 1
            ? attributeData.AttributeClass.TypeArguments[0]
            : null;
    }

    private static Boolean IsValidStrongIdShape(INamedTypeSymbol symbol)
    {
        // Must not be nested
        if (symbol.ContainingType is not null)
        {
            return false;
        }

        // Must be a record
        if (!symbol.IsRecord)
        {
            return false;
        }

        // Must be partial
        if (!IsPartial(symbol))
        {
            return false;
        }

        // Must be a struct or sealed class
        if (symbol.TypeKind == TypeKind.Struct)
        {
            return true;
        }

        return symbol.TypeKind == TypeKind.Class && symbol.IsSealed;
    }

    private static Boolean IsPartial(INamedTypeSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences
            .Select(static syntaxReference => syntaxReference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .All(static declaration => declaration.Modifiers.Any(static modifier => modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)));
    }

    private static Boolean GetBooleanNamedArgument(AttributeData attributeData, String propertyName, Boolean defaultValue)
    {
        foreach (var namedArgument in attributeData.NamedArguments)
        {
            if (String.Equals(namedArgument.Key, propertyName, StringComparison.Ordinal)
                && namedArgument.Value.Value is Boolean value)
            {
                return value;
            }
        }

        return defaultValue;
    }

    private static StrongIdBackingKindOption GetBackingKind(AttributeData attributeData)
    {
        foreach (var namedArgument in attributeData.NamedArguments)
        {
            if (String.Equals(namedArgument.Key, BackingKindPropertyName, StringComparison.Ordinal)
                && namedArgument.Value.Value is Int32 value
                && value == ReferenceBackingKindValue)
            {
                return StrongIdBackingKindOption.Reference;
            }
        }

        return StrongIdBackingKindOption.Value;
    }

    private static Boolean IsReadOnlyRecordStruct(INamedTypeSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences
            .Select(static syntaxReference => syntaxReference.GetSyntax())
            .OfType<RecordDeclarationSyntax>()
            .Any(static declaration => declaration.Modifiers.Any(static modifier => modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ReadOnlyKeyword)));
    }

    private static String Header(String passName)
    {
        return $"""
            // <auto-generated>
            //   This file was brought to you by {ThisAssembly.AssemblyName}
            //   Generator: {nameof(StrongIdSourceGenerator)}
            //   Pass: {passName}
            //
            //   DO NOT EDIT THIS FILE BY HAND OR THE WORLD MAY END!
            //   (Seriously. The generator will overwrite your changes anyway.)
            //
            // </auto-generated>

            """;
    }

    //private sealed class StrongIdCandidate(INamedTypeSymbol typeSymbol, AttributeData attributeData)
    //{
    //    public INamedTypeSymbol TypeSymbol { get; } = typeSymbol;

    //    public AttributeData AttributeData { get; } = attributeData;
    //}

    //private sealed class StrongIdModel(
    //    INamedTypeSymbol typeSymbol,
    //    ITypeSymbol valueType,
    //    Boolean allowDefault,
    //    Boolean generateTryFrom,
    //    Boolean generateTryParse,
    //    Boolean isReadOnlyRecordStruct,
    //    StrongIdBackingKindOption backingKind)
    //{
    //    public INamedTypeSymbol TypeSymbol { get; } = typeSymbol;

    //    public ITypeSymbol ValueType { get; } = valueType;

    //    public Boolean AllowDefault { get; } = allowDefault;

    //    public StrongIdBackingKindOption BackingKind { get; } = backingKind;

    //    public Boolean GenerateTryFrom { get; } = generateTryFrom;
    //    public Boolean GenerateTryParse { get; } = generateTryParse;

    //    public Boolean IsReadOnlyRecordStruct { get; } = isReadOnlyRecordStruct;

    //    public String BackingTypeName => ValueType.ToDisplayString(TypeNameFormat);

    //    public String GetInvalidValueExpression(String valueExpression)
    //        => ValueType.SpecialType switch
    //        {
    //            SpecialType.System_Int32 => $"{valueExpression} == 0",
    //            SpecialType.System_Int64 => $"{valueExpression} == 0",
    //            SpecialType.System_String => $"global::System.String.IsNullOrWhiteSpace({valueExpression})",
    //            _ => $"{valueExpression} == global::System.Guid.Empty",
    //        };

    //    public String GetStoredValueExpression(String valueExpression)
    //        => ValueType.SpecialType == SpecialType.System_String
    //            ? $"{valueExpression}.Trim()"
    //            : valueExpression;

    //    public String InvalidValueExpression => GetInvalidValueExpression("value");

    //    public String IsDefaultExpression => ValueType.SpecialType switch
    //    {
    //        SpecialType.System_Int32 => "Value == 0",
    //        SpecialType.System_Int64 => "Value == 0",
    //        SpecialType.System_String => "Value == global::System.String.Empty",
    //        _ => "Value == global::System.Guid.Empty",
    //    };

    //    public String ToStringExpression => ValueType.SpecialType == SpecialType.System_String ? "Value" : "Value.ToString()";

    //    public String StoredValueExpression => GetStoredValueExpression("value");

    //    public String ValidationErrorMessage => ValueType.SpecialType switch
    //    {
    //        SpecialType.System_Int32 => $"{TypeSymbol.Name} cannot be zero.",
    //        SpecialType.System_Int64 => $"{TypeSymbol.Name} cannot be zero.",
    //        SpecialType.System_String => $"{TypeSymbol.Name} cannot be null, empty, or whitespace.",
    //        _ => $"{TypeSymbol.Name} cannot be empty.",
    //    };
    //}

    //private enum StrongIdBackingKindOption
    //{
    //    Value = 0,
    //    Reference = 1,
    //}
}
