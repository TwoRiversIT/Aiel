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
// EventIdMismatchAnalyzer.cs  –  AIEL00012
//
// Reports when the integer EventId in [LoggerMessage] does not match the
// default value of the method's optional <ConfiguredType> eventId parameter.
//
// Uses AnalyzerConfiguration so the check works with any EventIds enum,
// not just Aiel.Logging.AielEventIds.
// -----------------------------------------------------------------------

using Aiel.Logging.Configuration;
using Aiel.Logging.Internal;
using Aiel.Internal;
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

        context.RegisterCompilationStartAction(compilationCtx =>
        {
            var config = AnalyzerConfiguration.Resolve(compilationCtx.Options);

            compilationCtx.RegisterSymbolAction(
                ctx => AnalyzeMethod(ctx, config),
                SymbolKind.Method);
        });
    }

    // ── Core analysis ────────────────────────────────────────────────────

    private static void AnalyzeMethod(SymbolAnalysisContext ctx, EventIdsTypeConfig config)
    {
        var method = (IMethodSymbol)ctx.Symbol;

        var attrData = AnalyzerHelpers.GetLoggerMessageAttribute(method, ctx.Compilation);
        if (attrData is null)
        {
            return;
        }

        config = ctx.ResolveConfigForSymbol(method, config);

        var eventIdsType = config.GetTypeSymbol(ctx.Compilation);
        if (eventIdsType is null)
        {
            return;
        }

        // ── 1. Read EventId from the attribute ───────────────────────────
        var attrArg = AnalyzerHelpers.GetNamedArgument(attrData, WellKnownTypes.EventIdArgName);
        if (attrArg?.Value is not Int32 attrEventIdValue)
        {
            return;
        }

        // ── 2. Find the optional eventId parameter ───────────────────────
        IParameterSymbol? eventIdParam = null;
        foreach (var p in method.Parameters)
        {
            if (SymbolEqualityComparer.Default.Equals(p.Type, eventIdsType)
                && String.Equals(p.Name, WellKnownTypes.EventIdParamName,
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

        if (eventIdParam.ExplicitDefaultValue is not Int32 paramDefault)
        {
            return;
        }

        // ── 3. Compare ───────────────────────────────────────────────────
        if (attrEventIdValue == paramDefault)
        {
            return;
        }

        var attrMember = AnalyzerHelpers.TryResolveMemberName(attrEventIdValue, eventIdsType)
                          ?? attrEventIdValue.ToString();
        var paramMember = AnalyzerHelpers.TryResolveMemberName(paramDefault, eventIdsType)
                          ?? paramDefault.ToString();

        var location = eventIdParam.Locations.FirstOrDefault()
                       ?? method.Locations.FirstOrDefault()
                       ?? Location.None;

        var props = AnalyzerConfiguration.BuildDiagnosticProperties(config);

        ctx.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.EventIdMismatch,
            location,
            properties: props,
            $"{config.ShortName}.{attrMember}",
            $"{config.ShortName}.{paramMember}"));
    }
}
