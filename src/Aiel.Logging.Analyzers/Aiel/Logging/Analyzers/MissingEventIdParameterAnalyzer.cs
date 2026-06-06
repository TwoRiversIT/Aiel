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

using Aiel.Logging.Helpers;
using Aiel.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Aiel.Logging.Analyzers;

/// <summary>
/// AIEL00009 – Every <c>[LoggerMessage]</c>-decorated method must expose an
/// optional <c>&lt;EventIdsType&gt; eventId = &lt;EventIdsType&gt;.X</c>
/// parameter so that callers can override the default event ID.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingEventIdParameterAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [DiagnosticDescriptors.MissingEventIdParameter];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    // ── Core analysis ────────────────────────────────────────────────────

    private static void AnalyzeMethod(SymbolAnalysisContext ctx)
    {
        var method = (IMethodSymbol)ctx.Symbol;

        // Only inspect [LoggerMessage]-decorated methods.
        if (!AnalyzerHelpers.HasLoggerMessageAttribute(method, ctx.Compilation))
        {
            return;
        }

        // Retrieve the AielEventIds type; if the framework is not referenced we
        // skip silently (avoids noise in non-Aiel projects that happen to use the analyzer).
        var aielType = AnalyzerHelpers.GetAielEventIdsType(ctx.Compilation);
        if (aielType is null)
        {
            return;
        }

        // Look for an optional parameter whose type IS AielEventIds.
        var eventIdParam = FindEventIdParameter(method, aielType);
        if (eventIdParam is not null)
        {
            return; // ← compliant
        }

        // Determine the expected default member name from the attribute.
        var attrData = AnalyzerHelpers.GetLoggerMessageAttribute(method, ctx.Compilation);
        var suggestedMember = TrySuggestMember(attrData, aielType);

        // Report on the [LoggerMessage] attribute when possible.
        var location = attrData?.ApplicationSyntaxReference?.GetSyntax(ctx.CancellationToken)
            .GetLocation()
            ?? method.Locations.FirstOrDefault()
            ?? Location.None;

        ctx.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.MissingEventIdParameter,
            location,
            method.Name,
            suggestedMember ?? "SomeMember"));
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Finds a parameter that:
    ///   1. Has type <c>AielEventIds</c>.
    ///   2. Is named <c>eventId</c> (case-insensitive).
    ///   3. Has a default value (i.e. is optional).
    /// </summary>
    private static IParameterSymbol? FindEventIdParameter(
        IMethodSymbol method, INamedTypeSymbol aielType)
    {
        foreach (var param in method.Parameters)
        {
            if (!SymbolEqualityComparer.Default.Equals(param.Type, aielType))
            {
                continue;
            }

            if (!string.Equals(param.Name, WellKnownTypes.EventIdParamName,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (param.HasExplicitDefaultValue)
            {
                return param;
            }
        }

        return null;
    }

    private static string? TrySuggestMember(AttributeData? attrData, INamedTypeSymbol aielType)
    {
        if (attrData is null)
        {
            return null;
        }

        // Try named arg first: EventId = (int)AielEventIds.X  →  integer constant
        var arg = AnalyzerHelpers.GetNamedArgument(attrData, WellKnownTypes.EventIdArgName);
        if (arg is null)
        {
            return null;
        }

        if (arg.Value.Value is int intVal)
        {
            return AnalyzerHelpers.TryResolveEventIdMemberName(intVal, aielType);
        }

        return null;
    }
}

