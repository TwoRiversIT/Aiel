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
using static Aiel.StrongIds.Generators.Consts;

namespace Aiel.StrongIds.Generators;

public sealed class StrongIdCandidate(INamedTypeSymbol typeSymbol, AttributeData attributeData)
{
    public INamedTypeSymbol TypeSymbol { get; } = typeSymbol;

    public AttributeData AttributeData { get; } = attributeData;
}

public sealed class StrongIdModel(
    INamedTypeSymbol typeSymbol,
    ITypeSymbol valueType,
    Boolean allowDefault,
    Boolean generateTryFrom,
    Boolean generateTryParse,
    Boolean isReadOnlyRecordStruct,
    StrongIdBackingKindOption backingKind)
{
    public INamedTypeSymbol TypeSymbol { get; } = typeSymbol;

    public ITypeSymbol ValueType { get; } = valueType;

    public Boolean AllowDefault { get; } = allowDefault;

    public StrongIdBackingKindOption BackingKind { get; } = backingKind;

    public Boolean GenerateTryFrom { get; } = generateTryFrom;
    public Boolean GenerateTryParse { get; } = generateTryParse;

    public Boolean IsReadOnlyRecordStruct { get; } = isReadOnlyRecordStruct;

    public String BackingTypeName => ValueType.ToDisplayString(Consts.TypeNameFormat);

    public String InvalidValueExpression(String valueParameterName)
        => ValueType.SpecialType switch
        {
            SpecialType.System_Int16 => $"{valueParameterName} == {DefaultValue}",
            SpecialType.System_UInt16 => $"{valueParameterName} == {DefaultValue}",
            SpecialType.System_Int32 => $"{valueParameterName} == {DefaultValue}",
            SpecialType.System_UInt32 => $"{valueParameterName} == {DefaultValue}",
            SpecialType.System_Int64 => $"{valueParameterName} == {DefaultValue}",
            SpecialType.System_UInt64 => $"{valueParameterName} == {DefaultValue}",
            SpecialType.System_String => $"global::System.String.IsNullOrWhiteSpace({valueParameterName})",
            _ => $"{valueParameterName} == global::System.Guid.Empty",
        };

    public String AssignValue(String valueParameterName)
        => ValueType.SpecialType == SpecialType.System_String
            ? $"{valueParameterName}.Trim()"
            : valueParameterName;

    public String DefaultValue
        => ValueType.SpecialType == SpecialType.System_String
            ? "global::System.String.Empty"
            : "default";

    public String InvalidValue => InvalidValueExpression(ParsedParameterName);

    public String DefaultExpression(String parameterName) => ValueType.SpecialType switch
    {
        SpecialType.System_Int16 => $"{parameterName} == 0",
        SpecialType.System_UInt16 => $"{parameterName} == 0",
        SpecialType.System_Int32 => $"{parameterName} == 0",
        SpecialType.System_UInt32 => $"{parameterName} == 0",
        SpecialType.System_Int64 => $"{parameterName} == 0",
        SpecialType.System_UInt64 => $"{parameterName} == 0",
        SpecialType.System_String => $"{parameterName} == global::System.String.Empty",
        _ => $"{parameterName} == global::System.Guid.Empty",
    };

    public String ToStringExpression => ValueType.SpecialType == SpecialType.System_String ? BackingPropertyName : BackingPropertyName + ".ToString()";

    public String NormalizedValue
        => ValueType.SpecialType == SpecialType.System_String
            ? $"{ValueParameterName}.Trim()"
            : ValueParameterName;

    public String ValidationErrorMessage => ValueType.SpecialType switch
    {
        SpecialType.System_Int16 => $"{TypeSymbol.Name} cannot be zero.",
        SpecialType.System_UInt16 => $"{TypeSymbol.Name} cannot be zero.",
        SpecialType.System_Int32 => $"{TypeSymbol.Name} cannot be zero.",
        SpecialType.System_UInt32 => $"{TypeSymbol.Name} cannot be zero.",
        SpecialType.System_Int64 => $"{TypeSymbol.Name} cannot be zero.",
        SpecialType.System_UInt64 => $"{TypeSymbol.Name} cannot be zero.",
        SpecialType.System_String => $"{TypeSymbol.Name} cannot be null, empty, or whitespace.",
        _ => $"{TypeSymbol.Name} cannot be empty.",
    };
}

public enum StrongIdBackingKindOption
{
    Value = 0,
    Reference = 1,
}
