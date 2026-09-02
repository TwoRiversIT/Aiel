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

// -----------------------------------------------------------------------
// AnalyzerHelpers.cs
// Shared utility methods used across multiple analyzer implementations.
//
// NOTE: There is no longer a GetAielEventIdsType(Compilation) helper here.
//       Use  config.GetTypeSymbol(compilation)  instead, where  config  is
//       the EventIdsTypeConfig resolved by AnalyzerConfiguration.Resolve().
// -----------------------------------------------------------------------

using Aiel.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Aiel.Logging.Internal;

public static class AnalyzerHelpers
{
    // ── ILogger helpers ──────────────────────────────────────────────────

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="type"/> IS or
    /// IMPLEMENTS <c>Microsoft.Extensions.Logging.ILogger</c> (open or closed).
    /// </summary>
    public static Boolean IsILogger(ITypeSymbol type, Compilation compilation)
    {
        var ilogger = compilation.GetTypeByMetadataName(WellKnownTypes.ILogger);
        var iloggerT = compilation.GetTypeByMetadataName(WellKnownTypes.ILoggerOfT);
        if (ilogger is null && iloggerT is null)
        {
            return false;
        }

        if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, ilogger)
         || SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, iloggerT))
        {
            return true;
        }

        return type.AllInterfaces.Any(i =>
            SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, ilogger)
         || SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, iloggerT));
    }

    // ── LoggerMessage attribute helpers ─────────────────────────────────

    /// <summary>Returns <see langword="true"/> when <c>[LoggerMessage]</c> is present on <paramref name="symbol"/>.</summary>
    public static Boolean HasLoggerMessageAttribute(ISymbol symbol, Compilation compilation)
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
    /// Returns the <c>[LoggerMessage]</c> <see cref="AttributeData"/> from
    /// <paramref name="symbol"/>, or <see langword="null"/> if absent.
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

    // ── Named-argument helpers ───────────────────────────────────────────

    /// <summary>
    /// Extracts a named argument value from <paramref name="attrData"/>
    /// by <paramref name="argName"/> (case-insensitive).
    /// </summary>
    public static TypedConstant? GetNamedArgument(AttributeData attrData, String argName)
    {
        foreach (var kvp in attrData.NamedArguments)
        {
            if (String.Equals(kvp.Key, argName, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        return null;
    }

    // ── Syntax helpers ───────────────────────────────────────────────────

    /// <summary>
    /// Returns the <see cref="AttributeSyntax"/> node whose resolved type
    /// matches <paramref name="attrFullName"/>.
    /// </summary>
    public static AttributeSyntax? FindAttributeSyntax(
        SyntaxList<AttributeListSyntax> attrLists,
        SemanticModel semanticModel,
        String attrFullName)
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

    // ── EventIds member resolution ───────────────────────────────────────

    /// <summary>
    /// Given an integer event-ID value and the EventIds enum type, returns
    /// the matching enum member name (e.g. <c>"ModuleStart"</c>), or
    /// <see langword="null"/> when no member has that value.
    /// </summary>
    public static String? TryResolveMemberName(Int32 eventIdValue, INamedTypeSymbol eventIdsType)
    {
        foreach (var member in eventIdsType.GetMembers().OfType<IFieldSymbol>())
        {
            if (member.HasConstantValue && Convert.ToInt32(member.ConstantValue) == eventIdValue)
            {
                return member.Name;
            }
        }

        return null;
    }

    // ── Cast-expression validation ───────────────────────────────────────

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="expr"/> is a
    /// <c>(int)EnumType.Member</c> cast where <c>EnumType</c> is the
    /// configured EventIds type.
    /// </summary>
    public static Boolean IsConfiguredEventIdsCast(
        ExpressionSyntax expr,
        SemanticModel model,
        EventIdsTypeConfig config)
    {
        if (expr is not CastExpressionSyntax cast)
        {
            return false;
        }

        var castType = model.GetTypeInfo(cast.Type).Type;
        if (castType?.SpecialType != SpecialType.System_Int32)
        {
            return false;
        }

        if (cast.Expression is not MemberAccessExpressionSyntax ma)
        {
            return false;
        }

        var configuredType = config.GetTypeSymbol(model.Compilation);
        if (configuredType is null)
        {
            return false;
        }

        var memberType = model.GetTypeInfo(ma.Expression).Type;
        return SymbolEqualityComparer.Default.Equals(memberType, configuredType)
            || SymbolEqualityComparer.Default.Equals(memberType?.OriginalDefinition, configuredType);
    }
}
