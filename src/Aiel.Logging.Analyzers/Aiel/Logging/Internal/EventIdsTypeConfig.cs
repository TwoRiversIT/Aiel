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

using Aiel.Logging.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;

namespace Aiel.Logging.Internal;

// ════════════════════════════════════════════════════════════════════════
// EventIdsTypeConfig  — immutable value-type for resolved config
// ════════════════════════════════════════════════════════════════════════

/// <summary>
/// Holds the resolved EventIds type name, split into fully-qualified and
/// short forms.  Produced by <see cref="AnalyzerConfiguration.Resolve"/>.
/// </summary>
public sealed class EventIdsTypeConfig
{
    /// <summary>Fully-qualified type name, e.g. <c>Acme.Logging.AcmeEventIds</c>.</summary>
    public String FullTypeName { get; }

    /// <summary>Simple (unqualified) type name, e.g. <c>AcmeEventIds</c>.</summary>
    public String ShortName { get; }

    private EventIdsTypeConfig(String fullTypeName, String shortName)
    {
        FullTypeName = fullTypeName;
        ShortName = shortName;
    }

    /// <summary>
    /// Splits a fully-qualified type name into the full name and the unqualified short name.
    /// Works for both namespaced (<c>A.B.C</c>) and top-level (<c>C</c>) names.
    /// </summary>
    public static EventIdsTypeConfig FromFullTypeName(String fullTypeName)
    {
        var lastDot = fullTypeName.LastIndexOf('.');
        var shortName = lastDot >= 0
            ? fullTypeName.Substring(lastDot + 1)
            : fullTypeName;

        return new EventIdsTypeConfig(fullTypeName, shortName);
    }

    public static EventIdsTypeConfig? FromContext(SyntaxNodeAnalysisContext context)
    {
        var optionsProvider = context.Options.AnalyzerConfigOptionsProvider;

        // Prefer per-tree options if you need file-scoped values:
        var tree = context.Node.SyntaxTree;
        var treeOptions = optionsProvider.GetOptions(tree);
        if (treeOptions.TryGetValue($"build_property.{AnalyzerConfiguration.MsBuildPropertyKey}", out var configuredValueFromTree))
        {
            // use configuredValueFromTree
        }

        // Fallback to global options
        if (!optionsProvider.GlobalOptions.TryGetValue($"build_property.{AnalyzerConfiguration.MsBuildPropertyKey}", out var configuredValue))
        {
            configuredValue = "Aiel.AielEvent"; // your default
        }

        // Resolve the type by metadata name
        var compilation = context.SemanticModel.Compilation;
        var enumType = compilation.GetTypeByMetadataName(configuredValue);
        if (enumType is null)
        {
            // handle missing type (report diagnostic or fallback)
        }

        return FromFullTypeName(configuredValue);
    }

    /// <summary>
    /// Resolves this config's type to an <see cref="INamedTypeSymbol"/> in the
    /// given <paramref name="compilation"/>.  Returns <see langword="null"/> if the
    /// type cannot be found (e.g. it is not referenced by the project).
    /// </summary>
    public INamedTypeSymbol? GetTypeSymbol(Compilation compilation)
        => compilation.GetTypeByMetadataName(FullTypeName)
        ?? compilation.GetTypeByMetadataName(ShortName);
}
