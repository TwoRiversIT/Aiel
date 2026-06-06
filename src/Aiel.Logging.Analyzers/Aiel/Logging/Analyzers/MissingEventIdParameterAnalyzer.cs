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
// MissingEventIdParameterAnalyzer.cs  –  AIEL00009
//
// Reports when a [LoggerMessage]-decorated method does NOT include an
// optional parameter of the configured EventIds type named "eventId".
//
// The EventIds enum type is resolved from AnalyzerConfiguration so that
// teams using a custom enum (e.g. AcmeEventIds) get the same enforcement.
// -----------------------------------------------------------------------

using Aiel.Logging.Configuration;
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

        context.RegisterCompilationStartAction(compilationCtx =>
        {
            compilationCtx.RegisterSymbolAction(
                ctx => AnalyzeSymbol(ctx),
                SymbolKind.Method);
        });
    }

    // ── Core analysis ────────────────────────────────────────────────────

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var config = AnalyzerConfiguration.Resolve(context.Options);

        var method = (IMethodSymbol)context.Symbol;
        if (!AnalyzerHelpers.HasLoggerMessageAttribute(method, context.Compilation))
        {
            return;
        }

        config = context.ResolveConfigForSymbol(method, config);

        var eventIdsType = config.GetTypeSymbol(context.Compilation);
        if (eventIdsType is null)
        {
            return;
        }

        if (FindEventIdParameter(method, eventIdsType) is not null)
        {
            return;
        }

        var attrData = AnalyzerHelpers.GetLoggerMessageAttribute(method, context.Compilation);
        var suggestedMember = TrySuggestMember(attrData, eventIdsType);
        var props = AnalyzerConfiguration.BuildDiagnosticProperties(config);
        var location = method.Locations.FirstOrDefault() ?? Location.None;

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.MissingEventIdParameter,
            location,
            properties: props,
            method.Name,
            suggestedMember ?? AnalyzerConfiguration.DefaultMemberName));
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Finds a parameter that:
    ///   1. Has the configured EventIds enum type.
    ///   2. Is named "eventId" (case-insensitive).
    ///   3. Has an explicit default value.
    /// </summary>
    private static IParameterSymbol? FindEventIdParameter(
        IMethodSymbol method, INamedTypeSymbol eventIdsType)
    {
        foreach (var param in method.Parameters)
        {
            if (!SymbolEqualityComparer.Default.Equals(param.Type, eventIdsType))
            {
                continue;
            }

            if (!String.Equals(param.Name, WellKnownTypes.EventIdParamName,
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

    private static String? TrySuggestMember(AttributeData? attrData, INamedTypeSymbol eventIdsType)
    {
        if (attrData is null)
        {
            return null;
        }

        var arg = AnalyzerHelpers.GetNamedArgument(attrData, WellKnownTypes.EventIdArgName);
        if (arg?.Value is Int32 intVal)
        {
            return AnalyzerHelpers.TryResolveMemberName(intVal, eventIdsType);
        }

        return null;
    }
}
