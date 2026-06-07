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
// MissingEventIdInMessageAnalyzer.cs  –  AIEL00010
//
// Reports when a [LoggerMessage] message template does not contain the
// "[{EventId}]" placeholder.  This rule does not inspect the EventIds
// enum type — the placeholder is fixed regardless of configuration.
// -----------------------------------------------------------------------

using Aiel.Logging.Configuration;
using Aiel.Logging.Internal;
using Aiel.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Aiel.Logging.Analyzers;

/// <summary>
/// AIEL00010 – The <c>Message</c> argument of every <c>[LoggerMessage]</c>
/// attribute must contain the <c>[{EventId}]</c> placeholder.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingEventIdInMessageAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [DiagnosticDescriptors.MissingEventIdInMessage];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Wrap in CompilationStart so configuration is resolved consistently
        // with the other analyzers, even though AIEL00010 doesn't need it.
        context.RegisterCompilationStartAction(compilationCtx =>
        {
            var config = AnalyzerConfiguration.Resolve(compilationCtx.Options);

            compilationCtx.RegisterSyntaxNodeAction(
                ctx => AnalyzeAttribute(ctx, config),
                SyntaxKind.Attribute);
        });
    }

    // ── Core analysis ────────────────────────────────────────────────────

    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext ctx, EventIdsTypeConfig config)
    {
        var attrNode = (AttributeSyntax)ctx.Node;

        var attrSymbol = ctx.SemanticModel
            .GetSymbolInfo(attrNode, ctx.CancellationToken)
            .Symbol?.ContainingType;

        if (attrSymbol?.ToDisplayString() != WellKnownTypes.LoggerMessageAttr)
        {
            return;
        }

        if (attrNode.ArgumentList is null)
        {
            return;
        }

        var messageArg = FindMessageArgument(attrNode);
        if (messageArg is null)
        {
            return;
        }

        var constant = ctx.SemanticModel
            .GetConstantValue(messageArg.Expression, ctx.CancellationToken);

        if (!constant.HasValue || constant.Value is not String messageText)
        {
            return;
        }

        if (messageText.StartsWith(WellKnownTypes.EventIdPlaceholder, StringComparison.Ordinal))
        {
            return; // compliant
        }

        // Attach config props so the code fix can echo them back if needed.
        var props = AnalyzerConfiguration.BuildDiagnosticProperties(config);

        ctx.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.MissingEventIdInMessage,
            messageArg.Expression.GetLocation(),
            properties: props,
            messageText));
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static AttributeArgumentSyntax? FindMessageArgument(AttributeSyntax attr)
    {
        if (attr.ArgumentList is null)
        {
            return null;
        }

        var args = attr.ArgumentList.Arguments;

        foreach (var arg in args)
        {
            if (arg.NameEquals?.Name.Identifier.ValueText == WellKnownTypes.MessageArgName)
            {
                return arg;
            }
        }

        // Positional index 2: (eventId, level, message)
        if (args.Count >= 3)
        {
            var candidate = args[2];
            if (candidate.NameEquals is null
                && candidate.Expression is LiteralExpressionSyntax lit
                && lit.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return candidate;
            }
        }

        return null;
    }
}
