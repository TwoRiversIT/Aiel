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
            builder.AppendLine($"namespace {model.TypeSymbol.ContainingNamespace.ToDisplayString()}");
            builder.AppendLine("{");
        }

        builder.AppendLine($"{I(1)}using Aiel.StrongIds;");
        builder.AppendLine();

        builder.AppendLine($"{I(1)}//  BackingType: {model.BackingTypeName}");
        builder.AppendLine($"{I(1)}//         Kind: {model.BackingKind}");
        builder.AppendLine($"{I(1)}// DefaultValue: {model.DefaultValue}");
        builder.AppendLine($"{I(1)}// AllowDefault: {model.AllowEmpty}");
        builder.AppendLine($"{I(1)}//      TryFrom: {model.GenerateTryFrom}");
        builder.AppendLine($"{I(1)}//     TryParse: {model.GenerateTryParse}");

        // Open type definition
        builder.AppendLine($"{I(1)}{GetTypeDeclaration(model)}");
        builder.AppendLine($"{I(1)}{{");
        EmitEmpty(builder, model, 2);

        // Backing Property
        builder.AppendLine($"{I(2)}public {model.BackingTypeName} {BackingPropertyName} {{ get; }}");
        builder.AppendLine();

        // Constructor
        var constructorAccessibility = model.BackingKind == StrongIdBackingKindOption.Reference ? "private" : "public";
        builder.AppendLine($"{I(2)}{constructorAccessibility} {model.TypeSymbol.Name}({model.BackingTypeName} {ValueParameterName})");
        builder.AppendLine($"{I(2)}{{");
        EmitValidation(builder, model, ValueParameterName, 3);

        // Backing Property
        builder.AppendLine($"{I(3)}/// <summary>");
        builder.AppendLine($"{I(3)}/// Gets an empty <see cref=\"{model.TypeSymbol.Name}\"/> instance, which is initialized with the default value of its backing type: <see cref=\"{model.DefaultValue}\"/>");
        builder.AppendLine($"{I(3)}/// </summary>");
        builder.AppendLine($"{I(3)}{BackingPropertyName} = {model.NormalizedValue};");
        builder.AppendLine($"{I(2)}}}");
        builder.AppendLine();

        // From
        builder.AppendLine($"{I(2)}public static {model.TypeSymbol.Name} From({model.BackingTypeName} {ValueParameterName}) => new({ValueParameterName});");

        // TryFrom
        if (model.GenerateTryFrom)
        {
            builder.AppendLine();
            builder.AppendLine($"{I(2)}public static global::System.Boolean TryFrom({model.BackingTypeName} {ValueParameterName}, out {model.TypeSymbol.Name} id)");
            builder.AppendLine($"{I(2)}{{");
            EmitTryFrom(builder, model, ValueParameterName, 3);
            builder.AppendLine($"{I(2)}}}");
        }

        // TryParse
        if (model.GenerateTryParse)
        {
            builder.AppendLine();
            builder.AppendLine($"{I(2)}public static global::System.Boolean TryParse(global::System.String? {ValueParameterName}, global::System.IFormatProvider? provider, out {model.TypeSymbol.Name} id)");
            builder.AppendLine($"{I(2)}{{");
            EmitTryParse(builder, model, ParsedParameterName, 3);
            builder.AppendLine($"{I(3)}id = {GetDefaultAssignment(model)};");
            builder.AppendLine($"{I(3)}return false;");
            builder.AppendLine($"{I(2)}}}");

            builder.AppendLine();
            builder.AppendLine($"{I(2)}public static global::System.Boolean TryParse(global::System.String {ValueParameterName}, out {model.TypeSymbol.Name} id) => TryParse({ValueParameterName}, null, out id);");
        }

        // HasValue
        builder.AppendLine();
        builder.AppendLine($"{I(2)}public global::System.Boolean HasValue => {model.HasValueExpression()};");

        // ToString
        builder.AppendLine();
        builder.AppendLine($"{I(2)}public override global::System.String ToString() => {model.ToStringExpression};");

        // IEquatable<T>
        //builder.AppendLine();
        //builder.AppendLine($"{I(2)}/// <inheritdoc />");
        //builder.AppendLine($"{I(2)}public global::System.Boolean Equals({model.BackingTypeName} other) => {BackingPropertyName}.Equals(other);");
        builder.AppendLine();
        builder.AppendLine($"{I(2)}/// <inheritdoc />");
        builder.AppendLine($"{I(2)}public global::System.Boolean Equals({FqIStrongId}<{model.BackingTypeName}>? other) => other is not null && {BackingPropertyName}.Equals(other.{BackingPropertyName});");

        // IComparable<T>
        builder.AppendLine();
        builder.AppendLine($"{I(2)}/// <inheritdoc />");
        builder.AppendLine($"{I(2)}public global::System.Int32 CompareTo(global::System.Object? obj) => obj is {model.TypeSymbol.Name} id ? CompareTo(id) : 1;");
        builder.AppendLine();
        builder.AppendLine($"{I(2)}/// <inheritdoc />");
        builder.AppendLine($"{I(2)}public global::System.Int32 CompareTo({FqIStrongId}<{model.BackingTypeName}>? other) => other is {model.TypeSymbol.Name} id ? {BackingPropertyName}.CompareTo(id.{BackingPropertyName}) : 1;");
        builder.AppendLine();
        builder.AppendLine($"{I(2)}/// <inheritdoc />");
        builder.AppendLine($"{I(2)}public global::System.Int32 CompareTo({model.TypeSymbol.Name} id) => {BackingPropertyName}.CompareTo(id.{BackingPropertyName});");

        // Operators
        builder.AppendLine();
        builder.AppendLine($"{I(2)}/// <inheritdoc />");
        builder.AppendLine($"{I(2)}public static global::System.Boolean operator <({model.TypeSymbol.Name} left, {model.TypeSymbol.Name} right) => left.CompareTo(right) < 0;");
        builder.AppendLine();
        builder.AppendLine($"{I(2)}/// <inheritdoc />");
        builder.AppendLine($"{I(2)}public static global::System.Boolean operator <=({model.TypeSymbol.Name} left, {model.TypeSymbol.Name} right) => left.CompareTo(right) <= 0;");
        builder.AppendLine();
        builder.AppendLine($"{I(2)}/// <inheritdoc />");
        builder.AppendLine($"{I(2)}public static global::System.Boolean operator >({model.TypeSymbol.Name} left, {model.TypeSymbol.Name} right) => left.CompareTo(right) > 0;");
        builder.AppendLine();
        builder.AppendLine($"{I(2)}public static global::System.Boolean operator >=({model.TypeSymbol.Name} left, {model.TypeSymbol.Name} right) => left.CompareTo(right) >= 0;");

        // Coercion Operators
        //builder.AppendLine();
        //builder.AppendLine($"{I(2)}/// <inheritdoc />");
        //builder.AppendLine($"{I(2)}public static explicit operator {model.TypeSymbol.Name}({model.BackingTypeName} {ValueParameterName}) => new({ValueParameterName});");
        //builder.AppendLine();
        //builder.AppendLine($"{I(2)}/// <inheritdoc />");
        //builder.AppendLine($"{I(2)}public static explicit operator {model.BackingTypeName}({model.TypeSymbol.Name} id) => id.{BackingPropertyName};");

        // Close type definition
        builder.AppendLine($"{I(1)}}}");
        if (!model.TypeSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            builder.AppendLine("}");
        }

        return builder.ToString();
    }

    private static void EmitEmpty(StringBuilder builder, StrongIdModel model, Int32 i)
    {
        if (model.AllowEmpty)
        {
            builder.AppendLine($"{I(i)}/// <summary>");
            builder.AppendLine($"{I(i)}/// Gets an empty <see cref=\"{model.TypeSymbol.Name}\"/> instance, which is initialized with the default value of its backing type: <see cref=\"{model.DefaultValue}\"/>");
            builder.AppendLine($"{I(i)}/// </summary>");
            if (String.Equals(model.BackingTypeName, "global::System.String", StringComparison.Ordinal))
            {
                // String.Empty is considered a default value for string-based strong IDs.
                builder.AppendLine($"{I(i)}public static readonly {model.TypeSymbol.Name} Empty = new(global::System.String.Empty);");
            }
            else
            {
                builder.AppendLine($"{I(i)}public static readonly {model.TypeSymbol.Name} Empty = new(default);");
            }

            builder.AppendLine();
        }
    }

    private static void EmitValidation(StringBuilder builder, StrongIdModel model, String parameterName, Int32 i)
    {
        if (model.AllowEmpty)
        {
            // For string types, we must disallow null
            if (String.Equals(model.BackingTypeName, "global::System.String", StringComparison.Ordinal))
            {
                // String.Empty is considered a default value for string-based strong IDs, so we check for that as well as null or whitespace.
                builder.AppendLine($"{I(i)}if (global::System.String.IsNullOrWhiteSpace({parameterName}))");
                builder.AppendLine($"{I(i)}{{");
                builder.AppendLine($"{I(i + 1)}    {parameterName} = global::System.String.Empty;");
                builder.AppendLine($"{I(i)}}}");
                builder.AppendLine();
            }

            return;
        }

        builder.AppendLine($"{I(i)}if ({model.InvalidValueExpression(parameterName)})");
        builder.AppendLine($"{I(i)}{{");
        builder.AppendLine($"{I(i)}    throw new global::System.ArgumentException(\"{model.ValidationErrorMessage}\", nameof({parameterName}));");
        builder.AppendLine($"{I(i)}}}");
        builder.AppendLine();
    }

    private static void EmitTryFrom(StringBuilder builder, StrongIdModel model, String valueParameterName, Int32 i)
    {
        if (!model.AllowEmpty)
        {
            builder.AppendLine($"{I(i)}if ({model.InvalidValueExpression(valueParameterName)})");
            builder.AppendLine($"{I(i)}{{");
            builder.AppendLine($"{I(i)}    id = {GetDefaultAssignment(model)};");
            builder.AppendLine($"{I(i)}    return false;");
            builder.AppendLine($"{I(i)}}}");
            builder.AppendLine();
        }

        builder.AppendLine($"{I(i)}id = new({model.AssignValue(valueParameterName)});");
        builder.AppendLine($"{I(i)}return true;");
    }

    private static void EmitTryParse(StringBuilder builder, StrongIdModel model, String parameterName, Int32 i)
    {
        switch (model.ValueType.SpecialType)
        {
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt16:
            case SpecialType.System_UInt32:
            case SpecialType.System_UInt64:
                builder.AppendLine($"{I(i)}if ({model.BackingTypeName}.TryParse({ValueParameterName}, provider, out var {parameterName}))");
                builder.AppendLine($"{I(i)}{{");
                EmitTryFrom(builder, model, parameterName, i + 1);
                builder.AppendLine($"{I(i)}}}");
                builder.AppendLine();
                break;

            case SpecialType.System_String:
                builder.AppendLine($"{I(i)}if ({ValueParameterName} is not null)");
                builder.AppendLine($"{I(i)}{{");
                EmitTryFrom(builder, model, ValueParameterName, i + 1);
                builder.AppendLine($"{I(i)}}}");
                builder.AppendLine();
                return;

            default:
                builder.AppendLine($"{I(i)}if (global::System.Guid.TryParse({ValueParameterName}, provider, out var {parameterName}))");
                builder.AppendLine($"{I(i)}{{");
                EmitTryFrom(builder, model, parameterName, i + 1);
                builder.AppendLine($"{I(i)}}}");
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
            return $"{accessibility}{readOnlyModifier}partial record struct {model.TypeSymbol.Name} : {FqIStrongId}<{model.BackingTypeName}>";
        }

        var sealedModifier = model.TypeSymbol.IsSealed ? "sealed " : String.Empty;
        return $"{accessibility}{sealedModifier}partial record {model.TypeSymbol.Name} : {FqIStrongId}<{model.BackingTypeName}>";
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
}
