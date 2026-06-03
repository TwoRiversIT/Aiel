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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Aiel.Logging.Analyzers;

/// <summary>
/// AIEL003 – The <c>Message</c> argument of every <c>[LoggerMessage]</c>
/// attribute must contain the <c>[{EventId}]</c> placeholder so that
/// structured log consumers can correlate events by ID.
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

        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    // ── Core analysis ────────────────────────────────────────────────────

    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext ctx)
    {
        var attrNode = (AttributeSyntax)ctx.Node;

        // Only interested in [LoggerMessage] attributes.
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

        // Find the Message argument (named wins; positional at index 2 for the
        // common overload: LoggerMessage(eventId, level, message)).
        var messageArg = FindMessageArgument(attrNode);
        if (messageArg is null)
        {
            return;
        }

        // Resolve the string constant value of the message expression.
        var constant = ctx.SemanticModel
            .GetConstantValue(messageArg.Expression, ctx.CancellationToken);

        if (!constant.HasValue || constant.Value is not string messageText)
        {
            return; // Can't evaluate – skip rather than raise false positives
        }

        // Check for the required placeholder (case-sensitive per Aiel convention).
        if (messageText.Contains(WellKnownTypes.EventIdPlaceholder,
                StringComparison.Ordinal))
        {
            return; // ← compliant
        }

        ctx.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.MissingEventIdInMessage,
            messageArg.Expression.GetLocation(),
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

        // Named argument: Message = "..."
        foreach (var arg in args)
        {
            if (arg.NameEquals?.Name.Identifier.ValueText == WellKnownTypes.MessageArgName)
            {
                return arg;
            }
        }

        // Positional: index 2 in the (eventId, level, message) constructor overload.
        // Guard: only treat it as the message slot when it's a string literal.
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

