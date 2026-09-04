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

namespace Aiel.CodeAnalysis;

public static class ITypeSymbolExtensions
{
    public static Boolean IsSupportedBackingType(this ITypeSymbol valueType)
    {
        return valueType.SpecialType == SpecialType.System_Int16
            || valueType.SpecialType == SpecialType.System_UInt16
            || valueType.SpecialType == SpecialType.System_Int32
            || valueType.SpecialType == SpecialType.System_UInt32
            || valueType.SpecialType == SpecialType.System_Int64
            || valueType.SpecialType == SpecialType.System_UInt64
            || valueType.SpecialType == SpecialType.System_String
            || String.Equals(valueType.ToDisplayString(TypeNameFormat), "global::System.Guid", StringComparison.Ordinal);
    }

    public static String ToDisplayString(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(TypeNameFormat);
    }

    public static readonly SymbolDisplayFormat TypeNameFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
}
