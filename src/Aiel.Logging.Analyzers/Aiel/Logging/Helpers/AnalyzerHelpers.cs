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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Aiel.Logging.Helpers;

public static class AnalyzerHelpers
{
    // ── Symbol helpers ───────────────────────────────────────────────────

    /// <summary>
    /// Returns true when <paramref name="type"/> IS or IMPLEMENTS
    /// <c>Microsoft.Extensions.Logging.ILogger</c> (open or closed).
    /// </summary>
    public static bool IsILogger(ITypeSymbol type, Compilation compilation)
    {
        var ilogger = compilation.GetTypeByMetadataName(WellKnownTypes.ILogger);
        var iloggerT = compilation.GetTypeByMetadataName(WellKnownTypes.ILoggerOfT);

        if (ilogger is null && iloggerT is null)
        {
            return false;
        }

        // Direct match
        if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, ilogger)
            || SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, iloggerT))
        {
            return true;
        }

        // Implemented interfaces
        return type.AllInterfaces.Any(i =>
            SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, ilogger)
            || SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, iloggerT));
    }

    /// <summary>
    /// Returns true when <paramref name="symbol"/> has
    /// <c>[LoggerMessage]</c> applied.
    /// </summary>
    public static bool HasLoggerMessageAttribute(ISymbol symbol, Compilation compilation)
    {
        var attrType = compilation.GetTypeByMetadataName(WellKnownTypes.LoggerMessageAttr);
        if (attrType is null)
        {
            return false;
        }

        return symbol.GetAttributes().Any(a =>
            SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrType));
    }

    /// <summary>
    /// Retrieves the <c>[LoggerMessage]</c> attribute data from <paramref name="symbol"/>,
    /// or <see langword="null"/> if not present.
    /// </summary>
    public static AttributeData? GetLoggerMessageAttribute(ISymbol symbol, Compilation compilation)
    {
        var attrType = compilation.GetTypeByMetadataName(WellKnownTypes.LoggerMessageAttr);
        if (attrType is null)
        {
            return null;
        }

        return symbol.GetAttributes().FirstOrDefault(a =>
            SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrType));
    }

    /// <summary>
    /// Tries to resolve the <c>AielEventIds</c> enum type from the compilation.
    /// Returns <see langword="null"/> when the type is not referenced.
    /// </summary>
    public static INamedTypeSymbol? GetAielEventIdsType(Compilation compilation)
        => compilation.GetTypeByMetadataName(WellKnownTypes.AielEventIds);

    // ── Named-argument helpers ───────────────────────────────────────────

    /// <summary>
    /// Extracts a named argument value from <paramref name="attrData"/>
    /// by <paramref name="argName"/> (case-insensitive).
    /// </summary>
    public static TypedConstant? GetNamedArgument(AttributeData attrData, string argName)
    {
        foreach (var entry in attrData.NamedArguments)
        {
            var key = entry.Key;
            var value = entry.Value;
            if (string.Equals(key, argName, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        return null;
    }

    // ── Syntax helpers ───────────────────────────────────────────────────

    /// <summary>
    /// Returns the <see cref="AttributeSyntax"/> node for the first attribute
    /// whose resolved type matches <paramref name="attrFullName"/>.
    /// </summary>
    public static AttributeSyntax? FindAttributeSyntax(
        SyntaxList<AttributeListSyntax> attrLists,
        SemanticModel semanticModel,
        string attrFullName)
    {
        foreach (var list in attrLists)
        {
            foreach (var attr in list.Attributes)
            {
                var sym = semanticModel.GetSymbolInfo(attr).Symbol;
                if (sym?.ContainingType?.ToDisplayString() == attrFullName)
                {
                    return attr;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Given the <c>EventId</c> integer value in a <c>[LoggerMessage]</c>
    /// attribute and the <c>AielEventIds</c> type, attempts to find the matching
    /// enum member name (e.g. <c>"ModuleStart"</c>).
    /// </summary>
    public static string? TryResolveEventIdMemberName(int eventIdValue, INamedTypeSymbol aielEventIdsType)
    {
        foreach (var member in aielEventIdsType.GetMembers().OfType<IFieldSymbol>())
        {
            if (member.HasConstantValue && Convert.ToInt32(member.ConstantValue) == eventIdValue)
            {
                return member.Name;
            }
        }

        return null;
    }
}

