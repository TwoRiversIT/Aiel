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
/// AIEL00012 – The numeric <c>EventId</c> in <c>[LoggerMessage]</c> must match
/// the integer value of the default for the <c>&lt;EventIdsType&gt; eventId</c>
/// parameter on the same method.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EventIdMismatchAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [DiagnosticDescriptors.EventIdMismatch];

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
        var attrData = AnalyzerHelpers.GetLoggerMessageAttribute(method, ctx.Compilation);
        if (attrData is null)
        {
            return;
        }

        // We need the AielEventIds type to inspect parameter default values.
        var aielType = AnalyzerHelpers.GetAielEventIdsType(ctx.Compilation);
        if (aielType is null)
        {
            return;
        }

        // ── 1. Read EventId from the attribute ───────────────────────────
        var attrEventIdArg = AnalyzerHelpers.GetNamedArgument(attrData, WellKnownTypes.EventIdArgName);
        if (attrEventIdArg is null || attrEventIdArg.Value.Value is not int attrEventIdValue)
        {
            return; // Attribute EventId not resolvable – let AIEL001 handle it
        }

        // ── 2. Read default value of the "eventId" parameter ────────────
        IParameterSymbol? eventIdParam = null;
        foreach (var p in method.Parameters)
        {
            if (SymbolEqualityComparer.Default.Equals(p.Type, aielType)
                && string.Equals(p.Name, WellKnownTypes.EventIdParamName,
                    StringComparison.OrdinalIgnoreCase)
                && p.HasExplicitDefaultValue)
            {
                eventIdParam = p;
                break;
            }
        }

        // AIEL00009 covers the missing-parameter case; we don't double-report.
        if (eventIdParam is null)
        {
            return;
        }

        // The default value of an enum parameter is its underlying integer.
        if (eventIdParam.ExplicitDefaultValue is not int paramDefaultValue)
        {
            return;
        }

        // ── 3. Compare ───────────────────────────────────────────────────
        if (attrEventIdValue == paramDefaultValue)
        {
            return; // ← all good
        }

        // Resolve display names for the diagnostic message.
        var attrMember = AnalyzerHelpers.TryResolveEventIdMemberName(attrEventIdValue, aielType)
                           ?? attrEventIdValue.ToString();
        var paramMember = AnalyzerHelpers.TryResolveEventIdMemberName(paramDefaultValue, aielType)
                           ?? paramDefaultValue.ToString();

        var location = eventIdParam.Locations.FirstOrDefault()
                       ?? method.Locations.FirstOrDefault()
                       ?? Location.None;

        ctx.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.EventIdMismatch,
            location,
            $"AielEventIds.{attrMember}",
            $"AielEventIds.{paramMember}"));
    }
}

