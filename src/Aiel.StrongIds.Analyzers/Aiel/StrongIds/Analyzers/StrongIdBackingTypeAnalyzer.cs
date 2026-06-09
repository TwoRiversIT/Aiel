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
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Aiel.StrongIds.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StrongIdBackingTypeAnalyzer : DiagnosticAnalyzer
{
    private const String StrongIdAttributeName = "StrongIdAttribute";
    private const String StrongIdNamespace = "Aiel.StrongIds";
    private static readonly SymbolDisplayFormat TypeNameFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [DiagnosticDescriptors.StrongIdBackingTypeUnsupported];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        // Get the StrongId attribute
        var strongIdAttribute = typeSymbol.GetAttributes()
            .FirstOrDefault(static attr => IsStrongIdAttribute(attr.AttributeClass));

        if (strongIdAttribute == null)
        {
            return;
        }

        // Extract the backing type from the generic argument
        var backingType = strongIdAttribute.AttributeClass?.TypeArguments.FirstOrDefault();
        if (backingType == null)
        {
            return;
        }

        // Validate the backing type is supported
        if (!IsSupportedBackingType(backingType))
        {
            var displayName = typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.StrongIdBackingTypeUnsupported,
                typeSymbol.Locations.FirstOrDefault(),
                displayName,
                backingType.ToDisplayString(TypeNameFormat)));
        }
    }

    private static Boolean IsSupportedBackingType(ITypeSymbol valueType)
    {
        return valueType.SpecialType == SpecialType.System_Int32
            || valueType.SpecialType == SpecialType.System_Int64
            || valueType.SpecialType == SpecialType.System_String
            || String.Equals(valueType.ToDisplayString(TypeNameFormat), "global::System.Guid", StringComparison.Ordinal);
    }

    private static Boolean IsStrongIdAttribute(INamedTypeSymbol? attributeClass)
    {
        if (attributeClass is null)
        {
            return false;
        }

        var originalDefinition = attributeClass.OriginalDefinition;

        return originalDefinition.Name == StrongIdAttributeName
            && originalDefinition.Arity == 1
            && originalDefinition.ContainingNamespace.ToDisplayString() == StrongIdNamespace;
    }
}
