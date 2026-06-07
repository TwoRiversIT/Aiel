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

using Aiel.StrongIds.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Aiel.StrongIds.Generators;

[Generator]
public sealed class StrongIdSourceGenerator : IIncrementalGenerator
{
    private const String BackingKindPropertyName = "BackingKind";
    private const String DisallowDefaultPropertyName = "DisallowDefault";
    private const String GenerateTryFromPropertyName = "GenerateTryFrom";
    private const String StrongIdAttributeMetadataName = "Aiel.StrongIds.StrongIdAttribute`1";
    private const String StrongIdInterfaceMetadataName = "global::Aiel.StrongIds.IStrongId<TValue>";
    private const Int32 ReferenceBackingKindValue = 1;

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
        var diagnostics = Validate(candidate);
        if (!diagnostics.IsDefaultOrEmpty)
        {
            foreach (var diagnostic in diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }

            return;
        }

        var model = CreateModel(candidate);
        var source = Render(model);
        context.AddSource(GetHintName(model.TypeSymbol), SourceText.From(source, Encoding.UTF8));
    }

    private static ImmutableArray<Diagnostic> Validate(StrongIdCandidate candidate)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var symbol = candidate.TypeSymbol;
        var valueType = GetBackingType(candidate.AttributeData);
        var displayName = symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

        if (!IsValidStrongIdShape(symbol))
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.StrongIdMustBePartialRecordType,
                symbol.Locations.FirstOrDefault(),
                displayName));
        }

        if (UsesPositionalRecordSyntax(symbol))
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.StrongIdMustNotUsePositionalRecordSyntax,
                symbol.Locations.FirstOrDefault(),
                displayName));
        }

        if (DeclaresValueMember(symbol))
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.StrongIdMustNotDeclareValueMember,
                symbol.Locations.FirstOrDefault(),
                displayName));
        }

        if (DeclaresInstanceConstructors(symbol))
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.StrongIdMustNotDeclareInstanceConstructors,
                symbol.Locations.FirstOrDefault(),
                displayName));
        }

        if (valueType is not null && !IsSupportedBackingType(valueType))
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.StrongIdBackingTypeUnsupported,
                symbol.Locations.FirstOrDefault(),
                displayName,
                valueType.ToDisplayString(TypeNameFormat)));
        }

        return diagnostics.ToImmutable();
    }

    private static StrongIdModel CreateModel(StrongIdCandidate candidate)
    {
        var valueType = GetBackingType(candidate.AttributeData)
            ?? throw new InvalidOperationException("StrongId attribute must have a backing type.");

        return new StrongIdModel(
            candidate.TypeSymbol,
            valueType,
            GetBooleanNamedArgument(candidate.AttributeData, DisallowDefaultPropertyName, defaultValue: true),
            GetBackingKind(candidate.AttributeData),
            GetBooleanNamedArgument(candidate.AttributeData, GenerateTryFromPropertyName, defaultValue: true),
            IsReadOnlyRecordStruct(candidate.TypeSymbol));
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

        builder.AppendLine(GetTypeDeclaration(model));
        builder.AppendLine("{");
        builder.AppendLine($"    public {model.BackingTypeName} Value {{ get; }}");
        builder.AppendLine();

        var constructorAccessibility = model.BackingKind == StrongIdBackingKindOption.Reference ? "private" : "public";
        builder.AppendLine($"    {constructorAccessibility} {model.TypeSymbol.Name}({model.BackingTypeName} value)");
        builder.AppendLine("    {");
        EmitValidation(builder, model, 2);
        builder.AppendLine($"        Value = {model.StoredValueExpression};");
        builder.AppendLine("    }");
        builder.AppendLine();

        builder.AppendLine($"    public static {model.TypeSymbol.Name} From({model.BackingTypeName} value) => new(value);");

        if (model.GenerateTryFrom)
        {
            builder.AppendLine();
            builder.AppendLine($"    public static bool TryFrom({model.BackingTypeName} value, out {model.TypeSymbol.Name} id)");
            builder.AppendLine("    {");
            EmitTryCreate(builder, model, "value", 2);
            builder.AppendLine("    }");
        }

        builder.AppendLine();
        builder.AppendLine($"    public static bool TryParse(string? value, global::System.IFormatProvider? provider, out {model.TypeSymbol.Name} id)");
        builder.AppendLine("    {");
        EmitTryParse(builder, model);
        builder.AppendLine($"        id = {GetDefaultAssignment(model)};");
        builder.AppendLine("        return false;");
        builder.AppendLine("    }");

        builder.AppendLine();
        builder.AppendLine($"    public static bool TryParse(string value, out {model.TypeSymbol.Name} id) => TryParse(value, null, out id);");

        builder.AppendLine();
        builder.AppendLine($"    public bool IsDefault => {model.IsDefaultExpression};");
        builder.AppendLine();
        builder.AppendLine($"    public override string ToString() => {model.ToStringExpression};");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static void EmitValidation(StringBuilder builder, StrongIdModel model, Int32 indentLevel)
    {
        var indent = new String(' ', indentLevel * 4);

        if (!model.DisallowDefault)
        {
            // For string types, we must disallow null
            if (String.Equals(model.BackingTypeName, "global::System.String", StringComparison.Ordinal))
            {
                // String.Empty is considered a default value for string-based strong IDs, so we check for that as well as null or whitespace.
                builder.AppendLine($"{indent}if (string.IsNullOrWhiteSpace(value))");
                builder.AppendLine($"{indent}{{");
                builder.AppendLine($"{indent}    value = string.Empty;");
                builder.AppendLine($"{indent}}}");
                builder.AppendLine();
            }

            return;
        }

        builder.AppendLine($"{indent}if ({model.InvalidValueExpression})");
        builder.AppendLine($"{indent}{{");
        builder.AppendLine($"{indent}    throw new global::System.ArgumentException(\"{model.ValidationErrorMessage}\", nameof(value));");
        builder.AppendLine($"{indent}}}");
        builder.AppendLine();
    }

    private static void EmitTryCreate(StringBuilder builder, StrongIdModel model, String valueExpression, Int32 indentLevel)
    {
        var indent = new String(' ', indentLevel * 4);

        if (model.DisallowDefault)
        {
            builder.AppendLine($"{indent}if ({model.GetInvalidValueExpression(valueExpression)})");
            builder.AppendLine($"{indent}{{");
            builder.AppendLine($"{indent}    id = {GetDefaultAssignment(model)};");
            builder.AppendLine($"{indent}    return false;");
            builder.AppendLine($"{indent}}}");
            builder.AppendLine();
        }

        builder.AppendLine($"{indent}id = new({model.GetStoredValueExpression(valueExpression)});");
        builder.AppendLine($"{indent}return true;");
    }

    private static void EmitTryParse(StringBuilder builder, StrongIdModel model)
    {
        switch (model.ValueType.SpecialType)
        {
            case SpecialType.System_Int32:
                builder.AppendLine("        if (global::System.Int32.TryParse(value, provider, out var parsedValue))");
                builder.AppendLine("        {");
                EmitTryCreate(builder, model, "parsedValue", 3);
                builder.AppendLine("        }");
                builder.AppendLine();
                return;

            case SpecialType.System_Int64:
                builder.AppendLine("        if (global::System.Int64.TryParse(value, provider, out var parsedValue))");
                builder.AppendLine("        {");
                EmitTryCreate(builder, model, "parsedValue", 3);
                builder.AppendLine("        }");
                builder.AppendLine();
                return;

            case SpecialType.System_String:
                builder.AppendLine("        if (value is not null)");
                builder.AppendLine("        {");
                EmitTryCreate(builder, model, "value", 3);
                builder.AppendLine("        }");
                builder.AppendLine();
                return;

            default:
                builder.AppendLine("        if (global::System.Guid.TryParse(value, provider, out var parsedValue))");
                builder.AppendLine("        {");
                EmitTryCreate(builder, model, "parsedValue", 3);
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
            return $"{accessibility}{readOnlyModifier}partial record struct {model.TypeSymbol.Name} : IStrongId<{model.BackingTypeName}>";
        }

        var sealedModifier = model.TypeSymbol.IsSealed ? "sealed " : String.Empty;
        return $"{accessibility}{sealedModifier}partial record {model.TypeSymbol.Name} : IStrongId<{model.BackingTypeName}>";
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
        if (symbol.ContainingType is not null)
        {
            return false;
        }

        if (!symbol.IsRecord)
        {
            return false;
        }

        if (!IsPartial(symbol))
        {
            return false;
        }

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

    private static Boolean UsesPositionalRecordSyntax(INamedTypeSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences
            .Select(static syntaxReference => syntaxReference.GetSyntax())
            .OfType<RecordDeclarationSyntax>()
            .Any(static declaration => declaration.ParameterList is not null);
    }

    private static Boolean DeclaresValueMember(INamedTypeSymbol symbol)
    {
        return symbol.GetMembers("Value").Any(static member => !member.IsImplicitlyDeclared);
    }

    private static Boolean DeclaresInstanceConstructors(INamedTypeSymbol symbol)
    {
        return symbol.InstanceConstructors.Any(static constructor => !constructor.IsImplicitlyDeclared);
    }

    private static Boolean IsSupportedBackingType(ITypeSymbol valueType)
    {
        return valueType.SpecialType == SpecialType.System_Int32
            || valueType.SpecialType == SpecialType.System_Int64
            || valueType.SpecialType == SpecialType.System_String
            || String.Equals(valueType.ToDisplayString(TypeNameFormat), "global::System.Guid", StringComparison.Ordinal);
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
            //   Generator Version: {ThisAssembly.AssemblyInformationalVersion}
            //   Package Version: {ThisAssembly.NuGetPackageVersion}
            //   Generator: {nameof(StrongIdSourceGenerator)}
            //   Pass: {passName}
            //
            //   DO NOT EDIT THIS FILE BY HAND OR THE WORLD MAY END!
            //   (Seriously. The generator will overwrite your changes anyway.)
            //
            // </auto-generated>

            """;
    }

    private sealed class StrongIdCandidate(INamedTypeSymbol typeSymbol, AttributeData attributeData)
    {
        public INamedTypeSymbol TypeSymbol { get; } = typeSymbol;

        public AttributeData AttributeData { get; } = attributeData;
    }

    private sealed class StrongIdModel(
        INamedTypeSymbol typeSymbol,
        ITypeSymbol valueType,
        Boolean disallowDefault,
        StrongIdBackingKindOption backingKind,
        Boolean generateTryFrom,
        Boolean isReadOnlyRecordStruct)
    {
        public INamedTypeSymbol TypeSymbol { get; } = typeSymbol;

        public ITypeSymbol ValueType { get; } = valueType;

        public Boolean DisallowDefault { get; } = disallowDefault;

        public StrongIdBackingKindOption BackingKind { get; } = backingKind;

        public Boolean GenerateTryFrom { get; } = generateTryFrom;

        public Boolean IsReadOnlyRecordStruct { get; } = isReadOnlyRecordStruct;

        public String BackingTypeName => ValueType.ToDisplayString(TypeNameFormat);

        public String GetInvalidValueExpression(String valueExpression)
            => ValueType.SpecialType switch
            {
                SpecialType.System_Int32 => $"{valueExpression} == 0",
                SpecialType.System_Int64 => $"{valueExpression} == 0",
                SpecialType.System_String => $"string.IsNullOrWhiteSpace({valueExpression})",
                _ => $"{valueExpression} == global::System.Guid.Empty",
            };

        public String GetStoredValueExpression(String valueExpression)
            => ValueType.SpecialType == SpecialType.System_String
                ? $"{valueExpression}.Trim()"
                : valueExpression;

        public String InvalidValueExpression => GetInvalidValueExpression("value");

        public String IsDefaultExpression => ValueType.SpecialType switch
        {
            SpecialType.System_Int32 => "Value == 0",
            SpecialType.System_Int64 => "Value == 0",
            SpecialType.System_String => "Value == string.Empty",
            _ => "Value == global::System.Guid.Empty",
        };

        public String ToStringExpression => ValueType.SpecialType == SpecialType.System_String ? "Value" : "Value.ToString()";

        public String StoredValueExpression => GetStoredValueExpression("value");

        public String ValidationErrorMessage => ValueType.SpecialType switch
        {
            SpecialType.System_Int32 => $"{TypeSymbol.Name} cannot be zero.",
            SpecialType.System_Int64 => $"{TypeSymbol.Name} cannot be zero.",
            SpecialType.System_String => $"{TypeSymbol.Name} cannot be null, empty, or whitespace.",
            _ => $"{TypeSymbol.Name} cannot be empty.",
        };
    }

    private enum StrongIdBackingKindOption
    {
        Value = 0,
        Reference = 1,
    }
}
