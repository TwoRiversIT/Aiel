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
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Composition;

namespace Aiel.Logging.CodeFixes;

/// <summary>
/// Fixes AIEL001 by replacing a raw integer literal with
/// <c>(int)AielEventIds.MemberName</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseAielEventIdsCodeFix))]
[Shared]
public sealed class UseAielEventIdsCodeFix : CodeFixProvider
{
    private const string Title = "Replace with (int)AielEventIds member";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds
        => [DiagnosticDescriptors.UseAielEventIds.Id];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var span = diagnostic.Location.SourceSpan;
        var node = root.FindNode(span);

        // The reported node should be the expression used as EventId value.
        var expr = node as ExpressionSyntax
            ?? node.DescendantNodesAndSelf().OfType<ExpressionSyntax>()
                .FirstOrDefault(e => e.Span == span)
            ?? root.FindToken(span.Start).Parent?.AncestorsAndSelf()
                .OfType<ExpressionSyntax>()
                .FirstOrDefault(e => e.Span.Contains(span));

        if (expr is null)
        {
            return;
        }

        // Extract the suggested member name from the diagnostic message args.
        // messageFormat: "EventId '{0}' is a raw integer. Use '(int)AielEventIds.{1}' instead."
        var props = diagnostic.Properties;
        var memberName = GetSuggestedMember(diagnostic, context.Document, expr);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: ct =>
                    ReplaceWithEnumCastAsync(context.Document, expr, memberName, ct),
                equivalenceKey: Title),
            diagnostic);
    }

    // ── Transformation ───────────────────────────────────────────────────

    private static async Task<Document> ReplaceWithEnumCastAsync(
        Document document,
        ExpressionSyntax expr,
        string memberName,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // Build:  (int)AielEventIds.MemberName
        var castExpr = SyntaxFactory.CastExpression(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(WellKnownTypes.AielEventIdsShort),
                SyntaxFactory.IdentifierName(memberName)))
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newRoot = root.ReplaceNode(expr, castExpr);
        return document.WithSyntaxRoot(newRoot);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static string GetSuggestedMember(
        Diagnostic diagnostic,
        Document document,
        ExpressionSyntax expr)
    {
        // The second message argument is the suggested member name;
        // try to recover it from the formatted message.
        var msg = diagnostic.GetMessage();

        // Pattern: "Use '(int)AielEventIds.{member}' instead."
        const string prefix = "(int)AielEventIds.";
        const string suffix = "' instead.";
        var start = msg.IndexOf(prefix, StringComparison.Ordinal);
        if (start >= 0)
        {
            start += prefix.Length;
            var end = msg.IndexOf(suffix, start, StringComparison.Ordinal);
            if (end > start)
            {
                return msg.Substring(start, end - start);
            }
        }

        return "SomeMember";
    }
}

